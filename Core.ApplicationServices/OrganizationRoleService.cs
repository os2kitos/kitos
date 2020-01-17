using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Authentication;
using Core.DomainModel;
using Core.DomainModel.Constants;
using Core.DomainModel.Organization;
using Core.DomainServices;

namespace Core.ApplicationServices
{
    public class OrganizationRoleService : IOrganizationRoleService
    {
        private readonly IGenericRepository<OrganizationRight> _organizationRights;
        private readonly IAuthenticationContext _authenticationContext;
        private readonly IUserRepository _userRepository;

        public OrganizationRoleService(
            IGenericRepository<OrganizationRight> organizationRights,
            IAuthenticationContext authenticationContext,
            IUserRepository userRepository)
        {
            _organizationRights = organizationRights;
            _authenticationContext = authenticationContext;
            _userRepository = userRepository;
        }

        private OrganizationRight AddOrganizationRoleToUser(User user, Organization organization, OrganizationRole organizationRole)
        {
            var kitosUser = _userRepository.GetById(_authenticationContext.UserId.GetValueOrDefault(EntityConstants.InvalidId));
            if (kitosUser == null)
            {
                throw new InvalidOperationException($"Cannot determine who is adding the role to the user with id:{user.Id}");
            }
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
