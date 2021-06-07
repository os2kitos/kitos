using Core.ApplicationServices.Authorization;
using Core.DomainModel.Notification;
using Core.DomainModel.Result;
using Core.DomainModel.Shared;
using Core.DomainServices.Notifications;
using Infrastructure.Services.Types;
using System.Linq;

namespace Core.ApplicationServices.Notification
{
    public class UserNotificationApplicationService : IUserNotificationApplicationService
    {
        private readonly IOrganizationalUserContext _activeUserContext;
        private readonly IUserNotificationService _userNotificationService;

        public UserNotificationApplicationService(IUserNotificationService userNotificationService, IOrganizationalUserContext activeUserContext)
        {
            _userNotificationService = userNotificationService;
            _activeUserContext = activeUserContext;
        }

        public Maybe<OperationError> Delete(int id)
        {
            var getResult = _userNotificationService.GetUserNotification(id);

            if (getResult.Failed)
            {
                return getResult.Error;
            }

            var notificationToDelete = getResult.Value;

            if(!IsActiveUser(notificationToDelete.NotificationRecipientId))
            {
                return new OperationError(OperationFailure.Forbidden);
            }

            return _userNotificationService.Delete(id);
        }

        public Result<IQueryable<UserNotification>, OperationError> GetNotificationsForUser(int organizationId, int userId, RelatedEntityType relatedEntityType)
        {
            if (!IsActiveUser(userId))
                return new OperationError(OperationFailure.Forbidden);

            return _userNotificationService.GetNotificationsForUser(organizationId, userId, relatedEntityType);
        }

        private bool IsActiveUser(int userId)
        {
            return userId == _activeUserContext.UserId;
        }
    }
}
