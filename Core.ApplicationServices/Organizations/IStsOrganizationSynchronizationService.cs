﻿using System;
using Core.Abstractions.Types;
using Core.DomainServices.Model.StsOrganization;

namespace Core.ApplicationServices.Organizations
{
    public interface IStsOrganizationSynchronizationService
    {
        /// <summary>
        /// Validates if KITOS can read organization data from STS Organisation
        Maybe<DetailedOperationError<CheckConnectionError>> ValidateConnection(Guid organizationId);
        /// <summary>
        /// Retrieves a view of the organization as it exists in STS Organization
        /// </summary>
        /// <param name="organizationId"></param>
        /// <param name="levelsToInclude"></param>
        /// <returns></returns>
        Result<StsOrganizationUnit, OperationError> GetStsOrganizationalHierarchy(Guid organizationId, Maybe<uint> levelsToInclude);
    }
}
