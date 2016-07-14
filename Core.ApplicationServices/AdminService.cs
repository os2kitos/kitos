using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainServices;

namespace Core.ApplicationServices
{
    public class AdminService : IAdminService
    {
        private readonly IGenericRepository<OrganizationRight> _organizationRights;
        private readonly IGenericRepository<OrganizationRole> _organizationRoles;

        public AdminService(IGenericRepository<OrganizationRight> organizationRights, IGenericRepository<OrganizationRole> organizationRoles)
        {
            _organizationRights = organizationRights;
            _organizationRoles = organizationRoles;
        }

        public OrganizationRight MakeLocalAdmin(User user, Organization organization, User kitosUser)
        {
            var result = _organizationRights.Insert(new OrganizationRight
                {
                    Object = organization,
                    User = user,
                    Role = GetLocalAdminRole(),
                    LastChangedByUser = kitosUser
                });
            // TODO update related objects, like the user and organization. Missing support for it right now.
            _organizationRights.Save();

            return result;
        }

        public void RemoveLocalAdmin(User user, Organization organization)
        {
            var role = GetLocalAdminRole();
            _organizationRights.DeleteByKey(organization.Id, role.Id, user.Id);
            _organizationRights.Save();
        }

        public bool IsGlobalAdmin(User user)
        {
            return user.IsGlobalAdmin;
        }

        public bool IsLocalAdmin(User user, Organization organization)
        {
            return user.OrganizationRights.Any(right => right.Role.Name == "LocalAdmin" && right.ObjectId == organization.Id);
        }

        public OrganizationRole GetLocalAdminRole()
        {
            return _organizationRoles.Get(role => role.Name == "LocalAdmin").First();
        }

        public IEnumerable<OrganizationRight> GetAdminRights()
        {
            return _organizationRights.Get();
        }
    }
}
