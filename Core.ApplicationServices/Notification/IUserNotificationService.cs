using Core.DomainModel;
using Core.DomainModel.Advice;
using Core.DomainModel.Notification;
using Core.DomainModel.Result;

namespace Core.ApplicationServices.Notification
{
    public interface IUserNotificationService
    {
        public Result<UserNotification, OperationError> AddUserNotification(int userToNotifyId, string name, string message, int RelatedEntityId, ObjectType RelatedEntityType);
    }
}
