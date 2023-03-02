using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Notification;
using Core.DomainModel.Advice;

namespace Core.ApplicationServices.Notification
{
    public interface IRegistrationNotificationService
    {
        Result<IQueryable<Advice>, OperationError> GetNotificationsByOrganizationId(int organizationId);
        Result<Advice, OperationError> GetNotificationById(int id);
        IQueryable<AdviceSent> GetSent();
        Result<Advice, OperationError> CreateImmediateNotification(ImmediateNotificationModel notificationModel);
        Result<Advice, OperationError> CreateScheduledNotification(ScheduledNotificationModel notificationModel);
        Result<Advice, OperationError> Update(int notificationId, UpdateScheduledNotificationModel notification);
        Maybe<OperationError> Delete(int notificationId);
        Result<Advice, OperationError> DeactivateNotification(int notificationId);
    }
}
