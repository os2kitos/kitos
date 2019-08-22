using System;
using System.Linq;
using Core.ApplicationServices;
using Core.DomainServices;

namespace Presentation.Web.Access
{
    public class UserContextFactory : IUserContextFactory
    {
        private readonly IUserRepository _userRepository;
        private readonly IFeatureChecker _featureChecker;

        public UserContextFactory(
            IUserRepository userRepository,
            IFeatureChecker featureChecker)
        {
            _userRepository = userRepository;
            _featureChecker = featureChecker;
        }

        public IOrganizationalUserContext Create(int? userId, int organizationId)
        {
            if (userId.HasValue)
            {
                var user = _userRepository.GetByKey(userId);
                if (user == null)
                {
                    throw new InvalidOperationException($"Cannot create user context for invalid user ID:{userId}");
                }

                //Get roles for the organization
                var organizationRoles = user.GetRolesInOrg(organizationId);

                var supportedFeatures =
                    Enum.GetValues(typeof(Feature))
                        .Cast<Feature>()
                        .Where(x => _featureChecker.CanExecute(user, x))
                        .ToList();

                return new OrganizationalUserContext(supportedFeatures, organizationRoles, user, organizationId);
            }
            return new AnonymouslUserContext();
        }
    }
}