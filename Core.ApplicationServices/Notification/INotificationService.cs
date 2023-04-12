using Core.DomainModel.Shared;
using System;
using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Notification.Write;
using Core.DomainModel.Advice;
using Core.DomainServices.Queries;
using Core.ApplicationServices.Model.Notification;
using Core.ApplicationServices.Model.Notification.Read;

namespace Core.ApplicationServices.Notification
{
    public interface INotificationService
    {
        Result<IEnumerable<NotificationResultModel>, OperationError> GetNotifications(Guid organizationUuid, int? page, int? pageSize, params IDomainQuery<Advice>[] conditions);
        Result<NotificationResultModel, OperationError> GetNotificationByUuid(Guid uuid, Guid relatedEntityUuid, RelatedEntityType relatedEntityType);
        Result<IEnumerable<AdviceSent>, OperationError> GetNotificationSentByUuid(Guid uuid, Guid relatedEntityUuid, RelatedEntityType relatedEntityType);
        Result<NotificationResultModel, OperationError> CreateImmediateNotification(ImmediateNotificationModificationParameters parameters);
        Result<NotificationResultModel, OperationError> CreateScheduledNotification(CreateScheduledNotificationModificationParameters parameters);
        Result<NotificationResultModel, OperationError> UpdateScheduledNotification(Guid notificationUuid, UpdateScheduledNotificationModificationParameters parameters);
        Result<NotificationResultModel, OperationError> DeactivateNotification(Guid notificationUuid, Guid relatedEntityUuid, RelatedEntityType relatedEntityType);
        Maybe<OperationError> DeleteNotification(Guid notificationUuid, Guid relatedEntityUuid, RelatedEntityType relatedEntityType);
        Result<NotificationPermissions, OperationError> GetPermissions(Guid notificationUuid, Guid relatedEntityUuid, RelatedEntityType relatedEntityType);
    }
}
