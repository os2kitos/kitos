using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.ItSystemUsage;
using Core.ApplicationServices.Model.Result;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;

namespace Core.ApplicationServices.ItSystemUsageMigration
{
    public class ItSystemUsageMigrationService : IItSystemUsageMigrationService
    {
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IGenericRepository<ItSystem> _itSystemRepository;
        private readonly IGenericRepository<ItSystemUsage> _itSystemUsageRepository;

        public ItSystemUsageMigrationService(
            IAuthorizationContext authorizationContext,
            IGenericRepository<ItSystem> itSystemRepository,
            IGenericRepository<ItSystemUsage> itSystemUsageRepository)
        {
            _authorizationContext = authorizationContext;
            _itSystemRepository = itSystemRepository;
            _itSystemUsageRepository = itSystemUsageRepository;
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

        public Result<OperationResult, Model.ItSystemUsage.ItSystemUsageMigration> GetSystemUsageMigration(int usageSystemId, int toSystemId)
        {
            //TODO
            var itSystemUsage = _itSystemUsageRepository.GetByKey(usageSystemId);
            var fromItSystem = _itSystemRepository.GetByKey(itSystemUsage.ItSystemId);
            var toItSystem = _itSystemRepository.GetByKey(toSystemId);

            var affectedContracts = itSystemUsage.Contracts;
            var affectedExhibitInterfaces = itSystemUsage.ItInterfaceExhibitUsages;
            var affectedInterfaces = itSystemUsage.ItInterfaceUsages;
            var affectedItProjects = itSystemUsage.ItProjects;

            var dummyAffectedContracts = new List<SystemUsageContractMigration>();

            return Result<OperationResult, Model.ItSystemUsage.ItSystemUsageMigration>.Ok(
                new Model.ItSystemUsage.ItSystemUsageMigration(itSystemUsage, fromItSystem, toItSystem, affectedItProjects, dummyAffectedContracts));
        }

        public Result<OperationResult, int> ExecuteSystemUsageMigration(int usageSystemId, int toSystemId)
        {
            //TODO
            //var migration = GetSystemUsageMigration(usageSystemId,toSystemId);
            
            //var itSystemUsage = _itSystemUsageRepository
            //    .Get(x => x.Id == usageSystemId);
            //var usage = itSystemUsage.First();
            //usage.ItSystemId = toSystemId;
            //_itSystemUsageRepository.Update(usage);
            //_itSystemUsageRepository.Save();
            return Result<OperationResult, int>.Ok(1);
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
    }
}