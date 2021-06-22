namespace Core.ApplicationServices.Model.Notification
{
    public class GlobalAdminNotificationMessage
    {
        public string Content { get; }
        public bool IsHtml { get; }

        public GlobalAdminNotificationMessage(string content, bool isHtml)
        {
            Content = content;
            IsHtml = isHtml;
        }
    }
}
