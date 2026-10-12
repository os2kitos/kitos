using Core.ApplicationServices.Users;
using Core.DomainModel.Events;
using Serilog;
using System;

namespace Core.ApplicationServices.Authentication
{
    public class UserLoggedInEventHandler(IUserLoggedInService userLoggedInService, ILogger logger) : IDomainEventHandler<UserLoggedInEvent>
    {
        public void Handle(UserLoggedInEvent domainEvent)
        {
            var uuid = domainEvent.UserUuid;
            var now = DateTime.UtcNow;
            
            var errorMaybe = userLoggedInService.UpdateLatestLoginWithoutAuthenticating(uuid, now);

            if (errorMaybe.HasValue) logger.Error($"Error handling user login event: {errorMaybe.Value.Message}");
        }
    }
}
