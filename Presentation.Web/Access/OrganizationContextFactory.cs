using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices;

namespace Presentation.Web.Access
{
    class OrganizationContextFactory : IOrganizationContextFactory
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<Organization> _organizationRepository;

        public OrganizationContextFactory(IGenericRepository<User> userRepository, IGenericRepository<Organization> organizationRepository)
        {
            _userRepository = userRepository;
            _organizationRepository = organizationRepository;
        }

        public OrganizationContext CreateOrganizationContext(int organizationId)
        {
            return new OrganizationContext(_userRepository, _organizationRepository, organizationId);
        }
    }
}