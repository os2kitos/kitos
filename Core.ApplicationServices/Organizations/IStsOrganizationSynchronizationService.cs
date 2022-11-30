using System;
using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Organizations;
using Core.DomainModel.Organization;

namespace Core.ApplicationServices.Organizations
{
    public interface IStsOrganizationSynchronizationService
    {
        /// <summary>
        /// Gets the synchronization details of the organization
        /// </summary>
        /// <returns></returns>
        Result<StsOrganizationSynchronizationDetails, OperationError> GetSynchronizationDetails(Guid organizationId);
        /// <summary>
        /// Retrieves a view of the organization as it exists in STS Organization
        /// </summary>
        /// <returns></returns>
        Result<ExternalOrganizationUnit, OperationError> GetStsOrganizationalHierarchy(Guid organizationId, Maybe<int> levelsToInclude);

        /// <summary>
        /// Connect the organization to "STS Organisation"
        /// </summary>
        /// <returns></returns>
        Maybe<OperationError> Connect(Guid organizationId, Maybe<int> levelsToInclude, bool subscribeToUpdates);
        /// <summary>
        /// Disconnect the KITOS organization from STS Organisation
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        Maybe<OperationError> Disconnect(Guid organizationId);
        /// <summary>
        /// Retrieves a view of the consequences of updating the synchronized hierarchy from that which exists in STS Organization
        /// </summary>
        /// <returns></returns>
        Result<OrganizationTreeUpdateConsequences, OperationError> GetConnectionExternalHierarchyUpdateConsequences(Guid organizationId, Maybe<int> levelsToInclude);
        /// <summary>
        /// Updates the connection to the STS Organization
        /// </summary>
        /// <returns></returns>
        Maybe<OperationError> UpdateConnection(Guid organizationId, Maybe<int> levelsToInclude, bool subscribeToUpdates);
        /// <summary>
        /// Gets the last x change logs for the organization
        /// </summary>
        /// <returns></returns>
        Result<IEnumerable<IExternalConnectionChangelog>, OperationError> GetChangeLogs(Guid organizationUuid, int numberOfChangeLogs);
    }
}
