using System;
using System.Linq;
using Core.DomainModel.Result;
using Core.DomainModel.SSO;
using Infrastructure.Services.Types;

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

        public Result<SsoOrganizationIdentity, OperationError> AddNew(DomainModel.Organization.Organization organization, Guid externalId)
        {
            if (organization == null)
            {
                throw new ArgumentNullException(nameof(organization));
            }
            var existing = GetByExternalUuid(externalId);
            if (existing.HasValue)
            {
                return new OperationError("Existing mapping already exists for UUID:{externalId}", OperationFailure.Conflict);
            }
            var identity = new SsoOrganizationIdentity(externalId, organization);
            identity = _repository.Insert(identity);
            _repository.Save();
            return identity;
        }
    }
}
