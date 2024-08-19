using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.RightsHolder;
using Core.ApplicationServices.Model.Users;
using Core.DomainModel.Organization;

namespace Core.ApplicationServices.Rights
{
    public interface IUserRightsService
    {
        Result<IEnumerable<UserRoleAssociationDTO>, OperationError> GetUsersWithRoleAssignment(OrganizationRole role);
        Result<UserRightsAssignments, OperationError> GetUserRights(int userId, int organizationId);
        Maybe<OperationError> RemoveAllRights(int userId, int organizationId);
        Maybe<OperationError> RemoveRights(int userId, int organizationId, UserRightsChangeParameters parameters);
        Maybe<OperationError> TransferRights(int fromUserId, int toUserId, int organizationId, UserRightsChangeParameters parameters);

        Maybe<OperationError> CopyRights(int fromUserId, int toUserId, int organizationId,
            UserRightsChangeParameters parameters);
    }
}
