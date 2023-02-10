using System;
using Core.DomainModel.Advice;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Models.API.V2.Internal.Response.Notifications;

namespace Presentation.Web.Controllers.API.V2.Internal.Notifications.Mapping
{
    public class NotificationResponseMapper : INotificationResponseMapper
    {
        public NotificationResponseDTO MapNotificationResponseDTO(Advice notification)
        {
            return new NotificationResponseDTO
            {
                Active = notification.IsActive,
                Name = notification.Name,
                LastSent = notification.SentDate,
                FromDate = new DateTime(), //TODO: ?????
                ToDate = notification.StopDate,
                Subject = notification.Subject,
                //TODO: Map receivers and ccs
                OwnerResource = notification.ObjectOwner.MapIdentityNamePairDTO()
                //Uuid = notification.Uuid //TODO: Add uuid to the notifications
            };
        }
    }
}