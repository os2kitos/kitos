using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices;

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

        public Return<IEnumerable<ItSystem>> GetUnusedItSystemsByOrganization(int organizationId, string q, int limit)
        {
            if (!_authorizationContext.AllowReadsWithinOrganization(organizationId))
            {
                return new Return<IEnumerable<ItSystem>>
                {
                    StatusCode = HttpStatusCode.Forbidden
                };
            }
            var itSystems = _itSystemRepository.Get(s =>
                    s.Name.Contains(q) &&
                    s.OrganizationId.Equals(organizationId) &&
                    (s.Usages.Count == 0 || s.Usages.Count(x => x.OrganizationId == organizationId) == 0)
            );

            return new Return<IEnumerable<ItSystem>>
            {
                StatusCode = HttpStatusCode.OK,
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
