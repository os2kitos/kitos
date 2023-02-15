using Core.DomainModel.Shared;
using System.Collections.Generic;
using System;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Notification.Write;
using Core.DomainModel.Advice;

namespace Core.ApplicationServices.Notification
{
    public interface INotificationService
    {
        Result<IEnumerable<Advice>, OperationError> GetAdvices(Guid organizationUuid, RelatedEntityType relatedEntityType, DateTime fromDate);
        Result<Advice, OperationError> GetAdviceByUuid(Guid uuid, RelatedEntityType relatedEntityType);
        Result<Advice, OperationError> CreateImmediateNotification(Guid organizationUuid, ImmediateNotificationModificationParameters parameters);
        Result<Advice, OperationError> CreateScheduledNotification(Guid organizationUuid, ScheduledNotificationModificationParameters parameters);
        Result<Advice, OperationError> UpdateScheduledNotification(Guid organizationUuid, UpdateScheduledNotificationModificationParameters parameters);
    }
}
