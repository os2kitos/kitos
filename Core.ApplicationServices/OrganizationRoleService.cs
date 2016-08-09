using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices;

namespace Core.ApplicationServices
{
    public class OrganizationRoleService : IOrganizationRoleService
    {
        private readonly IGenericRepository<OrganizationRight> _organizationRights;

        public OrganizationRoleService(IGenericRepository<OrganizationRight> organizationRights)
        {
            _organizationRights = organizationRights;
        }

        private OrganizationRight AddOrganizationRoleToUser(User user, Organization organization, User kitosUser, OrganizationRole organizationRole)
        {
            var result = _organizationRights.Insert(new OrganizationRight
            {
                Organization = organization,
                User = user,
                Role = organizationRole,
                LastChangedByUser = kitosUser
            });
            _organizationRights.Save();

            return result;
        }
        
        private void RemoveOrganizationRoleServiceFromUser(User user, Organization organization, OrganizationRole organizationRole)
        {
            _organizationRights.DeleteByKey(organization.Id, organizationRole, user.Id);
            _organizationRights.Save();
        }

        public OrganizationRight MakeLocalAdmin(User user, Organization organization, User kitosUser)
        {
            return AddOrganizationRoleToUser(user, organization, kitosUser, OrganizationRole.LocalAdmin);
        }

        public void RemoveLocalAdmin(User user, Organization organization)
        {
            RemoveOrganizationRoleServiceFromUser(user, organization, OrganizationRole.LocalAdmin);
        }

        public OrganizationRight MakeOrganizationModuleAdmin(User user, Organization organization, User kitosUser)
        {
            return AddOrganizationRoleToUser(user,organization,kitosUser, OrganizationRole.OrganizationModuleAdmin);
        }

        public void RemoveOrganizationModuleAdmin(User user, Organization organization)
        {
            RemoveOrganizationRoleServiceFromUser(user,organization, OrganizationRole.OrganizationModuleAdmin);
        }

        public OrganizationRight MakeProjectModuleAdmin(User user, Organization organization, User kitosUser)
        {
            return AddOrganizationRoleToUser(user, organization, kitosUser, OrganizationRole.ProjectModuleAdmin);
        }

        public void RemoveProjectModuleAdmin(User user, Organization organization)
        {
            RemoveOrganizationRoleServiceFromUser(user, organization, OrganizationRole.ProjectModuleAdmin);
        }

        public OrganizationRight MakeSystemModuleAdmin(User user, Organization organization, User kitosUser)
        {
            return AddOrganizationRoleToUser(user, organization, kitosUser, OrganizationRole.SystemModuleAdmin);
        }

        public void RemoveSystemModuleAdmin(User user, Organization organization)
        {
            RemoveOrganizationRoleServiceFromUser(user, organization, OrganizationRole.SystemModuleAdmin);
        }

        public OrganizationRight MakeContractModuleAdmin(User user, Organization organization, User kitosUser)
        {
            return AddOrganizationRoleToUser(user, organization, kitosUser, OrganizationRole.ContractModuleAdmin);
        }

        public void RemoveContractModuleAdmin(User user, Organization organization)
        {
            RemoveOrganizationRoleServiceFromUser(user, organization, OrganizationRole.ContractModuleAdmin);
        }

        public OrganizationRight MakeReportModuleAdmin(User user, Organization organization, User kitosUser)
        {
            return AddOrganizationRoleToUser(user, organization, kitosUser, OrganizationRole.ReportModuleAdmin);
        }

        public void RemoveReportModuleAdmin(User user, Organization organization)
        {
            RemoveOrganizationRoleServiceFromUser(user, organization, OrganizationRole.ReportModuleAdmin);
        }
    }
}
