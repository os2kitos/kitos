using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DomainServices;
using System.Net.Mail;

namespace Core.ApplicationServices
{
    public class MailClient : IMailClient
    {
        private readonly System.Net.Mail.SmtpClient _client;

        public MailClient(string host, int port, bool enableSsl = false)
        {
            _client = new System.Net.Mail.SmtpClient(host, port) {EnableSsl = enableSsl};
        }

        public void Send(string from, string to, string subject, string content, bool isHtml = true)
        {
            var message = new MailMessage(from, to, subject, content) {IsBodyHtml = isHtml};

            _client.Send(message);
        }
    }
}
