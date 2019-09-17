using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Interface.ExhibitUsage;
using Core.ApplicationServices.Interface.Usage;
using Core.ApplicationServices.Model.Result;
using Core.ApplicationServices.Model.SystemUsage.Migration;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;
using Core.DomainServices.Model;
using Core.DomainServices.Repositories.Contract;
using Core.DomainServices.Repositories.System;
using Core.DomainServices.Repositories.SystemUsage;
using Infrastructure.Services.DataAccess;
using Serilog;

namespace Core.ApplicationServices.SystemUsage.Migration
{
    public class ItSystemUsageMigrationService : IItSystemUsageMigrationService
    {
        private readonly IAuthorizationContext _authorizationContext;
        private readonly ITransactionManager _transactionManager;
        private readonly ILogger _logger;
        private readonly IItSystemRepository _systemRepository;
        private readonly IItSystemUsageRepository _systemUsageRepository;
        private readonly IItContractRepository _contractRepository;
        private readonly IInterfaceExhibitUsageService _interfaceExhibitUsageService;
        private readonly IInterfaceUsageService _interfaceUsageService;


        public ItSystemUsageMigrationService(
            IAuthorizationContext authorizationContext,
            ITransactionManager transactionManager,
            ILogger logger,
            IItSystemRepository systemRepository,
            IItSystemUsageRepository systemUsageRepository,
            IItContractRepository contractRepository,
            IInterfaceExhibitUsageService interfaceExhibitUsageService,
            IInterfaceUsageService interfaceUsageService)
        {
            _authorizationContext = authorizationContext;
            _transactionManager = transactionManager;
            _logger = logger;
            _systemRepository = systemRepository;
            _systemUsageRepository = systemUsageRepository;
            _contractRepository = contractRepository;
            _interfaceExhibitUsageService = interfaceExhibitUsageService;
            _interfaceUsageService = interfaceUsageService;
        }

        public Result<OperationResult, IReadOnlyList<ItSystem>> GetUnusedItSystemsByOrganization(
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
                throw new ArgumentException(nameof(numberOfItSystems) + $" Cannot be less than 1");
            }

            var dataAccessLevel = _authorizationContext.GetDataAccessLevel(organizationId);

            if (dataAccessLevel.CurrentOrganization < OrganizationDataReadAccessLevel.Public)
            {
                return Result<OperationResult, IReadOnlyList<ItSystem>>.Fail(OperationResult.Forbidden);
            }

            var queryBreadth = getPublicFromOtherOrganizations ? OrganizationDataQueryBreadth.IncludePublicDataFromOtherOrganizations : OrganizationDataQueryBreadth.TargetOrganization;
            var unusedSystems = _systemRepository.GetUnusedSystems(new OrganizationDataQueryParameters(organizationId, queryBreadth, dataAccessLevel));

            //Refine, order and take the amount requested
            var result = unusedSystems
                .ByPartOfName(nameContent)
                .OrderBy(x => x.Name)
                .Take(numberOfItSystems)
                .ToList()
                .AsReadOnly();

            return Result<OperationResult, IReadOnlyList<ItSystem>>.Ok(result);
        }

        public Result<OperationResult, ItSystemUsageMigration> GetSystemUsageMigration(int usageId, int toSystemId)
        {
            if (!CanExecuteMigration())
            {
                return Result<OperationResult, ItSystemUsageMigration>.Fail(OperationResult.Forbidden);
            }

            // Get usage
            var itSystemUsage = _systemUsageRepository.GetSystemUsage(usageId);
            if (itSystemUsage == null)
            {
                return Result<OperationResult, ItSystemUsageMigration>.Fail(OperationResult.BadInput);
            }
            if (!_authorizationContext.AllowReads(itSystemUsage))
            {
                return Result<OperationResult, ItSystemUsageMigration>.Fail(OperationResult.Forbidden);
            }

            // Get system
            var toItSystem = _systemRepository.GetSystem(toSystemId);
            if (toItSystem == null)
            {
                return Result<OperationResult, ItSystemUsageMigration>.Fail(OperationResult.BadInput);
            }
            if (!_authorizationContext.AllowReads(toItSystem))
            {
                return Result<OperationResult, ItSystemUsageMigration>.Fail(OperationResult.Forbidden);
            }

            // Map all contract migrations
            var contractMigrations = GetContractMigrations(itSystemUsage);

            return Result<OperationResult, ItSystemUsageMigration>.Ok(
                new ItSystemUsageMigration(
                    systemUsage: itSystemUsage,
                    fromItSystem: itSystemUsage.ItSystem,
                    toItSystem: toItSystem,
                    affectedProjects: itSystemUsage.ItProjects,
                    affectedContracts: contractMigrations));
        }

