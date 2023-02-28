using System.Collections.Generic;

namespace Core.ApplicationServices.Model.Notification
{
    public class RecipientModel
    {
        public RecipientModel(IEnumerable<EmailRecipientModel> emailRecipients, IEnumerable<RoleRecipientModel> roleRecipients)
        {
            EmailRecipients = emailRecipients;
            RoleRecipients = roleRecipients;
        }

        public IEnumerable<EmailRecipientModel> EmailRecipients { get; }
        public IEnumerable<RoleRecipientModel> RoleRecipients { get; }

    }
}
