using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Authorization.Permissions;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.SystemUsage.Migration;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;
using Core.DomainServices.Model;
using Core.DomainServices.Repositories.System;
using Core.DomainServices.Repositories.SystemUsage;
using Infrastructure.Services.DataAccess;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.SystemUsage.Relations;
using Core.DomainModel.Events;
using Core.DomainServices.Queries;
using Core.DomainServices.Queries.SystemUsage;

namespace Core.ApplicationServices.SystemUsage.Migration
{
    public class ItSystemUsageMigrationService : IItSystemUsageMigrationService
    {
        private readonly IAuthorizationContext _authorizationContext;
        private readonly ITransactionManager _transactionManager;
        private readonly ILogger _logger;
        private readonly IItSystemRepository _systemRepository;
        private readonly IItSystemUsageRepository _systemUsageRepository;
        private readonly IItsystemUsageRelationsService _itSystemUsageRelationsService;
        private readonly IDomainEvents _domainEvents;

        public ItSystemUsageMigrationService(
            IAuthorizationContext authorizationContext,
            ITransactionManager transactionManager,
            ILogger logger,
            IItSystemRepository systemRepository,
            IItSystemUsageRepository systemUsageRepository,
            IItsystemUsageRelationsService itSystemUsageRelationsService,
            IDomainEvents domainEvents)
        {
            _authorizationContext = authorizationContext;
            _transactionManager = transactionManager;
            _logger = logger;
            _systemRepository = systemRepository;
            _systemUsageRepository = systemUsageRepository;
            _itSystemUsageRelationsService = itSystemUsageRelationsService;
            _domainEvents = domainEvents;
        }

        public Result<IQueryable<ItSystem>, OperationError> GetUnusedItSystemsByOrganizationQuery(
            int organizationId,
            int numberOfItSystems,
            bool getPublicFromOtherOrganizations,
            params IDomainQuery<ItSystem>[] conditions)
        {
            if (numberOfItSystems is < 1 or > 25)
            {
                return new OperationError($"{nameof(numberOfItSystems)} must satisfy constraint: 1 <= n <= 25", OperationFailure.BadInput);
            }

            var dataAccessLevel = _authorizationContext.GetDataAccessLevel(organizationId);

            if (dataAccessLevel.CurrentOrganization < OrganizationDataReadAccessLevel.Public)
            {
                return new OperationError("User is not allowed to access organization", OperationFailure.Forbidden);
            }

            var subQueries = new List<IDomainQuery<ItSystem>>();

            subQueries.AddRange(conditions);

            var queryBreadth = getPublicFromOtherOrganizations ? OrganizationDataQueryBreadth.IncludePublicDataFromOtherOrganizations : OrganizationDataQueryBreadth.TargetOrganization;
            var baseQuery = _systemRepository.GetUnusedSystems(new OrganizationDataQueryParameters(organizationId, queryBreadth, dataAccessLevel));

            var result = subQueries.Any()
                ? new IntersectionQuery<ItSystem>(subQueries).Apply(baseQuery)
                : baseQuery;

            var finalQuery = result
                .Where(x => x.Disabled == false)
                .OrderBy(x => x.Name)
                .Take(numberOfItSystems);

            return Result<IQueryable<ItSystem>, OperationError>.Success(finalQuery);
        }

        public Result<IReadOnlyList<ItSystem>, OperationError> GetUnusedItSystemsByOrganization(
            int organizationId,
            string nameContent,
            int numberOfItSystems,
            bool getPublicFromOtherOrganizations)
        {
            if (string.IsNullOrWhiteSpace(nameContent))
            {
                return new List<ItSystem>();
            }

            var conditions = new List<IDomainQuery<ItSystem>>
            {
                new QueryByPartOfName<ItSystem>(nameContent)
            };
            
            return GetUnusedItSystemsByOrganizationQuery(organizationId, numberOfItSystems, getPublicFromOtherOrganizations, conditions.ToArray())
                .Select<IReadOnlyList<ItSystem>>(unusedSystems => unusedSystems.ToList().AsReadOnly());
        }

