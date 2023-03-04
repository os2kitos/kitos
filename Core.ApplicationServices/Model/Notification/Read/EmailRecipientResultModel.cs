namespace Core.ApplicationServices.Model.Notification.Read
{
    public class EmailRecipientResultModel
    {
        public EmailRecipientResultModel(string email)
        {
            Email = email;
        }

        public string Email { get; }
    }
}
