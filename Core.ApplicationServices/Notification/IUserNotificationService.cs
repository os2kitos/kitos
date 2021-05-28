using Core.DomainModel.Advice;
using Core.DomainModel.Notification;
using Core.DomainModel.Result;
using System.Collections.Generic;

namespace Core.ApplicationServices.Notification
{
    public interface IUserNotificationService
    {
        public Result<UserNotification, OperationError> AddUserNotification(int userToNotifyId, string name, string message, int RelatedEntityId, ObjectType RelatedEntityType, NotificationType notificationType);
        public Result<IEnumerable<UserNotification>, OperationError> GetNotificationsForUser(int organizationId, int userId);
        public Result<UserNotification, OperationError> Delete(int id);
    }
}
