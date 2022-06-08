using System;
using Core.Abstractions.Types;
using Core.DomainServices.Model.StsOrganization;

namespace Core.ApplicationServices.Organizations
{
    public interface IStsOrganizationSynchronizationService
    {
        //TODO: Levels to import (under the root)
        /// <summary>
        /// Retrieves a view of the organization as it exists in STS Organization
        /// </summary>
        /// <param name="organizationId"></param>
        /// <param name="levelsToInclude"></param>
        /// <returns></returns>
        Result<StsOrganizationUnit, OperationError> GetStsOrganizationalHierarchy(Guid organizationId, Maybe<uint> levelsToInclude);
    }
}
