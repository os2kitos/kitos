using Core.DomainServices;
using System.Net.Mail;
using Infrastructure.Services.Delegates;

namespace Core.ApplicationServices
{
    public class SingleThreadedMailClient : IMailClient
    {
        /// <summary>
        /// Overcome restrictions defined by the target service e.g. services such as Office365: https://techcommunity.microsoft.com/t5/exchange-team-blog/changes-coming-to-the-smtp-authenticated-submission-client/ba-p/607825
        /// </summary>
        private static readonly object ConnectionLock = new object();

        private readonly Factory<SmtpClient> _clientFactory;

        /// <summary>
        /// Construct a smtp client with configuration from web.config
        /// </summary>
        public SingleThreadedMailClient()
        {
            _clientFactory = () => new SmtpClient();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleThreadedMailClient"/> class.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <param name="enableSsl">if set to <c>true</c> [enable SSL].</param>
        public SingleThreadedMailClient(string host, int port, bool enableSsl = false)
        {
            _clientFactory = () => new SmtpClient(host, port) { EnableSsl = enableSsl };
        }

        /// <summary>
        /// Sends the specified mail.
        /// </summary>
        /// <param name="message">The message.</param>
        public virtual void Send(MailMessage message)
        {
            lock (ConnectionLock)
            {
                using var client = _clientFactory();
                client.Send(message);
            }
        }
    }
}
