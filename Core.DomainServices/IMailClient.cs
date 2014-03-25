namespace Core.DomainServices
{
    public interface IMailClient
    {
        void Send(string to, string subject, string content, bool isHtml = true);
    }
}