using System.Collections.Generic;
using Core.DomainModel;
using Core.DomainModel.Organization;

namespace Core.DomainServices
{
    public interface IOrganizationRoleService
    {
        OrganizationRight MakeUser(User user, Organization organization, User kitosUser);

        OrganizationRight MakeLocalAdmin(User user, Organization organization, User kitosUser);
        void RemoveLocalAdmin(User user, Organization organization);

        OrganizationRight MakeOrganizationModuleAdmin(User user, Organization organization, User kitosUser);
        void RemoveOrganizationModuleAdmin(User user, Organization organization);

        OrganizationRight MakeProjectModuleAdmin(User user, Organization organization, User kitosUser);
        void RemoveProjectModuleAdmin(User user, Organization organization);

        OrganizationRight MakeSystemModuleAdmin(User user, Organization organization, User kitosUser);
        void RemoveSystemModuleAdmin(User user, Organization organization);

        OrganizationRight MakeContractModuleAdmin(User user, Organization organization, User kitosUser);
        void RemoveContractModuleAdmin(User user, Organization organization);

        OrganizationRight MakeReportModuleAdmin(User user, Organization organization, User kitosUser);
        void RemoveReportModuleAdmin(User user, Organization organization);

        IEnumerable<OrganizationRole> GetRolesInOrganization(User user, int organizationId);
    }
}
