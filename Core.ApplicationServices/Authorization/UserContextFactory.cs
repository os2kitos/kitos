using System;
using System.Linq;
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

        public IOrganizationalUserContext Create(int userId)
        {
            var user = _userRepository.GetByKey(userId);
            if (user == null)
            {
                throw new InvalidOperationException($"Cannot create user context for invalid user ID:{userId}");
            }

            //Get roles for the organization
            var organizationRoles = _roleService.GetOrganizationRoles(user);

            var organizationCategories = user
                .OrganizationRights
                .GroupBy(x => x.OrganizationId)
                .Select(x => new { x.Key, x.First().Organization.Type.Category })
                .ToDictionary(x => x.Key, x => x.Category);


            return new OrganizationalUserContext(user.Id, organizationRoles, organizationCategories, user.HasStakeHolderAccess, user.IsSystemIntegrator);
        }
    }
}