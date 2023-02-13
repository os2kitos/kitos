using System;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.Notification.Write
{
    public class ImmediateNotificationModificationParameters
    {
        public OptionalValueChange<string> Subject { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<string> Body { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<Guid> OwnerResourceUuid { get; set; } = OptionalValueChange<Guid>.None;

        public Maybe<RecipientModificationParameters> Ccs { get; set; }
        public Maybe<RecipientModificationParameters> Receivers { get; set; }
    }
}
