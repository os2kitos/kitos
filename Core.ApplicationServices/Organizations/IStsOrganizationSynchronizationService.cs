using System;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Organizations;
using Core.DomainServices.Model.StsOrganization;

namespace Core.ApplicationServices.Organizations
{
    public interface IStsOrganizationSynchronizationService
    {
        /// <summary>
        /// Gets the synchronization details of the organization
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        Result<StsOrganizationSynchronizationDetails, OperationError> GetSynchronizationDetails(Guid organizationId);
        /// <summary>
        /// Retrieves a view of the organization as it exists in STS Organization
        /// </summary>
        /// <param name="organizationId"></param>
        /// <param name="levelsToInclude"></param>
        /// <returns></returns>
        Result<StsOrganizationUnit, OperationError> GetStsOrganizationalHierarchy(Guid organizationId, Maybe<int> levelsToInclude);
        /// <summary>
        /// Connect the organization to "STS Organisation"
        /// </summary>
        /// <param name="organizationId"></param>
        /// <param name="levelsToInclude"></param>
        /// <returns></returns>
        Maybe<OperationError> Connect(Guid organizationId, Maybe<int> levelsToInclude);
    }
}
