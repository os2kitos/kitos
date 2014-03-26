using Core.DomainServices;
using System.Net.Mail;

namespace Core.ApplicationServices
{
    public class MailClient : IMailClient
    {
        private readonly SmtpClient _client;

        /// <summary>
        /// Construct a smtp client with configuration from web.config
        /// </summary>
        public MailClient()
        {
            _client = new SmtpClient();
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="MailClient"/> class.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <param name="enableSsl">if set to <c>true</c> [enable SSL].</param>
        public MailClient(string host, int port, bool enableSsl = false)
        {
            _client = new SmtpClient(host, port) {EnableSsl = enableSsl};
        }

        /// <summary>
        /// Sends the specified mail.
        /// </summary>
        /// <param name="to">To.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="content">The content.</param>
        /// <param name="isHtml">if set to <c>true</c> [is HTML].</param>
        public void Send(string to, string subject, string content, bool isHtml = true)
        {
            var address = new MailAddress(to);
            var message = new MailMessage
                {
                    Subject = subject,
                    Body = content,
                    IsBodyHtml = isHtml
                };
            message.To.Add(address);

            _client.Send(message);
        }
    }
}
