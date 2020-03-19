using System;
using System.Linq;
using Core.DomainModel.Result;
using Core.DomainModel.SSO;

namespace Core.DomainServices.Repositories.SSO
{
    public class SsoUserIdentityRepository : ISsoUserIdentityRepository
    {
        private readonly IGenericRepository<SsoUserIdentity> _repository;

        public SsoUserIdentityRepository(IGenericRepository<SsoUserIdentity> repository)
        {
            _repository = repository;
        }

        public Maybe<SsoUserIdentity> GetByExternalUuid(Guid externalId)
        {
            return _repository
                .AsQueryable()
                .Where(identity => identity.ExternalUuid == externalId)
                .FirstOrDefault();
        }
    }
}
