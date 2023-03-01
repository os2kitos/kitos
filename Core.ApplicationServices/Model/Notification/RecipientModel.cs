using System.Collections.Generic;

namespace Core.ApplicationServices.Model.Notification
{
    public class RecipientModel
    {
        public static RecipientModel Empty() => new (new List<EmailRecipientModel>(), new List<RoleRecipientModel>());

        public RecipientModel(IEnumerable<EmailRecipientModel> emailRecipients, IEnumerable<RoleRecipientModel> roleRecipients)
        {
            EmailRecipients = emailRecipients;
            RoleRecipients = roleRecipients;
        }

        public IEnumerable<EmailRecipientModel> EmailRecipients { get; }
        public IEnumerable<RoleRecipientModel> RoleRecipients { get; }

    }
}
