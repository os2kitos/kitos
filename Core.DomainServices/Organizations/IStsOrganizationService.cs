using System;
using Core.Abstractions.Types;
using Core.DomainModel.Organization;

namespace Core.DomainServices.Organizations
{
    public interface IStsOrganizationService
    {
        //TODO: Consider specific enum for this error 
        Result<Guid, OperationError> ResolveStsOrganizationUuid(Organization organization);
    }
}
