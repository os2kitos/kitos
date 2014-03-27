using System.Net.Mail;

namespace Core.DomainServices
{
    public interface IMailClient
    {
        void Send(MailMessage message);
    }
}