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

        Result<UserCollectionPermissionsResult, OperationError> GetCollectionPermissions(
            Guid organizationUuid);
    }
}
