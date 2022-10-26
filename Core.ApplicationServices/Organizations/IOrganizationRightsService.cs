using Core.Abstractions.Types;
using Core.DomainModel.Organization;
using System.Collections.Generic;

namespace Core.ApplicationServices.Organizations
{
    public interface IOrganizationRightsService
    {
        Result<OrganizationRight, OperationFailure> AssignRole(int organizationId, int userId, OrganizationRole roleId);
        Result<OrganizationRight, OperationFailure> RemoveRole(int organizationId, int userId, OrganizationRole rightId);
        Result<OrganizationRight, OperationFailure> RemoveRole(int rightId);
        Maybe<OperationError> RemoveOrganizationUnitRight(int rightId);
        Maybe<OperationError> RemoveUnitRightsByIds(IEnumerable<int> rightIds);
        Maybe<OperationError> TransferUnitRightsByIds(int targetUnitId, IEnumerable<int> rightIds);
    }
}
