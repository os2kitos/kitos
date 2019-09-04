using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        public Return<IEnumerable<ItSystem>> GetUnusedItSystemsByOrganization(int organizationId, string nameContent, int limit)
        {

            var itSystems = _itSystemRepository
                .AsQueryable()
                .ByPublicAccessOrOrganizationId(organizationId)
                .ExceptByInUsage(organizationId)
                .ByPartOfName(nameContent)
                .OrderBy(x => x.Name)
                .Take(limit);

            return new Return<IEnumerable<ItSystem>>
            {
                ReturnCode = ReturnType.Ok,
                ReturnValue = itSystems
            };
        }

        public Return<string> GetMigrationConflicts(int fromSystemId, int toSystemId)
        {
            throw new NotImplementedException();
        }

        public void toExecute(string input)
        {
            throw new NotImplementedException();
        }
    }
}
