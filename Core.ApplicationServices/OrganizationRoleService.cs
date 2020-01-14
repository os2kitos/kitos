using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices;

namespace Core.ApplicationServices
{
    public class OrganizationRoleService : IOrganizationRoleService
    {
        private readonly IGenericRepository<OrganizationRight> _organizationRights;
        private readonly IOrganizationalUserContext _userContext;

        public OrganizationRoleService(IGenericRepository<OrganizationRight> organizationRights, IOrganizationalUserContext userContext)
        {
            _organizationRights = organizationRights;
            _userContext = userContext;
        }

        private OrganizationRight AddOrganizationRoleToUser(User user, Organization organization, OrganizationRole organizationRole)
        {
            var kitosUser = _userContext.UserEntity;
            var result = _organizationRights.Insert(new OrganizationRight
            {
                Organization = organization,
                User = user,
                Role = organizationRole,
                LastChangedByUser = kitosUser,
                ObjectOwner = kitosUser
            });
            _organizationRights.Save();

            return result;
        }

        public OrganizationRight MakeUser(User user, Organization organization)
        {
            return AddOrganizationRoleToUser(user, organization, OrganizationRole.User);
        }

        public OrganizationRight MakeLocalAdmin(User user, Organization organization)
        {
            return AddOrganizationRoleToUser(user, organization, OrganizationRole.LocalAdmin);
        }

        public IEnumerable<OrganizationRole> GetRolesInOrganization(User user, int organizationId)
        {
            var roles =
                user
                    .OrganizationRights
                    .Where(or => or.OrganizationId == organizationId)
                    .Select(x => x.Role)
                    .ToList();

            //NOTE: Use of this property is somewhat messy. In some cases it applies the IsGlobalAdmin boolean (the right way) and in other cases it uses the "right" with the role "Global admin" which is the wrong way
            if (user.IsGlobalAdmin)
            {
                roles.Add(OrganizationRole.GlobalAdmin);
            }

            return roles
                .Distinct()
                .ToList()
                .AsReadOnly();
        }
    }
}
