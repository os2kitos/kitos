using System;
using Core.DomainServices;

namespace Core.ApplicationServices.Authorization
{
    public class UserContextFactory : IUserContextFactory
    {
        private readonly IUserRepository _userRepository;
        private readonly IOrganizationRoleService _roleService;

        public UserContextFactory(
            IUserRepository userRepository,
            IOrganizationRoleService roleService)
        {
            _userRepository = userRepository;
            _roleService = roleService;
        }

        public IOrganizationalUserContext Create(int userId, int organizationId)
        {
            var user = _userRepository.GetByKey(userId);
            if (user == null)
            {
                throw new InvalidOperationException($"Cannot create user context for invalid user ID:{userId}");
            }

            //Get roles for the organization
            var organizationRoles = _roleService.GetRolesInOrganization(user, organizationId);

            return new OrganizationalUserContext(organizationRoles, user, organizationId);
        }
    }
}