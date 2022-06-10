using System;
using Core.Abstractions.Types;
using Core.DomainModel.Organization;
using Core.DomainServices.Model.StsOrganization;

namespace Core.DomainServices.Organizations
{
    public interface IStsOrganizationService
    {
        Result<Guid, DetailedOperationError<ResolveOrganizationUuidError>> ResolveStsOrganizationUuid(Organization organization);
    }
}
