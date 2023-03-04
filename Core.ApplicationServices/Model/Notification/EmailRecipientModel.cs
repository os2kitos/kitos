namespace Core.ApplicationServices.Model.Notification
{
    public class EmailRecipientModel
    {
        public EmailRecipientModel(string email)
        {
            Email = email;
        }

        public string Email { get; }
    }
}
