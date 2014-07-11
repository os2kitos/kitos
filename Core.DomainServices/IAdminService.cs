using System.Collections.Generic;
using Core.DomainModel;

namespace Core.DomainServices
{
    public interface IAdminService
    {
        AdminRight MakeLocalAdmin(User user, Organization organization, User kitosUser);
        void RemoveLocalAdmin(User user, Organization organization);

        bool IsGlobalAdmin(User user);
        bool IsLocalAdmin(User user, Organization organization);

        AdminRole GetLocalAdminRole();

        IEnumerable<AdminRight> GetAdminRights();
    }
}