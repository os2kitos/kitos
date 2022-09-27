using System;
using Core.Abstractions.Types;
using Core.DomainServices.Model.StsOrganization;

namespace Core.ApplicationServices.Organizations
{
    public interface IStsOrganizationSynchronizationService
    {
        //TODO: More detailed error
        /// <summary>
        /// TODO: Description
        /// </summary>
        /// <typeparam name="StsOrganizationUnit"></typeparam>
        /// <typeparam name="OperationError"></typeparam>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <returns></returns>
        Maybe<OperationError> ValidateConnection(Guid organizationId);
        /// <summary>
        /// Retrieves a view of the organization as it exists in STS Organization
        /// </summary>
        /// <param name="organizationId"></param>
        /// <param name="levelsToInclude"></param>
        /// <returns></returns>
        Result<StsOrganizationUnit, OperationError> GetStsOrganizationalHierarchy(Guid organizationId, Maybe<uint> levelsToInclude);
    }
}
