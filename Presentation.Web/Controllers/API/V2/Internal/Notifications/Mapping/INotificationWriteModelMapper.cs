using Core.ApplicationServices.Model.Notification.Write;
using Presentation.Web.Models.API.V2.Internal.Request.Notifications;

namespace Presentation.Web.Controllers.API.V2.Internal.Notifications.Mapping
{
    public interface INotificationWriteModelMapper
    {
        ImmediateNotificationModificationParameters FromImmediatePOST(ImmediateNotificationWriteRequestDTO dto);
        ScheduledNotificationWriteRequestDTO FromScheduledPOST(ScheduledNotificationWriteRequestDTO dto);
        ScheduledNotificationWriteRequestDTO FromScheduledPut(UpdateScheduledNotificationWriteRequestDTO dto);
    }
}
