using System.Collections.Generic;
using Core.DomainModel;

namespace Core.DomainServices
{
    public interface IAdminService
    {
        OrganizationRight MakeLocalAdmin(User user, Organization organization, User kitosUser);
        void RemoveLocalAdmin(User user, Organization organization);

        bool IsGlobalAdmin(User user);
        bool IsLocalAdmin(User user, Organization organization);

        OrganizationRole GetLocalAdminRole();

        IEnumerable<OrganizationRight> GetAdminRights();
    }
}