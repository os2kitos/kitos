namespace Core.ApplicationServices.Model.Notification.Write
{
    public class EmailRecipientModificationParameters
    {
        public EmailRecipientModificationParameters(string email)
        {
            Email = email;
        }

        public string Email { get; }
    }
}
