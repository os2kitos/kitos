using Core.DomainModel.Shared;

namespace Core.DomainModel.Notification
{
    public class UserNotification : Entity, IOwnedByOrganization
    {
        public const int MaxNameLength = 100;
        public const int MaxMessageLength = 200;

        public string Name { get; set; }
        public string NotificationMessage { get; set; }
        public int RelatedEntityId { get; set; }
        public RelatedEntityType RelatedEntityType { get; set; }
        public NotificationType NotificationType { get; set; }
        public int OrganizationId { get; set; }
        public Organization.Organization Organization { get; set; }
    }

    public enum NotificationType
    {
        advice = 0
    }

}
