using System.Collections.Generic;

namespace Core.ApplicationServices.Model.Notification
{
    public class RecipientModel
    {
        public IEnumerable<EmailRecipientModel> EmailRecipients { get; set; }
        public IEnumerable<RoleRecipientModel> RoleRecipients { get; set; }

    }
}
