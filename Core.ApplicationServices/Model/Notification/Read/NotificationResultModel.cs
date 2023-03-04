using System;
using Core.DomainModel;
using Core.DomainModel.Advice;
using Core.DomainModel.Shared;

namespace Core.ApplicationServices.Model.Notification.Read
{
    public class NotificationResultModel
    {
        public NotificationResultModel(int id,
            Guid uuid,
            bool isActive,
            string name,
            DateTime? fromDate,
            DateTime? toDate,
            DateTime? sentDate,
            string subject,
            string body,
            Scheduling? repetitionFrequency,
            IEntityWithAdvices ownerResource,
            RelatedEntityType relatedEntityType,
            AdviceType notificationType,
            RecipientResultModel ccs,
            RecipientResultModel receivers)
        {
            Id = id;
            Uuid = uuid;
            IsActive = isActive;
            Name = name;
            FromDate = fromDate;
            ToDate = toDate;
            SentDate = sentDate;
            Subject = subject;
            Body = body;
            RepetitionFrequency = repetitionFrequency;
            OwnerResource = ownerResource;
            RelatedEntityType = relatedEntityType;
            NotificationType = notificationType;
            Ccs = ccs;
            Receivers = receivers;
        }

        public int Id { get; }
        public Guid Uuid { get; }
        public bool IsActive { get; }
        public string Name { get; }
        public DateTime? FromDate { get; }
        public DateTime? ToDate { get; }
        public DateTime? SentDate { get; }
        public string Subject { get; } 
        public string Body { get; }
        public Scheduling? RepetitionFrequency { get; }
        public IEntityWithAdvices OwnerResource { get; }
        public RelatedEntityType RelatedEntityType { get; }
        public AdviceType NotificationType { get;  }
        public RecipientResultModel Ccs { get; }
        public RecipientResultModel Receivers{ get; }
    }
}
