using System;
using Core.DomainServices;
using System.Net.Mail;

namespace Core.ApplicationServices
{
    public class MailClient : IMailClient, IDisposable
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
        /// <param name="message">The message.</param>
        public void Send(MailMessage message)
        {
            _client.Send(message);
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
