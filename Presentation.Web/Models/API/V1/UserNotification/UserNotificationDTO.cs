using System;
using Core.DomainModel.Notification;
using Core.DomainModel.Shared;

namespace Presentation.Web.Models.API.V1.UserNotification
{
    public class UserNotificationDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NotificationMessage { get; set; }
        public int RelatedEntityId { get; set; }
        public RelatedEntityType RelatedEntityType { get; set; }
        public NotificationType NotificationType { get; set; }
        public DateTime Created { get; set; }
    }
}