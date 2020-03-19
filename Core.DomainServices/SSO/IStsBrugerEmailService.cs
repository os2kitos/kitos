using System.Collections.Generic;

namespace Core.DomainServices.SSO
{
    public interface IStsBrugerEmailService
    {
        IEnumerable<string> GetStsBrugerEmails(string uuid);
    }
}