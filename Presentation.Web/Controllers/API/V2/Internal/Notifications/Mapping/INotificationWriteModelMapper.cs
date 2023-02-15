using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Notification;
using Core.ApplicationServices.Model.Notification.Write;
using Core.DomainModel.Advice;
using Presentation.Web.Models.API.V2.Internal.Request.Notifications;
using Presentation.Web.Models.API.V2.Types.Notifications;

namespace Presentation.Web.Controllers.API.V2.Internal.Notifications.Mapping
{
    public interface INotificationWriteModelMapper
    {
        Result<ImmediateNotificationModificationParameters, OperationError> FromImmediatePOST(ImmediateNotificationWriteRequestDTO dto, OwnerResourceType ownerResourceType);
        Result<ScheduledNotificationModificationParameters, OperationError> FromScheduledPOST(ScheduledNotificationWriteRequestDTO dto, OwnerResourceType ownerResourceType);
        Result<UpdateScheduledNotificationModificationParameters, OperationError> FromScheduledPUT(UpdateScheduledNotificationWriteRequestDTO dto, OwnerResourceType ownerResourceType);
    }
}
