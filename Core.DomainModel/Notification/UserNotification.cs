using Core.DomainModel.GDPR;

namespace Core.DomainModel.Notification
{

    public class UserNotification : Entity, IOwnedByOrganization
    {
        public UserNotification(string name, string notificationMessage, NotificationType notificationType, int organizationId, int userId)
        {
            Name = name;
            NotificationMessage = notificationMessage;
            NotificationType = notificationType;
            OrganizationId = organizationId;
            ObjectOwnerId = userId;
        }

        public string Name { get; set; }
        public string NotificationMessage { get; set; }
        public NotificationType NotificationType { get; set; }
        public int OrganizationId { get; set; }
        public Organization.Organization Organization { get; set; }

        public int? ItProject_Id { get; set; }
        public virtual ItProject.ItProject ItProject { get; set; }

        public int? Itcontract_Id { get; set; }
        public virtual ItContract.ItContract ItContract { get; set; }

        public int? ItSystemUsage_Id { get; set; }
        public virtual ItSystemUsage.ItSystemUsage ItSystemUsage { get; set; }

        public int? DataProcessingRegistration_Id { get; set; }
        public virtual DataProcessingRegistration DataProcessingRegistration { get; set; }
    }

    public enum NotificationType
    {
        Advice = 0
    }

}
