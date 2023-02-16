using System;
using Core.DomainModel.Advice;
using Core.DomainModel.Shared;

namespace Core.ApplicationServices.Model.Notification.Write
{
    public class ScheduledNotificationModificationParameters: UpdateScheduledNotificationModificationParameters
    {
        public ScheduledNotificationModificationParameters(string body, string subject, RelatedEntityType type, Guid ownerResourceUuid, RootRecipientModificationParameters ccs, RootRecipientModificationParameters receivers, string name, DateTime? toDate, Scheduling repetitionFrequency, DateTime fromDate) : base(body, subject, type, ownerResourceUuid, ccs, receivers, name, toDate)
        {
            RepetitionFrequency = repetitionFrequency;
            FromDate = fromDate;
        }

        public Scheduling RepetitionFrequency { get; }
        public DateTime FromDate { get; }

    }
}
