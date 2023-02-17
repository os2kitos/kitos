using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Notification;
using Core.DomainModel.Advice;
using Core.DomainModel.Shared;

namespace Core.ApplicationServices.Notification
{
    public interface IRegistrationNotificationService
    {
        IQueryable<Advice> GetCurrentUserNotifications();
        Result<IQueryable<Advice>, OperationError> GetNotificationsByOrganizationId(int organizationId);
        Maybe<Advice> GetNotificationById(int id);
        IQueryable<AdviceSent> GetSent();
        Result<Advice, OperationError> Create(NotificationModel notification);
        Result<Advice, OperationError> Update(int notificationId, UpdateNotificationModel notification);
        Maybe<OperationError> Delete(int notificationId);
        Result<Advice, OperationError> DeactivateNotification(int id);
    }
}
