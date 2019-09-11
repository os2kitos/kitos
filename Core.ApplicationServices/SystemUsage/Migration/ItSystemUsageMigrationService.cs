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
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;
using Core.DomainServices.Model;
using Core.DomainServices.Repositories.Contract;
using Core.DomainServices.Repositories.System;
using Infrastructure.Services.DataAccess;
using Serilog;

namespace Core.ApplicationServices.SystemUsage.Migration
{
    public class ItSystemUsageMigrationService : IItSystemUsageMigrationService
    {
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IGenericRepository<ItSystemUsage> _itSystemUsageRepository;//TODO: We should use other services
        private readonly ITransactionManager _transactionManager;
        private readonly ILogger _logger;
        private readonly IItSystemRepository _systemsRepository;
        private readonly IItContractRepository _contractRepository;
        private readonly IInterfaceExhibitUsageService _interfaceExhibitUsageService;
        private readonly IInterfaceUsageService _interfaceUsageService;


        public ItSystemUsageMigrationService(
            IAuthorizationContext authorizationContext,
            IGenericRepository<ItSystemUsage> itSystemUsageRepository,
            ITransactionManager transactionManager,
            ILogger logger,
            IItSystemRepository systemsRepository,
            IItContractRepository contractRepository,
            IInterfaceExhibitUsageService interfaceExhibitUsageService,
            IInterfaceUsageService interfaceUsageService)
        {
            _authorizationContext = authorizationContext;
            _itSystemUsageRepository = itSystemUsageRepository;
            _transactionManager = transactionManager;
            _logger = logger;
            _systemsRepository = systemsRepository;
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
            var unusedSystems = _systemsRepository.GetUnusedSystems(new OrganizationDataQueryParameters(organizationId, queryBreadth, dataAccessLevel));

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

            var itSystemUsage = _itSystemUsageRepository.GetByKey(usageId);
            if (!_authorizationContext.AllowReads(itSystemUsage))
            {
                return Result<OperationResult, ItSystemUsageMigration>.Fail(OperationResult.Forbidden);
            }

            var toItSystem = _systemsRepository.GetSystem(toSystemId);
            if (!_authorizationContext.AllowReads(toItSystem))
            {
                return Result<OperationResult, ItSystemUsageMigration>.Fail(OperationResult.Forbidden);
            }

            //Map all contract migrations
            var usageContractIds = itSystemUsage.Contracts.Select(x => x.ItContractId).ToList();
            var interfaceExhibitUsages = itSystemUsage.ItInterfaceExhibitUsages.ToList();
            var interfaceUsages = itSystemUsage.ItInterfaceUsages.ToList();

            var contractMigrations = _contractRepository.GetBySystemUsageAssociation(usageId)
                .AsEnumerable()
                .Select(contract => CreateContractMigration(interfaceExhibitUsages, interfaceUsages, contract, usageContractIds.Contains(contract.Id)))
                .ToList()
                .AsReadOnly();

            return Result<OperationResult, ItSystemUsageMigration>.Ok(
                new ItSystemUsageMigration(
                    systemUsage: itSystemUsage,
                    fromItSystem: itSystemUsage.ItSystem,
                    toItSystem: toItSystem,
                    affectedProjects: itSystemUsage.ItProjects,
                    affectedContracts: contractMigrations));
        }

