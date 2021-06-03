using Core.DomainModel.Advice;
using Core.DomainModel.Notification;
using Core.DomainModel.Shared;
using System;

namespace Presentation.Web.Controllers.API
{
    public class UserNotificationDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NotificationMessage { get; set; }
        public int RelatedEntityId { get; set; }
        public RelatedEntityType RelatedEntityType { get; set; }
        public NotificationType NotificationType { get; set; }
        public DateTime LastChanged { get; set; }
    }
}