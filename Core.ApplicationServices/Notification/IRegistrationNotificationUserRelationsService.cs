using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Notification;
using System.Collections.Generic;

namespace Core.ApplicationServices.Notification
{
    public interface IRegistrationNotificationUserRelationsService
    {
        Maybe<OperationError> UpdateNotificationUserRelations(int notificationId, IEnumerable<RecipientModel> updateModels);
    }
}
