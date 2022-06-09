using System;
using Core.Abstractions.Types;
using Core.DomainModel.Organization;

namespace Core.DomainServices.Organizations
{
    public interface IStsOrganizationCompanyLookupService
    {
        Result<Guid, OperationError> ResolveStsOrganizationCompanyUuid(Organization organization);
    }
}
