using System.Collections.Generic;

namespace Core.ApplicationServices.Model.Notification.Write
{
    public class RootRecipientModificationParameters
    {
        public RootRecipientModificationParameters(IEnumerable<EmailRecipientModificationParameters> emailRecipients, IEnumerable<RoleRecipientModificationParameters> roleRecipients)
        {
            EmailRecipients = emailRecipients;
            RoleRecipients = roleRecipients;
        }

        public IEnumerable<EmailRecipientModificationParameters> EmailRecipients { get; }
        public IEnumerable<RoleRecipientModificationParameters> RoleRecipients { get; }
    }   
}
