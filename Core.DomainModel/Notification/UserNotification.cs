using Core.DomainModel.GDPR;
using Core.DomainModel.Shared;
using System;
using Core.Abstractions.Types;

namespace Core.DomainModel.Notification
{

    public class UserNotification : Entity, IOwnedByOrganization, IHasName, IHasUuid
    {
        public UserNotification(string name, string notificationMessage, NotificationType notificationType, int organizationId, int userId, DateTime created)
        {
            Name = name;
            NotificationMessage = notificationMessage;
            NotificationType = notificationType;
            OrganizationId = organizationId;
            NotificationRecipientId = userId;
            Created = created;
            Uuid = Guid.NewGuid();
        }

        public UserNotification()
        {
        }

        public Guid Uuid { get; set; }

        public string Name { get; set; }
        public string NotificationMessage { get; set; }
        public NotificationType NotificationType { get; set; }

        public User NotificationRecipient { get; set; }
        public int NotificationRecipientId { get; set; }

        public DateTime Created { get; set; }

        public int OrganizationId { get; set; }
        public Organization.Organization Organization { get; set; }

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
                (IEntityWithUserNotification)DataProcessingRegistration;
        }

        public Guid GetRelatedEntityUuid()
        {
            return GetOwner().Uuid;
        }

        public int GetRelatedEntityId()
        {
            return ResolveRelatedEntityInfo()
                .Select(x => x.Item1)
                .Match(relatedEntityType => relatedEntityType, () => throw new InvalidOperationException($"Cannot find any RelatedEntityId for advis with Id: {Id}"));
        }

        public RelatedEntityType GetRelatedEntityType()
        {
            return ResolveRelatedEntityInfo()
                .Select(x => x.Item2)
                .Match(relatedEntityType => relatedEntityType, () => throw new InvalidOperationException($"Cannot find any RelatedEntityId to created RelatedEntityType from for advis with Id: {Id}"));
        }

        private Maybe<(int, RelatedEntityType)> ResolveRelatedEntityInfo()
        {
            if (Itcontract_Id != null)
            {
                return Maybe<(int, RelatedEntityType)>.Some((Itcontract_Id.Value, RelatedEntityType.itContract));
            }
            if (ItSystemUsage_Id != null)
            {
                return Maybe<(int, RelatedEntityType)>.Some((ItSystemUsage_Id.Value, RelatedEntityType.itSystemUsage));
            }
            if (DataProcessingRegistration_Id != null)
            {
                return Maybe<(int, RelatedEntityType)>.Some((DataProcessingRegistration_Id.Value, RelatedEntityType.dataProcessingRegistration));
            }
            return Maybe<(int, RelatedEntityType)>.None;
        }

    }

}
