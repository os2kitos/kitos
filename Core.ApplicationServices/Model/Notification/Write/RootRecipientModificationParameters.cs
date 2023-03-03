using System.Collections.Generic;

namespace Core.ApplicationServices.Model.Notification.Write
{
    public class RootRecipientModificationParameters
    {
        public static RootRecipientModificationParameters Empty() => new (new List<EmailRecipientModificationParameters>(), new List<RoleRecipientModificationParameters>());
        public RootRecipientModificationParameters(IEnumerable<EmailRecipientModificationParameters> emailRecipients, IEnumerable<RoleRecipientModificationParameters> roleRecipients)
        {
            EmailRecipients = emailRecipients;
            RoleRecipients = roleRecipients;
        }

        public IEnumerable<EmailRecipientModificationParameters> EmailRecipients { get; }
        public IEnumerable<RoleRecipientModificationParameters> RoleRecipients { get; }
    }   
}
