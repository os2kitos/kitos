using Core.Abstractions.Types;
using System;

namespace Core.ApplicationServices.Users
{
    public interface IUserLoggedInService
    {
        Maybe<OperationError> UpdateLatestLoginWithoutAuthenticating(Guid userUuid, DateTime latestLogin);
    }
}
