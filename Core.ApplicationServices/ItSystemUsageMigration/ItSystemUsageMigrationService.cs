using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;

namespace Core.ApplicationServices.ItSystemUsageMigration
{
    public class ItSystemUsageMigrationService : IItSystemUsageMigrationService
    {
        private IAuthorizationContext _authorizationContext;
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

        public Result<ResultStatus, IReadOnlyList<ItSystem>> GetUnusedItSystemsByOrganization(
            int organizationId, 
            string nameContent, 
            int numberOfItSystems, 
            bool getPublicFromOtherOrganizations)
        {
            if (_authorizationContext.GetOrganizationReadAccessLevel(organizationId) < OrganizationDataReadAccessLevel.All)
            {
                return Result<ResultStatus, IReadOnlyList<ItSystem>>.Fail(ResultStatus.Forbidden);
            }

            var idsOfSystemsInUse = GetIdsOfItSystemsInUseByOrganizationId(organizationId);
            var unusedItSystems = GetUnusedItSystems(idsOfSystemsInUse, organizationId, nameContent, numberOfItSystems,
                getPublicFromOtherOrganizations);
            
            return Result<ResultStatus, IReadOnlyList<ItSystem>>.Ok(unusedItSystems);
        }

        public Result<ResultStatus, string> GetMigrationConflicts(int usageSystemId, int toSystemId)
        {
            return Result<ResultStatus, string>.Ok("Changed");
        }

        public void toExecute(string input)
        {
            //var itSystemUsage = _itSystemUsageRepository
            //    .Get(x => x.Id == usageSystemId);
            //var usage = itSystemUsage.First();
            //usage.ItSystemId = toSystemId;
            //_itSystemUsageRepository.Update(usage);
            //_itSystemUsageRepository.Save();

            throw new NotImplementedException();
        }


        private IReadOnlyList<int> GetIdsOfItSystemsInUseByOrganizationId(int organizationId)
        {
            return _itSystemUsageRepository
                    .AsQueryable()
                    .ByOrganizationId(organizationId)
                    .Select(x => x.ItSystemId)
                    .ToList();
        }

        private IReadOnlyList<ItSystem> GetUnusedItSystems(IReadOnlyList<int> exceptIds, int organizationId, string nameContent, int numberOfItSystems,
            bool getPublicFromOtherOrganizations)
        {
            var unusedItSystems = _itSystemRepository
                .AsQueryable();
            unusedItSystems = getPublicFromOtherOrganizations
                ? unusedItSystems.ByPublicAccessOrOrganizationId(organizationId)
                : unusedItSystems.ByOrganizationId(organizationId);
            unusedItSystems = unusedItSystems
                .ByEntitiesExceptWithIds(exceptIds)
                .ByPartOfName(nameContent)
                .OrderBy(x => x.Name)
                .Take(numberOfItSystems);

            return unusedItSystems.ToList();

        }
    }
}
