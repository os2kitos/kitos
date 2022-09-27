using System;
using Core.Abstractions.Types;
using Core.DomainModel.Organization;
using Core.DomainServices.Model.StsOrganization;

namespace Core.DomainServices.Organizations
{
    public interface IStsOrganizationService
    {
        Maybe<OperationError> ValidateConnection(Organization organization);
        Result<Guid, DetailedOperationError<ResolveOrganizationUuidError>> ResolveStsOrganizationUuid(Organization organization);
    }
}