        public Result<OperationResult, ItSystemUsage> ExecuteSystemUsageMigration(int usageSystemId, int toSystemId)
        {
            using (var transaction = _transactionManager.Begin(IsolationLevel.Serializable))
            {
                try
                {
                    var migrationConsequences = GetSystemUsageMigration(usageSystemId, toSystemId);
                    if (migrationConsequences.Status != OperationResult.Ok)
                    {
                        return Result<OperationResult, ItSystemUsage>.Fail(migrationConsequences.Status);
                    }
                    var migration = migrationConsequences.ResultValue;
                    var systemUsage = migration.SystemUsage;

                    if (!_authorizationContext.AllowModify(systemUsage))
                    {
                        return Result<OperationResult, ItSystemUsage>.Fail(OperationResult.Forbidden);
                    }
                    if (systemUsage.ItSystemId == toSystemId)
                    {
                        return Result<OperationResult, ItSystemUsage>.Ok(systemUsage);
                    }
                    
                    var interfaceUsagesToBeUpdated =
                        migration.AffectedContracts.SelectMany(x => x.AffectedInterfaceUsages).Distinct();
                    var interfaceMigration = UpdateInterfaceUsages(interfaceUsagesToBeUpdated, toSystemId);
                    if (interfaceMigration != OperationResult.Ok)
                    {
                        return Result<OperationResult, ItSystemUsage>.Fail(interfaceMigration);
                    }

                    var exhibitsToBeDeleted =
                        migration.AffectedContracts.SelectMany(x => x.ExhibitUsagesToBeDeleted).Distinct();

                    var deletedStatus = DeleteExhibits(exhibitsToBeDeleted);
                    if (deletedStatus != OperationResult.Ok)
                    {
                        return Result<OperationResult, ItSystemUsage>.Fail(deletedStatus);
                    }

                    //TODO: Add ItSystemUsageRepository::Update
                    systemUsage.ItSystemId = toSystemId;
                    _itSystemUsageRepository.Update(systemUsage);
                    _itSystemUsageRepository.Save();

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

        private static ItContractMigration CreateContractMigration(
            IEnumerable<ItInterfaceExhibitUsage> interfaceExhibitUsage,
            IEnumerable<ItInterfaceUsage> itInterfaceUsage,
            ItContract contract,
            bool systemAssociatedInContract)
        {
            var itInterfaceUsages = itInterfaceUsage.Where(x => x.ItContractId == contract.Id);
            var interfaceExhibitUsages = interfaceExhibitUsage.Where(x => x.ItContractId == contract.Id);

            return new ItContractMigration(
                contract,
                systemAssociatedInContract,
                itInterfaceUsages,
                interfaceExhibitUsages);
        }
        
        private OperationResult DeleteExhibits(IEnumerable<ItInterfaceExhibitUsage> exhibitsToBeDeleted)
        {
            foreach (var itInterfaceExhibitUsage in exhibitsToBeDeleted)
            {
                var deletedStatus = _interfaceExhibitUsageService.Delete(
                    itInterfaceExhibitUsage.ItSystemUsageId, 
                    itInterfaceExhibitUsage.ItInterfaceExhibitId);
                if (deletedStatus != OperationResult.Ok)
                {
                    _logger.Error($"Deleting interface exhibit usages failed with {deletedStatus}");
                    return OperationResult.UnknownError;
                }
            }
            return OperationResult.Ok;
        }

        private OperationResult UpdateInterfaceUsages(IEnumerable<ItInterfaceUsage> usagesToBeUpdated, int toSystemId)
        {
            foreach (var interfaceUsage in usagesToBeUpdated)
            {
                var interfaceCreationResult = _interfaceUsageService.AssociateInContract(
                    interfaceUsage.ItSystemUsageId, 
                    toSystemId, 
                    interfaceUsage.ItInterfaceId,
                    interfaceUsage.ItContractId,
                    interfaceUsage.InfrastructureId,
                    interfaceUsage.IsWishedFor);
                if (interfaceCreationResult.Status != OperationResult.Ok)
                {
                    _logger.Error($"Creating new interface usages failed with {interfaceCreationResult.Status}");
                    return OperationResult.UnknownError;
                }

                var deletedStatus = _interfaceUsageService.Delete(
                    interfaceUsage.ItSystemUsageId, 
                    interfaceUsage.ItSystemId, 
                    interfaceUsage.ItInterfaceId);
                if (deletedStatus != OperationResult.Ok)
                {
                    _logger.Error($"Deleting old interface usages failed with {deletedStatus}");
                    return OperationResult.UnknownError;
                }
                
            }
            return OperationResult.Ok;
        }

    }
}