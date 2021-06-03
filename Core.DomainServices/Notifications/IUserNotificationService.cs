using Core.DomainModel.Notification;
using Core.DomainModel.Result;
using Core.DomainModel.Shared;
using System.Collections.Generic;

namespace Core.DomainServices.Notifications
{
    public interface IUserNotificationService
    {
        public Result<UserNotification, OperationError> AddUserNotification(int organizationId, int userToNotifyId, string name, string message, int RelatedEntityId, RelatedEntityType RelatedEntityType, NotificationType notificationType);
        public Result<IEnumerable<UserNotification>, OperationError> GetNotificationsForUser(int organizationId, int userId, RelatedEntityType relatedEntityType);
        public bool Delete(int id);
        public Result<int, OperationError> GetNumberOfUnresolvedNotificationsForUser(int organizationId, int userId, RelatedEntityType relatedEntityType);
        public Result<UserNotification, OperationError> GetUserNotification(int id);
    }
}
