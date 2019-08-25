using System;
using System.Linq;
using Core.ApplicationServices;
using Core.DomainServices;

namespace Presentation.Web.Infrastructure.Authorization.Context
{
    public class UserContextFactory : IUserContextFactory
    {
        private readonly IUserRepository _userRepository;
        private readonly IFeatureChecker _featureChecker;
        private readonly IOrganizationRoleService _roleService;

        public UserContextFactory(
            IUserRepository userRepository,
            IFeatureChecker featureChecker,
            IOrganizationRoleService roleService)
        {
            _userRepository = userRepository;
            _featureChecker = featureChecker;
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

            var supportedFeatures =
                Enum.GetValues(typeof(Feature))
                    .Cast<Feature>()
                    .Where(x => _featureChecker.CanExecute(user, x))
                    .ToList();

            return new OrganizationalUserContext(supportedFeatures, organizationRoles, user, organizationId);
        }
    }
}