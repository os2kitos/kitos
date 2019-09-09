using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Result;
using Core.ApplicationServices.Model.SystemUsage.Migration;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;
using Infrastructure.Services.DataAccess;

namespace Core.ApplicationServices.SystemUsage.Migration
{
    public class ItSystemUsageMigrationService : IItSystemUsageMigrationService
    {
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IGenericRepository<ItSystem> _itSystemRepository;
        private readonly IGenericRepository<ItSystemUsage> _itSystemUsageRepository;
        private readonly IGenericRepository<ItContract> _itContractRepository;
        private readonly ITransactionManager _transactionManager;


        public ItSystemUsageMigrationService(
            IAuthorizationContext authorizationContext,
            IGenericRepository<ItSystem> itSystemRepository,
            IGenericRepository<ItSystemUsage> itSystemUsageRepository,
            IGenericRepository<ItContract> itContractRepository,
            ITransactionManager transactionManager)
        {
            _authorizationContext = authorizationContext;
            _itSystemRepository = itSystemRepository;
            _itSystemUsageRepository = itSystemUsageRepository;
            _itContractRepository = itContractRepository;
            _transactionManager = transactionManager;
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


            var accessLevel = _authorizationContext.GetOrganizationReadAccessLevel(organizationId);

            if (accessLevel < OrganizationDataReadAccessLevel.Public)
            {
                return Result<OperationResult, IReadOnlyList<ItSystem>>.Fail(OperationResult.Forbidden);
            }

            var idsOfSystemsInUse = GetIdsOfItSystemsInUseByOrganizationId(organizationId);
            var unusedItSystems = GetUnusedItSystems(idsOfSystemsInUse, organizationId, nameContent, numberOfItSystems,
                getPublicFromOtherOrganizations);

            return Result<OperationResult, IReadOnlyList<ItSystem>>.Ok(unusedItSystems);
        }

        public Result<OperationResult, ItSystemUsageMigration> GetSystemUsageMigration(int usageSystemId, int toSystemId)
        {
            if (!CanExecuteMigration())
            {
                return Result<OperationResult, ItSystemUsageMigration>.Fail(OperationResult.Forbidden);
            }

            var itSystemUsage = _itSystemUsageRepository.GetByKey(usageSystemId);
            if (!_authorizationContext.AllowReads(itSystemUsage))
            {
                return Result<OperationResult, ItSystemUsageMigration>.Fail(OperationResult.Forbidden);
            }
            var toItSystem = _itSystemRepository.GetByKey(toSystemId);
            if (!_authorizationContext.AllowReads(toItSystem))
            {
                return Result<OperationResult, ItSystemUsageMigration>.Fail(OperationResult.Forbidden);
            }

            var fromItSystem = itSystemUsage.ItSystem;
            var affectedItProjects = itSystemUsage.ItProjects;
            var usageContractIds= itSystemUsage.Contracts.Select(x => x.ItContractId).ToList();
            var interfaceExhibitUsages = itSystemUsage.ItInterfaceExhibitUsages;
            var interfaceUsages = itSystemUsage.ItInterfaceUsages;

            var affectedContracts = GetContractMigrations(usageContractIds, interfaceExhibitUsages, interfaceUsages);

            return Result<OperationResult, ItSystemUsageMigration>.Ok(
                new ItSystemUsageMigration(itSystemUsage, fromItSystem, toItSystem, affectedItProjects, affectedContracts));
        }

        public Result<OperationResult, ItSystemUsage> ExecuteSystemUsageMigration(int usageSystemId, int toSystemId)
        {
            using (var transaction = _transactionManager.Begin(IsolationLevel.Serializable))
            {
                try
                {
                    var migration = GetSystemUsageMigration(usageSystemId, toSystemId);
                    var systemUsage = migration.ResultValue.ItSystemUsage;
                    systemUsage.ItSystemId = toSystemId;
                    systemUsage.ItInterfaceExhibitUsages = new List<ItInterfaceExhibitUsage>();
                    _itSystemUsageRepository.Update(systemUsage);
                    _itSystemRepository.Save();

                    transaction.Commit();
                    return Result<OperationResult, ItSystemUsage>.Ok(systemUsage);
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    //TODO Logger
                    throw;
                }
                
            }

        }

        public bool CanExecuteMigration()
        {
            return _authorizationContext.AllowSystemUsageMigration();
        }

        private IReadOnlyList<int> GetIdsOfItSystemsInUseByOrganizationId(int organizationId)
        {
            return _itSystemUsageRepository
                .AsQueryable()
                .ByOrganizationId(organizationId)
                .Select(x => x.ItSystemId)
                .ToList()
                .AsReadOnly();
        }

        private IReadOnlyList<ItSystem> GetUnusedItSystems(
            IReadOnlyList<int> idsOfSystemsInUse,
            int organizationId,
            string nameContent,
            int numberOfItSystems,
            bool getPublicFromOtherOrganizations)
        {
            var crossLevelAccess = _authorizationContext.GetCrossOrganizationReadAccess();
            var organizationAccess = _authorizationContext.GetOrganizationReadAccessLevel(organizationId);
            var unusedItSystems = _itSystemRepository.AsQueryable();
            unusedItSystems = getPublicFromOtherOrganizations
                ? unusedItSystems.ByOrganizationDataAndPublicDataFromOtherOrganizations(organizationId,
                    organizationAccess, crossLevelAccess)
                : unusedItSystems.ByOrganizationId(organizationId, organizationAccess);
            unusedItSystems = unusedItSystems
                .ExceptEntitiesWithIds(idsOfSystemsInUse)
                .ByPartOfName(nameContent)
                .OrderBy(x => x.Name)
                .Take(numberOfItSystems);
            return unusedItSystems.ToList().AsReadOnly();
        }

        private IReadOnlyList<ItContractMigration> GetContractMigrations(
            IEnumerable<int> contracts,
            IEnumerable<ItInterfaceExhibitUsage> interfaceExhibitUsage,
            IEnumerable<ItInterfaceUsage> itInterfaceUsage)
        {
            var itInterfaceExhibitUsages = interfaceExhibitUsage.ToList();
            var itInterfaceUsages = itInterfaceUsage.ToList();
            var contractsAsList = contracts.ToList();

            var allContractIds = contractsAsList
                .Concat(itInterfaceExhibitUsages.Select(x => x.ItContract.Id))
                .Concat(itInterfaceUsages.Select(x => x.ItContract.Id))
                .Distinct()
                .ToList();

            var allContracts = _itContractRepository
                .AsQueryable()
                .ByIds(allContractIds)
                .ToList();

            return allContracts
                .Select(contract => CreateContractMigration(itInterfaceExhibitUsages, itInterfaceUsages, contract, contractsAsList.Contains(contract.Id)))
                .ToList()
                .AsReadOnly();
        }

        private static ItContractMigration CreateContractMigration(
            IEnumerable<ItInterfaceExhibitUsage> interfaceExhibitUsage,
            IEnumerable<ItInterfaceUsage> itInterfaceUsage,
            ItContract contract,
            bool systemAssociatedInContract)
        {
            return new ItContractMigration(
                contract,
                systemAssociatedInContract,
                itInterfaceUsage.Where(x => x.ItContractId == contract.Id),
                interfaceExhibitUsage.Where(x => x.ItContractId == contract.Id));
        }

    }
}