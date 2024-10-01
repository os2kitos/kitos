using System;
using Core.Abstractions.Types;
using Core.DomainModel.Organization;
using System.Collections.Generic;

namespace Core.ApplicationServices.Organizations
{
    public interface IOrganizationRightsService
    {
        Result<OrganizationRight, OperationFailure> AssignRole(int organizationId, int userId, OrganizationRole role);
        Result<OrganizationRight, OperationFailure> RemoveRole(int organizationId, int userId, OrganizationRole role);
        Result<OrganizationRight, OperationFailure> RemoveRole(int rightId);
        Maybe<OperationError> RemoveUnitRightsByIds(Guid organizationUuid, Guid unitUuid, IEnumerable<int> rightIds);
        Maybe<OperationError> TransferUnitRightsByIds(Guid organizationUuid, Guid unitUuid, Guid targetUnitUuid, IEnumerable<int> rightIds);
    }
}
