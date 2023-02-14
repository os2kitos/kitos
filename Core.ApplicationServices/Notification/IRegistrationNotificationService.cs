using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Notification.Write;
using Core.DomainModel.Advice;

namespace Core.ApplicationServices.Notification
{
    public interface IRegistrationNotificationService
    {
        IQueryable<Advice> GetCurrentUserNotifications();
        Result<IQueryable<Advice>, OperationError> GetNotificationsByOrganizationId(int organizationId);
        Result<Advice, OperationError> GetNotificationById(int id);
        IQueryable<AdviceSent> GetSent();
        Result<Advice, OperationError> Create(int organizationId, NotificationModificationModel notification);
        Result<Advice, OperationError> Update(int organizationId, UpdateNotificationModificationModel notification);
        Maybe<OperationError> Delete(int notificationId);
        Maybe<OperationError> DeleteUserRelationByAdviceId(int notificationId);
        Result<Advice, OperationError> DeactivateNotification(int id);
    }
}