        private IEnumerable<ItContractMigration> GetContractMigrations(ItSystemUsage itSystemUsage)
        {
            //Find all contract associations by it system usage ID and then map into contract migration
            return _contractRepository
                .GetBySystemUsageAssociation(itSystemUsage.Id)
                .AsEnumerable()
                .Select(
                    contract =>
                        new ItContractMigration(
                            contract: contract,
                            systemAssociatedInContract: contract
                                .AssociatedSystemUsages
                                .Any(x => x.ItSystemUsageId == itSystemUsage.Id),
                            affectedUsages: contract
                                .AssociatedInterfaceUsages
                                .Where(x => x.ItSystemUsageId == itSystemUsage.Id)
                                .ToList(),
                            exhibitUsagesToBeDeleted: contract
                                .AssociatedInterfaceExposures
                                .Where(x => x.ItSystemUsageId == itSystemUsage.Id)
                                .ToList()
                        )
                )
                .ToList()
                .AsReadOnly();
        }

        public Result<OperationResult, ItSystemUsage> ExecuteSystemUsageMigration(int usageSystemId, int toSystemId)
        {
            if (!CanExecuteMigration())
            {
                return Result<OperationResult, ItSystemUsage>.Fail(OperationResult.Forbidden);
            }

            using (var transaction = _transactionManager.Begin(IsolationLevel.Serializable))
            {
                try
                {
                    // **********************************
                    // *** Check migration conditions ***
                    // **********************************

                    // If migration description cannot be retrieved, bail out
                    var migrationConsequences = GetSystemUsageMigration(usageSystemId, toSystemId);
                    if (migrationConsequences.Status != OperationResult.Ok)
                    {
                        return Result<OperationResult, ItSystemUsage>.Fail(migrationConsequences.Status);
                    }
                    var migration = migrationConsequences.Value;
                    var systemUsage = migration.SystemUsage;

                    //If modification of the target usage is not allowed, bail out
                    if (!_authorizationContext.AllowModify(systemUsage))
                    {
                        return Result<OperationResult, ItSystemUsage>.Fail(OperationResult.Forbidden);
                    }

                    // If target equals current system, bail out
                    if (systemUsage.ItSystemId == migration.ToItSystem.Id)
                    {
                        return Result<OperationResult, ItSystemUsage>.Ok(systemUsage);
                    }

                    // *************************
                    // *** Perform migration ***
                    // *************************
                    
                    // Migrate interface usages
                    var interfaceMigration = UpdateInterfaceUsages(migration);
                    if (interfaceMigration == false)
                    {
                        transaction.Rollback();
                        return Result<OperationResult, ItSystemUsage>.Fail(OperationResult.UnknownError);
                    }

                    // Delete interface exhibit usages
                    var deletedStatus = DeleteExhibits(migration);
                    if (deletedStatus == false)
                    {
                        transaction.Rollback();
                        return Result<OperationResult, ItSystemUsage>.Fail(OperationResult.UnknownError);
                    }

                    //Perform final switchover of "source IT-System"
                    systemUsage.ItSystemId = toSystemId;
                    _systemUsageRepository.Update(systemUsage);

                    transaction.Commit();
                    return Result<OperationResult, ItSystemUsage>.Ok(systemUsage);
                }
                catch (Exception e)
                {
                    _logger.Error(e, $"Migrating usageSystem with id: {usageSystemId}, to system with id: {toSystemId} failed");
                    transaction.Rollback();
                    return Result<OperationResult, ItSystemUsage>.Fail(OperationResult.UnknownError);
                }
            }
        }

        public bool CanExecuteMigration()
        {
            return _authorizationContext.AllowSystemUsageMigration();
        }

        private bool DeleteExhibits(ItSystemUsageMigration migration)
        {
            var exhibitsToBeDeleted = migration.AffectedContracts.SelectMany(x => x.ExhibitUsagesToBeDeleted);
            foreach (var itInterfaceExhibitUsage in exhibitsToBeDeleted)
            {
                var deletedStatus = _interfaceExhibitUsageService.Delete(
                    itInterfaceExhibitUsage.ItSystemUsageId,
                    itInterfaceExhibitUsage.ItInterfaceExhibitId);

                if (deletedStatus != OperationResult.Ok)
                {
                    _logger.Error($"Deleting interface exhibit usages failed with {deletedStatus}");
                    return false;
                }
            }
            return true;
        }

        private bool UpdateInterfaceUsages(ItSystemUsageMigration migration)
        {
            var usagesToBeUpdated = migration.AffectedContracts.SelectMany(x => x.AffectedInterfaceUsages);
            var toSystemId = migration.ToItSystem.Id;

            foreach (var interfaceUsage in usagesToBeUpdated)
            {
                var interfaceCreationResult = _interfaceUsageService.Create(
                    interfaceUsage.ItSystemUsageId,
                    toSystemId,
                    interfaceUsage.ItInterfaceId,
                    interfaceUsage.IsWishedFor,
                    interfaceUsage.ItContractId.GetValueOrDefault(),
                    interfaceUsage.InfrastructureId);

                if (interfaceCreationResult.Status != OperationResult.Ok)
                {
                    _logger.Error($"Creating new interface usages failed with {interfaceCreationResult.Status}");
                    return false;
                }

                var deletedStatus = _interfaceUsageService.Delete(
                    interfaceUsage.ItSystemUsageId,
                    interfaceUsage.ItSystemId,
                    interfaceUsage.ItInterfaceId);

                if (deletedStatus != OperationResult.Ok)
                {
                    _logger.Error($"Deleting old interface usages failed with {deletedStatus}");
                    return false;
                }

            }
            return true;
        }

    }
}