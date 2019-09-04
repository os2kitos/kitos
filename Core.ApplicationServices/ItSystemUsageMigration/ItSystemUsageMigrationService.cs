using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Core.DomainServices.Extensions;

namespace Core.ApplicationServices.ItSystemUsageMigration
{
    public class ItSystemUsageMigrationService : IItSystemUsageMigrationService
    {
        private IAuthorizationContext _authorizationContext;
        private readonly IGenericRepository<ItSystem> _itSystemRepository;

        public ItSystemUsageMigrationService(IAuthorizationContext authorizationContext, IGenericRepository<ItSystem> itSystemRepository)
        {
            _authorizationContext = authorizationContext;
            _itSystemRepository = itSystemRepository;
        }

        public Result<ResultStatus, IEnumerable<ItSystem>> GetUnusedItSystemsByOrganization(int organizationId, string nameContent, int numberOfItSystems, bool getPublic)
        {

            if (getPublic)
            {
                var itSystems = _itSystemRepository
                    .AsQueryable()
                    .ByPublicAccessOrOrganizationId(organizationId)
                    .ExceptByInUsage(organizationId)
                    .ByPartOfName(nameContent)
                    .OrderBy(x => x.Name)
                    .Take(numberOfItSystems);

                return Result<ResultStatus, IEnumerable<ItSystem>>.Ok(itSystems);
            }
            else
            {
                var itSystems = _itSystemRepository
                    .AsQueryable()
                    .ByOrganizationId(organizationId)
                    .ExceptByInUsage(organizationId)
                    .ByPartOfName(nameContent)
                    .OrderBy(x => x.Name)
                    .Take(numberOfItSystems);

                return Result<ResultStatus, IEnumerable<ItSystem>>.Ok(itSystems);
            }
        }

        public Result<ResultStatus, string> GetMigrationConflicts(int usageSystemId, int toSystemId)
        {
            throw new NotImplementedException();
        }

        public void toExecute(string input)
        {
            throw new NotImplementedException();
        }
    }
}
