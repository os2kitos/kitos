using System;
using Core.ApplicationServices.Model.Notification.Write;
using Presentation.Web.Models.API.V2.Internal.Request.Notifications;
using Presentation.Web.Models.API.V2.Types.Notifications;

namespace Presentation.Web.Controllers.API.V2.Internal.Notifications.Mapping
{
    public interface INotificationWriteModelMapper
    {
        ImmediateNotificationModificationParameters FromImmediatePOST(ImmediateNotificationWriteRequestDTO dto, Guid ownerResourceUuid, OwnerResourceType ownerResourceType);
        CreateScheduledNotificationModificationParameters FromScheduledPOST(ScheduledNotificationWriteRequestDTO dto, Guid ownerResourceUuid, OwnerResourceType ownerResourceType);
        UpdateScheduledNotificationModificationParameters FromScheduledPUT(UpdateScheduledNotificationWriteRequestDTO dto, Guid ownerResourceUuid, OwnerResourceType ownerResourceType);
    }
}
