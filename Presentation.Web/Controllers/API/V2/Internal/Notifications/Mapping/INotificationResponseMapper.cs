using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Notification;
using Core.DomainModel.Advice;
using Presentation.Web.Models.API.V2.Internal.Request.Notifications;
using Presentation.Web.Models.API.V2.Internal.Response.Notifications;

namespace Presentation.Web.Controllers.API.V2.Internal.Notifications.Mapping
{
    public interface INotificationResponseMapper
    {
        Result<NotificationResponseDTO, OperationError> MapNotificationResponseDTO(Advice notification);
        Result<IEnumerable<NotificationResponseDTO>, OperationError> MapNotificationResponseDTOs(IEnumerable<Advice> notifications);
        NotificationSentResponseDTO MapNotificationSentResponseDTO(AdviceSent notificationSent);
        NotificationAccessRightsResponseDTO MapNotificationAccessRightsResponseDTO(NotificationAccessRights accessRights);
    }
}
