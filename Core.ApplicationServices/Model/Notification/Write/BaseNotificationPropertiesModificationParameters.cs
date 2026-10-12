using Core.DomainModel.Shared;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Core.ApplicationServices.Model.Notification.Write
{
    public class BaseNotificationPropertiesModificationParameters
    {
        [SetsRequiredMembers]
        public BaseNotificationPropertiesModificationParameters(string? body, string subject, RelatedEntityType type, Guid ownerResourceUuid, RootRecipientModificationParameters ccs, RootRecipientModificationParameters receivers)
        {
            Body = body;
            Subject = subject;
            Type = type;
            OwnerResourceUuid = ownerResourceUuid;
            Ccs = ccs;
            Receivers = receivers;
        }

        public string? Body { get; }
        public required string Subject { get; init; }
        public RelatedEntityType Type { get; }
        public Guid OwnerResourceUuid { get; }
        public RootRecipientModificationParameters Ccs { get; }
        public RootRecipientModificationParameters Receivers { get; }
    }
}
