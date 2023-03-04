using System.Collections.Generic;

namespace Core.ApplicationServices.Model.Notification.Read
{
    public class RecipientResultModel
    {
        public RecipientResultModel(IEnumerable<EmailRecipientResultModel> emailRecipients, IEnumerable<RoleRecipientResultModel> roleRecipients)
        {
            EmailRecipients = emailRecipients;
            RoleRecipients = roleRecipients;
        }

        public IEnumerable<EmailRecipientResultModel> EmailRecipients { get; }
        public IEnumerable<RoleRecipientResultModel> RoleRecipients { get; }
    }
}
