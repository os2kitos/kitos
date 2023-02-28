using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Notification;
using Core.DomainModel.Advice;
using Core.DomainModel.Shared;

namespace Core.ApplicationServices.Notification
{
    public interface IRegistrationNotificationUserRelationsService
    {
        Result<Advice, OperationError> UpdateNotificationUserRelations(int notificationId, RecipientModel ccRecipients, RecipientModel receiverRecipients, RelatedEntityType relatedEntityType);
    }
}
