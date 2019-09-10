using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Result;
using Core.ApplicationServices.Model.SystemUsage.Migration;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;
using Core.DomainServices.Model;
using Core.DomainServices.Repositories.System;
using Infrastructure.Services.DataAccess;
using Serilog;

namespace Core.ApplicationServices.SystemUsage.Migration
{
    public class ItSystemUsageMigrationService : IItSystemUsageMigrationService
    {
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IGenericRepository<ItSystem> _itSystemRepository;
        private readonly IGenericRepository<ItSystemUsage> _itSystemUsageRepository;
        private readonly IGenericRepository<ItContract> _itContractRepository;
        private readonly IGenericRepository<ItInterfaceExhibitUsage> _itInterfaceExhibitUsageRepository;
        private readonly IGenericRepository<ItInterfaceUsage> _itInterfaceUsageRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly ILogger _logger;
        private readonly IItSystemRepository _systemsRepository;


        public ItSystemUsageMigrationService(
            IAuthorizationContext authorizationContext,
            IGenericRepository<ItSystem> itSystemRepository,
            IGenericRepository<ItSystemUsage> itSystemUsageRepository,
            IGenericRepository<ItContract> itContractRepository,
            IGenericRepository<ItInterfaceExhibitUsage> itInterfaceExhibitUsageRepository,
            IGenericRepository<ItInterfaceUsage> itInterfaceUsageRepository,
            ITransactionManager transactionManager,
            ILogger logger,
            IItSystemRepository systemsRepository)
        {
            _authorizationContext = authorizationContext;
            _itSystemRepository = itSystemRepository;
            _itSystemUsageRepository = itSystemUsageRepository;
            _itContractRepository = itContractRepository;
            _itInterfaceExhibitUsageRepository = itInterfaceExhibitUsageRepository;
            _itInterfaceUsageRepository = itInterfaceUsageRepository;
            _transactionManager = transactionManager;
            _logger = logger;
            _systemsRepository = systemsRepository;
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
            var usageContractIds = itSystemUsage.Contracts.Select(x => x.ItContractId).ToList();
            var interfaceExhibitUsages = itSystemUsage.ItInterfaceExhibitUsages.ToList();
            var interfaceUsages = itSystemUsage.ItInterfaceUsages.ToList();

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
                    if (!_authorizationContext.AllowModify(systemUsage))
                    {
                        return Result<OperationResult, ItSystemUsage>.Fail(OperationResult.Forbidden);
                    }

                    systemUsage.ItSystemId = toSystemId;

                    var exhibitsToBeDeleted =
                        migration.ResultValue.AffectedContracts.SelectMany(x => x.ExhibitUsagesToBeDeleted).Distinct();

                    foreach (var itInterfaceExhibitUsage in exhibitsToBeDeleted)
                    {
                        _itInterfaceExhibitUsageRepository.Delete(itInterfaceExhibitUsage);
                        _itInterfaceExhibitUsageRepository.Save();
                    }

                    var interfaceUsagesToBeUpdated =
                        migration.ResultValue.AffectedContracts.SelectMany(x => x.AffectedInterfaceUsages).Distinct();

                    foreach (var interfaceUsage in interfaceUsagesToBeUpdated)
                    {
                        var key = new object[] { interfaceUsage.ItSystemUsageId, toSystemId, interfaceUsage.ItInterfaceId };
                        var item = _itInterfaceUsageRepository.GetByKey(key);

                        if (item != null)
                        {
                            _logger.Error($"Migrating failed as ItInterfaceUsage with key {{ " +
                                          $"ItSystemUsageId: {interfaceUsage.ItSystemUsageId}, " +
                                          $"ItSystemId: {toSystemId}," +
                                          $"ItInterfaceId: {interfaceUsage.ItInterfaceId} }} already exists");
                            return Result<OperationResult, ItSystemUsage>.Fail(OperationResult.Error);
                        }

                        item = _itInterfaceUsageRepository.Create();
                        item.ItSystemUsageId = interfaceUsage.ItSystemUsageId;
                        item.ItSystemId = toSystemId;
                        item.ItInterfaceId = interfaceUsage.ItInterfaceId;
                        _itInterfaceUsageRepository.Insert(item);

                        item.InfrastructureId = interfaceUsage.InfrastructureId;
                        item.IsWishedFor = interfaceUsage.IsWishedFor;
                        item.ItContractId = interfaceUsage.ItContractId;

                        _itInterfaceUsageRepository.Save();

                        _itInterfaceUsageRepository.Delete(interfaceUsage);
                        _itInterfaceUsageRepository.Save();
                    }

                    _itSystemUsageRepository.Update(systemUsage);
                    _itSystemRepository.Save();

                    transaction.Commit();
                    return Result<OperationResult, ItSystemUsage>.Ok(systemUsage);
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    _logger.Error(e, $"Migrating usageSystem with id: {usageSystemId}, to system with id: {toSystemId} failed");
                    return Result<OperationResult, ItSystemUsage>.Fail(OperationResult.Error);
                }
            }
        }

        public bool CanExecuteMigration()
        {
            return _authorizationContext.AllowSystemUsageMigration();
        }

        private IReadOnlyList<ItContractMigration> GetContractMigrations(
            IReadOnlyList<int> idsOfContractsThatHaveSystemAssociations,
            IReadOnlyList<ItInterfaceExhibitUsage> interfaceExhibitUsage,
            IReadOnlyList<ItInterfaceUsage> itInterfaceUsages)
        {
            #region to refactor service call om ContractService: GetBySystemUsage(organizationId)

            var allContractIds = 
                idsOfContractsThatHaveSystemAssociations
                .Concat(interfaceExhibitUsage.Select(x => x.ItContract.Id))
                .Concat(itInterfaceUsages.Select(x => x.ItContract.Id))
                .Distinct()
                .ToList();

            var allContracts = _itContractRepository
                .AsQueryable()
                .ByIds(allContractIds)
                .ToList();

            #endregion to refactor service call om ContractService: GetBySystemUsage(organizationId)

            return allContracts
                .Select(contract => CreateContractMigration(interfaceExhibitUsage, itInterfaceUsages, contract, idsOfContractsThatHaveSystemAssociations.Contains(contract.Id)))
                .ToList()
                .AsReadOnly();
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

    }
}