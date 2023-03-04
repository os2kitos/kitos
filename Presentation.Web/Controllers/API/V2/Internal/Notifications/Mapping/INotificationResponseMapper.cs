using Core.ApplicationServices.Model.Notification;
using Core.ApplicationServices.Model.Notification.Read;
using Core.DomainModel.Advice;
using Presentation.Web.Models.API.V2.Internal.Request.Notifications;
using Presentation.Web.Models.API.V2.Internal.Response.Notifications;

namespace Presentation.Web.Controllers.API.V2.Internal.Notifications.Mapping
{
    public interface INotificationResponseMapper
    {
        NotificationResponseDTO MapNotificationResponseDTO(NotificationResultModel notification);
        NotificationSentResponseDTO MapNotificationSentResponseDTO(AdviceSent notificationSent);
        NotificationResourcePermissionsDTO MapNotificationAccessRightsResponseDTO(NotificationPermissions permissions);
    }
}
