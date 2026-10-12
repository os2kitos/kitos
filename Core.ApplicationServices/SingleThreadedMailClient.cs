using Core.DomainServices;
using System.Net;
using System.Net.Mail;
using Core.Abstractions.Types;

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
        /// Initializes a client that drops mail to a local pickup directory (e.g. for local development).
        /// </summary>
        /// <param name="pickupDirectoryLocation">Path where .eml files are written.</param>
        public SingleThreadedMailClient(string pickupDirectoryLocation)
        {
            _clientFactory = () => new SmtpClient
            {
                DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                PickupDirectoryLocation = pickupDirectoryLocation
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleThreadedMailClient"/> class.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <param name="enableSsl">if set to <c>true</c> [enable SSL].</param>
        /// <param name="userName">Optional SMTP username for authentication.</param>
        /// <param name="password">Optional SMTP password for authentication.</param>
        public SingleThreadedMailClient(string host, int port, bool enableSsl = false, string? userName = null, string? password = null)
        {
            _clientFactory = () =>
            {
                var client = new SmtpClient(host, port) { EnableSsl = enableSsl };
                if (!string.IsNullOrWhiteSpace(userName))
                    client.Credentials = new NetworkCredential(userName, password);
                return client;
            };
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
