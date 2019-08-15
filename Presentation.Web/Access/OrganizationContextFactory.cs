using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainServices;

namespace Presentation.Web.Access
{
    class OrganizationContextFactory : IOrganizationContextFactory
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly IGenericRepository<ItSystemRole> _itSystemRoleRepository;

        public OrganizationContextFactory(
            IGenericRepository<User> userRepository, 
            IGenericRepository<Organization> organizationRepository,
            IGenericRepository<ItSystemRole> itSystemRoleRepository
            )
        {
            _userRepository = userRepository;
            _organizationRepository = organizationRepository;
            _itSystemRoleRepository = itSystemRoleRepository;
        }

        public OrganizationContext CreateOrganizationContext(int organizationId)
        {
            return new OrganizationContext(_userRepository, _organizationRepository, _itSystemRoleRepository, organizationId);
        }
    }
}