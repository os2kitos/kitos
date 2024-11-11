using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Users;
using Core.ApplicationServices.Model.Users.Write;
using Core.DomainModel;
using System;

namespace Core.ApplicationServices.Users.Write
{
    public interface IUserWriteService
    {
        Result<User, OperationError> Create(Guid organizationUuid, CreateUserParameters parameters);

        Result<User, OperationError> Update(Guid organizationUuid, Guid userUuid, UpdateUserParameters parameters);

        Maybe<OperationError> SendNotification(Guid organizationUuid, Guid userUuid);

        Result<UserCollectionPermissionsResult, OperationError> GetCollectionPermissions(
            Guid organizationUuid);

        Maybe<OperationError> CopyUserRights(Guid organizationUuid, Guid fromUserUuid, Guid toUserUuid, UserRightsChangeParameters parameters);
        Maybe<OperationError> TransferUserRights(Guid organizationUuid, Guid fromUserUuid, Guid toUserUuid, UserRightsChangeParameters parameters);
        Maybe<OperationError> DeleteUser(Guid userUuid, Maybe<Guid> scopedToOrganizationUuid);
    }
}
