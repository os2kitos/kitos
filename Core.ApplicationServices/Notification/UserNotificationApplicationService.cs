using Core.ApplicationServices.Authorization;
using Core.DomainModel.Notification;
using Core.DomainModel.Result;
using Core.DomainModel.Shared;
using Core.DomainServices.Authorization;
using Core.DomainServices.Notifications;
using System;
using System.Collections.Generic;

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

        public Result<UserNotification, OperationError> Delete(int id)
        {
            var getResult = _userNotificationService.GetUserNotification(id);

            if (!getResult.Ok)
            {
                return getResult;
            }

            var notificationToDelete = getResult.Value;

            if(notificationToDelete.NotificationRecipientId != _activeUserContext.UserId)
            {
                return new OperationError(OperationFailure.Forbidden);
            }

            var deleteResult = _userNotificationService.Delete(id);
            if (!deleteResult)
            {
                return new OperationError(OperationFailure.UnknownError);
            }

            return notificationToDelete;
        }

        public Result<IEnumerable<UserNotification>, OperationError> GetNotificationsForUser(int organizationId, int userId, RelatedEntityType relatedEntityType)
        {
            if (userId != _activeUserContext.UserId)
                return new OperationError(OperationFailure.Forbidden);

            return _userNotificationService.GetNotificationsForUser(organizationId, userId, relatedEntityType);
        }

        public Result<int, OperationError> GetNumberOfUnresolvedNotificationsForUser(int organizationId, int userId, RelatedEntityType relatedEntityType)
        {
            if (userId != _activeUserContext.UserId)
                return new OperationError(OperationFailure.Forbidden);

            return _userNotificationService.GetNumberOfUnresolvedNotificationsForUser(organizationId, userId, relatedEntityType);
        }
    }
}
