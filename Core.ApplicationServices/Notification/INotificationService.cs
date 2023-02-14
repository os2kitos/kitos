using Core.DomainModel.Shared;
using System.Collections.Generic;
using System;
using Core.Abstractions.Types;
using Core.DomainModel.Advice;

namespace Core.ApplicationServices.Notification
{
    public interface INotificationService
    {
        Result<IEnumerable<Advice>, OperationError> GetAdvices(Guid organizationUuid, RelatedEntityType relatedEntityType, DateTime fromDate);
        Result<Advice, OperationError> GetAdviceByUuid(Guid uuid, RelatedEntityType relatedEntityType);
        Result<Advice, OperationError> CreateNotification(Guid organizationUuid, Advice notification);
        Result<Advice, OperationError> UpdateNotification(Guid organizationUuid, Advice notification);
    }
}