        public Result<ItSystemUsageMigration, OperationError> GetSystemUsageMigration(int usageId, int toSystemId)
        {
            if (!CanExecuteMigration())
            {
                return new OperationError("User doesn't have proper permissions to access the migration", OperationFailure.Forbidden);
            }

            // Get usage
            var itSystemUsage = _systemUsageRepository.GetSystemUsage(usageId);
            if (itSystemUsage == null)
            {
                return new OperationError($"ItSystemUsage with id: {usageId} was not found", OperationFailure.NotFound);
            }
            if (!_authorizationContext.AllowReads(itSystemUsage))
            {
                return new OperationError($"User doesn't have read access to it system usage with id: {usageId}", OperationFailure.Forbidden);
            }

            // Get system
            var toItSystem = _systemRepository.GetSystem(toSystemId);
            if (toItSystem == null)
            {
                return new OperationError($"Target ItSystem with id: {toSystemId} was not found", OperationFailure.NotFound);
            }
            if (!_authorizationContext.AllowReads(toItSystem))
            {
                return new OperationError($"User doesn't have read access to it system with id: {usageId}", OperationFailure.Forbidden);
            }

            if (toItSystem.Disabled)
            {
                return new OperationError("Target ItSystem cannot be disabled", OperationFailure.BadState);
            }

            // Get contracts
            var contracts = itSystemUsage.Contracts.Select(x => x.ItContract).ToList();

            // Map relations
            var relationMigrations = GetRelationMigrations(itSystemUsage);

            // Data processing registrations
            var dprs = itSystemUsage.AssociatedDataProcessingRegistrations.ToList();

            return new ItSystemUsageMigration(
                systemUsage: itSystemUsage,
                fromItSystem: itSystemUsage.ItSystem,
                toItSystem: toItSystem,
                affectedContracts: contracts,
                affectedRelations: relationMigrations,
                affectedDataProcessingRegistrations: dprs);
        }

        public Result<ItSystemUsage, OperationError> ExecuteSystemUsageMigration(int usageSystemId, int toSystemId)
        {
            if (!CanExecuteMigration())
            {
                return new OperationError("User doesn't have proper permissions to access the migration", OperationFailure.Forbidden);
            }

            using var transaction = _transactionManager.Begin();

            try
            {
                // **********************************
                // *** Check migration conditions ***
                // **********************************

                // If migration description cannot be retrieved, bail out
                var migrationConsequences = GetSystemUsageMigration(usageSystemId, toSystemId);
                if (migrationConsequences.Ok == false)
                {
                    return migrationConsequences.Error;
                }
                var migration = migrationConsequences.Value;
                var systemUsage = migration.SystemUsage;
                var oldSystem = migration.FromItSystem;
                var newSystem = migration.ToItSystem;

                //If modification of the target usage is not allowed, bail out
                if (!_authorizationContext.AllowModify(systemUsage))
                {
                    return new OperationError($"User is not allowed to modify it system usage with id: {usageSystemId}", OperationFailure.Forbidden);
                }

                // If target equals current system, bail out
                if (systemUsage.ItSystemId == migration.ToItSystem.Id)
                {
                    return systemUsage;
                }

                // *************************
                // *** Perform migration ***
                // *************************

                // Delete UsedByRelation interfaces
                var relationsMigrated = PerformRelationMigrations(migration);
                if (relationsMigrated == false)
                {
                    transaction.Rollback();
                    return new OperationError("Unknown error occurred while performing the relation migration", OperationFailure.UnknownError);
                }
                //***********************************************
                //Perform final switchover of "source IT-System"
                //***********************************************

                // Switch the ID
                systemUsage.ItSystemId = toSystemId;
                _systemUsageRepository.Update(systemUsage);

                //Raise events for all affected roots
                _domainEvents.Raise(new EntityUpdatedEvent<ItSystemUsage>(systemUsage));
                _domainEvents.Raise(new EntityUpdatedEvent<ItSystem>(oldSystem));
                _domainEvents.Raise(new EntityUpdatedEvent<ItSystem>(newSystem));
                transaction.Commit();
                return systemUsage;
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Migrating usageSystem with id: {usageSystemId}, to system with id: {toSystemId} failed");
                transaction.Rollback();
                return new OperationError($"Unknown error occurred while performing the usage migration with id: {usageSystemId} to system with id: {toSystemId}", OperationFailure.UnknownError);
            }
        }

        public bool CanExecuteMigration()
        {
            return _authorizationContext.HasPermission(new SystemUsageMigrationPermission());
        }

        private static IEnumerable<SystemRelation> GetRelationMigrations(ItSystemUsage fromSystemUsage)
        {
            return fromSystemUsage
                .UsedByRelations
                .AsEnumerable()
                .Where(x => x.RelationInterfaceId != null)
                .ToList()
                .AsReadOnly();
        }

        private bool PerformRelationMigrations(ItSystemUsageMigration migration)
        {
            foreach (var relation in migration.AffectedSystemRelations)
            {
                var modifyStatus = _itSystemUsageRelationsService.ModifyRelation(
                    relation.FromSystemUsageId,
                    relation.Id,
                    relation.ToSystemUsageId,
                    relation.Description,
                    relation.Reference,
                    null,
                    relation.AssociatedContractId,
                    relation.UsageFrequencyId);

                if (modifyStatus.Failed)
                {
                    _logger.Error("Deleting interface from relation with id {relationId} failed with {error}", relation.Id, modifyStatus.Error);
                    return false;
                }
            }
            return true;
        }
    }
}