using System;
using Core.Abstractions.Types;
using Core.DomainModel.Organization;
using Infrastructure.STS.Common.Model;

namespace Core.DomainServices.Organizations
{
    public interface IStsOrganizationCompanyLookupService
    {
        Result<Guid, DetailedOperationError<StsError>> ResolveStsOrganizationCompanyUuid(Organization organization);
    }
}
