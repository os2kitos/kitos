using Core.DomainModel.Notification;
using Core.DomainModel.Result;
using Core.DomainModel.Shared;
using Infrastructure.Services.Types;
using System.Linq;

namespace Core.ApplicationServices.Notification
{
    public interface IUserNotificationApplicationService
    {
        public Maybe<OperationError> Delete(int id);
        public Result<IQueryable<UserNotification>, OperationError> GetNotificationsForUser(int organizationId, int userId, RelatedEntityType relatedEntityType);
    }
}
