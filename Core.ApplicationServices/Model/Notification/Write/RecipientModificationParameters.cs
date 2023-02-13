using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.Notification.Write
{
    public class RecipientModificationParameters
    {
        public OptionalValueChange<Maybe<IEnumerable<EmailRecipientModificationParameters>>> EmailRecipients { get; set; } = OptionalValueChange<Maybe<IEnumerable<EmailRecipientModificationParameters>>>.None;
        public OptionalValueChange<Maybe<IEnumerable<RoleRecipientModificationParameters>>> RoleRecipients { get; set; } = OptionalValueChange<Maybe<IEnumerable<RoleRecipientModificationParameters>>>.None;
    }
}
