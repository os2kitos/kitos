using Core.DomainModel.Notification;
using Core.DomainModel.Result;
using Core.DomainModel.Shared;
using Infrastructure.Services.Types;
using System.Collections.Generic;
using System.Linq;

namespace Core.DomainServices.Notifications
{
    public interface IUserNotificationService
    {
        public Result<UserNotification, OperationError> AddUserNotification(int organizationId, int userToNotifyId, string name, string message, int RelatedEntityId, RelatedEntityType RelatedEntityType, NotificationType notificationType);
        public Result<IQueryable<UserNotification>, OperationError> GetNotificationsForUser(int organizationId, int userId, RelatedEntityType relatedEntityType);
        public Maybe<OperationError> Delete(int id);
        public Result<UserNotification, OperationError> GetUserNotification(int id);
        void BulkDeleteUserNotification(IEnumerable<UserNotification> toBeDeleted);
    }
}
