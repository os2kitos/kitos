using Core.Abstractions.Types;
using Core.DomainServices;
using Infrastructure.Services.DataAccess;
using System;

namespace Core.ApplicationServices.Users
{
    public class UserLoggedInService(IUserRepository userRepository, ITransactionManager transactionManager) : IUserLoggedInService
    {
        public Maybe<OperationError> UpdateLatestLoginWithoutAuthenticating(Guid userUuid, DateTime latestLogin)
        {
            var userMaybe = userRepository.GetByUuid(userUuid);
            if (userMaybe.IsNone) return new OperationError($"User with uuid {userUuid} not found", OperationFailure.NotFound);
            var user = userMaybe.Value;

            using var transaction = transactionManager.Begin();
            user.LastLogin = latestLogin;
            userRepository.Update(user);
            userRepository.Save();
            transaction.Commit();

            return Maybe<OperationError>.None;
        }
    }
}
