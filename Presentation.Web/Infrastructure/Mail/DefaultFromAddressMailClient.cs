using System.Net.Mail;
using Core.DomainServices;

namespace Presentation.Web.Infrastructure.Mail
{
    /// <summary>
    /// Decorates an <see cref="IMailClient"/> and ensures every outgoing message has a From address.
    /// This replaces the .NET Framework behaviour where SmtpClient read the default sender from
    /// &lt;system.net&gt;&lt;mailSettings&gt; in web.config, which is not supported in .NET Core/5+.
    /// </summary>
    internal sealed class DefaultFromAddressMailClient : IMailClient
    {
        private readonly IMailClient _inner;
        private readonly string _fromAddress;

        public DefaultFromAddressMailClient(IMailClient inner, string fromAddress)
        {
            _inner = inner;
            _fromAddress = fromAddress;
        }

        public void Send(MailMessage message)
        {
            message.From ??= new MailAddress(_fromAddress);
            _inner.Send(message);
        }
    }
}
