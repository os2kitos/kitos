using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices;

namespace Presentation.Web.Access
{
    public class OrganizationContextFactory : IOrganizationContextFactory
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly IUserContextFactory _userContextFactory;

        public OrganizationContextFactory(
            IGenericRepository<User> userRepository,
            IGenericRepository<Organization> organizationRepository,
            IUserContextFactory userContextFactory
            )
        {
            _userRepository = userRepository;
            _organizationRepository = organizationRepository;
            _userContextFactory = userContextFactory;
        }

        public OrganizationContext CreateOrganizationContext(int organizationId)
        {
            return new OrganizationContext(_userRepository, _organizationRepository, organizationId);
        }

        public OrganizationAccessContext CreateOrganizationAccessContext(int organizationId)
        {
            return new OrganizationAccessContext(_userContextFactory,organizationId);
        }
    }
}