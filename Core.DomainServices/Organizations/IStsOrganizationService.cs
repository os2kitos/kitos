using System;
using Core.Abstractions.Types;
using Core.DomainModel.Organization;
using Core.DomainServices.Model.StsOrganization;

namespace Core.DomainServices.Organizations
{
    public interface IStsOrganizationService
    {
        Maybe<DetailedOperationError<CheckConnectionError>> ValidateConnection(Organization organization);
        Result<Guid, DetailedOperationError<ResolveOrganizationUuidError>> ResolveStsOrganizationUuid(Organization organization);
        /// <summary>
        /// In FK Organisation an organization may contain multiple hierarchies, but only one of them is considered "the active hierarchy".
        /// </summary>
        /// <param name="organization"></param>
        /// <returns></returns>
        Result<Guid, OperationError> ResolveOrganizationHierarchyRootUuid(Organization organization);
    }
}