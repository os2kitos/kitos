using System;
using Core.DomainModel.Shared;

namespace Core.ApplicationServices.Model.Notification.Write
{
    public class ImmediateNotificationModificationParameters
    {
        public ImmediateNotificationModificationParameters(string body, string subject, RelatedEntityType type, Guid ownerResourceUuid, RootRecipientModificationParameters ccs, RootRecipientModificationParameters receivers)
        {
            Body = body;
            Subject = subject;
            Type = type;
            OwnerResourceUuid = ownerResourceUuid;
            Ccs = ccs;
            Receivers = receivers;
        }

        public string Body { get; }
        public string Subject { get; }
        public virtual RelatedEntityType Type{ get; }
        public Guid OwnerResourceUuid { get; }
        public RootRecipientModificationParameters Ccs { get; }
        public RootRecipientModificationParameters Receivers { get; }
    }
}
