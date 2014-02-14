namespace Core.DomainServices
{
    public interface IMailClient
    {
        void Send(string from, string to, string subject, string content, bool isHtml = true);
    }
}