using Core.DomainModel.GDPR;
using Core.DomainModel.Shared;
using System;

namespace Core.DomainModel.Notification
{

    public class UserNotification : Entity, IOwnedByOrganization, IHasName
    {
        public UserNotification(string name, string notificationMessage, NotificationType notificationType, int organizationId, int userId, DateTime created)
        {
            Name = name;
            NotificationMessage = notificationMessage;
            NotificationType = notificationType;
            OrganizationId = organizationId;
            NotificationRecipientId = userId;
            Created = created;
        }

        public UserNotification()
        {
        }

        public string Name { get; set; }
        public string NotificationMessage { get; set; }
        public NotificationType NotificationType { get; set; }

        public User NotificationRecipient { get; set; }
        public int NotificationRecipientId { get; set; }

        public DateTime Created { get; set; }

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

        public IEntityWithUserNotification GetOwner()
        {
            return
                ItSystemUsage ??
                ItContract ??
                ItProject ??
                (IEntityWithUserNotification) DataProcessingRegistration;
        }

        public int GetRelatedEntityId()
        {
            return
                ItProject_Id ??
                Itcontract_Id ??
                ItSystemUsage_Id ??
                DataProcessingRegistration_Id ??
                throw new InvalidOperationException($"Cannot find any RelatedEntityId for advis with Id: {Id}");
        }

        public RelatedEntityType GetRelatedEntityType()
        {
            if(ItProject_Id != null)
            {
                return RelatedEntityType.itProject;
            }
            if (Itcontract_Id != null)
            {
                return RelatedEntityType.itContract;
            }
            if (ItSystemUsage_Id != null)
            {
                return RelatedEntityType.itSystemUsage;
            }
            if (DataProcessingRegistration_Id != null)
            {
                return RelatedEntityType.dataProcessingRegistration;
            }
            throw new InvalidOperationException($"Cannot find any RelatedEntityId to created RelatedEntityType from for advis with Id: {Id}");
        }

    }

    public enum NotificationType
    {
        Advice = 0
    }

}
