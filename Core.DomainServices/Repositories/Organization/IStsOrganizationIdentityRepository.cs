using System;
using Core.Abstractions.Types;
using Core.DomainModel.Organization;

namespace Core.DomainServices.Repositories.Organization
{
    public interface IStsOrganizationIdentityRepository
    {
        Maybe<StsOrganizationIdentity> GetByExternalUuid(Guid externalId);
        Result<StsOrganizationIdentity, OperationError> AddNew(DomainModel.Organization.Organization organization, Guid externalId);
    }
}
