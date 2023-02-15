using System;
using Core.DomainModel.Advice;
using Core.DomainModel.Shared;

namespace Core.ApplicationServices.Model.Notification.Write
{
    public class UpdateScheduledNotificationModificationParameters: ImmediateNotificationModificationParameters
    {
        public UpdateScheduledNotificationModificationParameters(string body, string subject, RelatedEntityType type, Guid ownerResourceUuid, RootRecipientModificationParameters ccs, RootRecipientModificationParameters receivers, string name, DateTime? toDate) : base(body, subject, type, ownerResourceUuid, ccs, receivers)
        {
            Name = name;
            ToDate = toDate;
        }

        public string Name { get; }
        public DateTime? ToDate { get; }

    }
}
