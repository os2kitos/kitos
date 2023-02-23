using Core.DomainModel.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Notification.Write;
using Core.DomainModel.Advice;
using Core.DomainServices.Queries;
using Core.ApplicationServices.Model.Notification;

namespace Core.ApplicationServices.Notification
{
    public interface INotificationService
    {
        Result<IQueryable<Advice>, OperationError> GetNotifications(Guid organizationUuid, params IDomainQuery<Advice>[] conditions);
        Result<Advice, OperationError> GetNotificationByUuid(Guid uuid, Guid relatedEntityUuid, RelatedEntityType relatedEntityType);
        Result<IEnumerable<AdviceSent>, OperationError> GetNotificationSentByUuid(Guid uuid, Guid relatedEntityUuid, RelatedEntityType relatedEntityType);
        Result<Advice, OperationError> CreateImmediateNotification(ImmediateNotificationModificationParameters parameters);
        Result<Advice, OperationError> CreateScheduledNotification(CreateScheduledNotificationModificationParameters parameters);
        Result<Advice, OperationError> UpdateScheduledNotification(Guid notificationUuid, UpdateScheduledNotificationModificationParameters parameters);
        Result<Advice, OperationError> DeactivateNotification(Guid notificationUuid, Guid relatedEntityUuid, RelatedEntityType relatedEntityType);
        Maybe<OperationError> DeleteNotification(Guid notificationUuid, Guid relatedEntityUuid, RelatedEntityType relatedEntityType);
        Result<NotificationPermissions, OperationError> GetPermissions(Guid notificationUuid, Guid relatedEntityUuid, RelatedEntityType relatedEntityType);
    }
}
