using Core.DomainModel.Advice;
using Presentation.Web.Models.API.V2.Internal.Response.Notifications;

namespace Presentation.Web.Controllers.API.V2.Internal.Notifications.Mapping
{
    public interface INotificationResponseMapper
    {
        NotificationResponseDTO MapNotificationResponseDTO(Advice notification);
    }
}
