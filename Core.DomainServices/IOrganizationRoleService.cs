using System.Collections.Generic;
using Core.DomainModel;
using Core.DomainModel.Organization;

namespace Core.DomainServices
{
    public interface IOrganizationRoleService
    {
        OrganizationRight MakeUser(User user, Organization organization, User kitosUser);

        OrganizationRight MakeLocalAdmin(User user, Organization organization, User kitosUser);

        IEnumerable<OrganizationRole> GetRolesInOrganization(User user, int organizationId);
    }
}
