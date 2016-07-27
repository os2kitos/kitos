using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices;

namespace Core.ApplicationServices
{
    public class AdminService : IAdminService
    {
        private readonly IGenericRepository<OrganizationRight> _organizationRights;

        public AdminService(IGenericRepository<OrganizationRight> organizationRights)
        {
            _organizationRights = organizationRights;
        }

        public OrganizationRight MakeLocalAdmin(User user, Organization organization, User kitosUser)
        {
            var result = _organizationRights.Insert(new OrganizationRight
                {
                    Organization = organization,
                    User = user,
                    Role = OrganizationRole.LocalAdmin,
                    LastChangedByUser = kitosUser
                });
            // TODO update related objects, like the user and organization. Missing support for it right now.
            _organizationRights.Save();

            return result;
        }

        public void RemoveLocalAdmin(User user, Organization organization)
        {
            _organizationRights.DeleteByKey(organization.Id, OrganizationRole.LocalAdmin, user.Id);
            _organizationRights.Save();
        }
    }
}
