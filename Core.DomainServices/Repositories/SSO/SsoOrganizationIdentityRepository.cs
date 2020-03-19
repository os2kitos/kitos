using System;
using System.Linq;
using Core.DomainModel.Result;
using Core.DomainModel.SSO;

namespace Core.DomainServices.Repositories.SSO
{
    public class SsoOrganizationIdentityRepository : ISsoOrganizationIdentityRepository
    {
        private readonly IGenericRepository<SsoOrganizationIdentity> _repository;

        public SsoOrganizationIdentityRepository(IGenericRepository<SsoOrganizationIdentity> repository)
        {
            _repository = repository;
        }

        public Maybe<SsoOrganizationIdentity> GetByExternalUuid(Guid externalId)
        {
            return
                _repository
                    .AsQueryable()
                    .Where(identity => identity.ExternalUuid == externalId)
                    .FirstOrDefault();
        }
    }
}
