using System.Collections.Generic;

namespace Core.ApplicationServices.SSO
{
    public interface ISSOFlowApplicationService
    {
        bool HasCurrentUserKitosPrivilege();
        IEnumerable<string> GetStsBrugerEmails(string uuid);
    }
}