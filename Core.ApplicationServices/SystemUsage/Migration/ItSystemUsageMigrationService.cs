﻿using Core.ApplicationServices.Authorization;
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
using System.Data;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.SystemUsage.Relations;
using Core.DomainModel.Events;

namespace Core.ApplicationServices.SystemUsage.Migration
{
    public class ItSystemUsageMigrationService : IItSystemUsageMigrationService
    {
        private readonly IAuthorizationContext _authorizationContext;
        private readonly ITransactionManager _transactionManager;
        private readonly ILogger _logger;
        private readonly IItSystemRepository _systemRepository;
        private readonly IItSystemUsageRepository _systemUsageRepository;
        private readonly IItSystemUsageService _itSystemUsageService;
        private readonly IItsystemUsageRelationsService _itsystemUsageRelationsService;
        private readonly IDomainEvents _domainEvents;


        public ItSystemUsageMigrationService(
            IAuthorizationContext authorizationContext,
            ITransactionManager transactionManager,
            ILogger logger,
            IItSystemRepository systemRepository,
            IItSystemUsageRepository systemUsageRepository,
            IItSystemUsageService itSystemUsageService,
            IItsystemUsageRelationsService itsystemUsageRelationsService,
            IDomainEvents domainEvents)
        {
            _authorizationContext = authorizationContext;
            _transactionManager = transactionManager;
            _logger = logger;
            _systemRepository = systemRepository;
            _systemUsageRepository = systemUsageRepository;
            _itSystemUsageService = itSystemUsageService;
            _itsystemUsageRelationsService = itsystemUsageRelationsService;
            _domainEvents = domainEvents;
        }

        public Result<IReadOnlyList<ItSystem>, OperationFailure> GetUnusedItSystemsByOrganization(
            int organizationId,
            string nameContent,
            int numberOfItSystems,
            bool getPublicFromOtherOrganizations)
        {
            if (string.IsNullOrWhiteSpace(nameContent))
            {
                throw new ArgumentException(nameof(nameContent) + " must be string containing more than whitespaces");
            }
            if (numberOfItSystems < 1)
            {
                throw new ArgumentException(nameof(numberOfItSystems) + " Cannot be less than 1");
            }

            var dataAccessLevel = _authorizationContext.GetDataAccessLevel(organizationId);

            if (dataAccessLevel.CurrentOrganization < OrganizationDataReadAccessLevel.Public)
            {
                return OperationFailure.Forbidden;
            }

            var queryBreadth = getPublicFromOtherOrganizations ? OrganizationDataQueryBreadth.IncludePublicDataFromOtherOrganizations : OrganizationDataQueryBreadth.TargetOrganization;
            var unusedSystems = _systemRepository.GetUnusedSystems(new OrganizationDataQueryParameters(organizationId, queryBreadth, dataAccessLevel));

            //Refine, order and take the amount requested
            var result = unusedSystems
                .ByPartOfName(nameContent)
                .Where(x => x.Disabled == false)
                .OrderBy(x => x.Name)
                .Take(numberOfItSystems)
                .ToList()
                .AsReadOnly();

            return result;
        }

        public Result<ItSystemUsageMigration, OperationFailure> GetSystemUsageMigration(int usageId, int toSystemId)
        {
            if (!CanExecuteMigration())
            {
                return OperationFailure.Forbidden;
            }

            // Get usage
            var itSystemUsage = _systemUsageRepository.GetSystemUsage(usageId);
            if (itSystemUsage == null)
            {
                return OperationFailure.BadInput;
            }
            if (!_authorizationContext.AllowReads(itSystemUsage))
            {
                return OperationFailure.Forbidden;
            }

            // Get system
            var toItSystem = _systemRepository.GetSystem(toSystemId);
            if (toItSystem == null)
            {
                return OperationFailure.BadInput;
            }
            if (!_authorizationContext.AllowReads(toItSystem))
            {
                return OperationFailure.Forbidden;
            }

            if (toItSystem.Disabled)
            {
                return OperationFailure.BadInput;
            }

            // Get contracts
            var contracts = itSystemUsage.Contracts.Select(x => x.ItContract);

            // Map relations
            var relationMigrations = GetRelationMigrations(itSystemUsage);

            return new ItSystemUsageMigration(
                systemUsage: itSystemUsage,
                fromItSystem: itSystemUsage.ItSystem,
                toItSystem: toItSystem,
                affectedProjects: itSystemUsage.ItProjects,
                affectedContracts: contracts,
                affectedRelations: relationMigrations);
        }

        public Result<ItSystemUsage, OperationFailure> ExecuteSystemUsageMigration(int usageSystemId, int toSystemId)
        {
            if (!CanExecuteMigration())
            {
                return OperationFailure.Forbidden;
            }

            using (var transaction = _transactionManager.Begin())
            {
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
                        return OperationFailure.Forbidden;
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
                        return OperationFailure.UnknownError;
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
                    return OperationFailure.UnknownError;
                }
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
                var modifyStatus = _itsystemUsageRelationsService.ModifyRelation(
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