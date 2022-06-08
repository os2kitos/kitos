using System;
using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.Organization;

namespace Core.DomainServices.Repositories.Organization
{
    public class StsOrganizationIdentityRepository : IStsOrganizationIdentityRepository
    {
        private readonly IGenericRepository<StsOrganizationIdentity> _repository;

        public StsOrganizationIdentityRepository(IGenericRepository<StsOrganizationIdentity> repository)
        {
            _repository = repository;
        }

        public Maybe<StsOrganizationIdentity> GetByExternalUuid(Guid externalId)
        {
            return
                _repository
                    .AsQueryable()
                    .Where(identity => identity.ExternalUuid == externalId)
                    .FirstOrDefault();
        }

        public Result<StsOrganizationIdentity, OperationError> AddNew(DomainModel.Organization.Organization organization, Guid externalId)
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
            var identity = new StsOrganizationIdentity(externalId, organization);
            identity = _repository.Insert(identity);
            _repository.Save();
            return identity;
        }
    }
}
