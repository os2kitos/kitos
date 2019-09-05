using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
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

        public Result<ResultStatus, IEnumerable<ItSystem>> GetUnusedItSystemsByOrganization(
            int organizationId, 
            string nameContent, 
            int numberOfItSystems, 
            bool getPublicFromOtherOrganizations)
        {

            //var idsOfSystemsInUse =
            //    _itSystemUsageRepository
            //        .AsQueryable();
            //    idsOfSystemsInUse = getPublicFromOtherOrganizations ? idsOfSystemsInUse.ByPublicAccessOrOrganizationId(organizationId) : idsOfSystemsInUse.ByOrganizationId(organizationId);


            //    .ByOrganizationId(organizationId)
                    
            //        .Select(x => x.ItSystemId)
            //        .ToList();

            var itSystems = _itSystemRepository
                .AsQueryable();
            itSystems = getPublicFromOtherOrganizations ? itSystems.ByPublicAccessOrOrganizationId(organizationId) : itSystems.ByOrganizationId(organizationId);

            itSystems = itSystems.ExceptByInUsage(organizationId)
                .ByPartOfName(nameContent)
                .OrderBy(x => x.Name)
                .Take(numberOfItSystems);

            return Result<ResultStatus, IEnumerable<ItSystem>>.Ok(itSystems);
            
            
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
    }
}
