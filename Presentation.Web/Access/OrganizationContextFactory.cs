using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Model.Authentication;

namespace Presentation.Web.Access
{
    public class OrganizationContextFactory : IOrganizationContextFactory
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly IUserContextFactory _userContextFactory;
        private readonly IAuthenticationContext _authenticationContext;

        public OrganizationContextFactory(
            IGenericRepository<User> userRepository,
            IGenericRepository<Organization> organizationRepository,
            IUserContextFactory userContextFactory,
            IAuthenticationContext authenticationContext
            )
        {
            _userRepository = userRepository;
            _organizationRepository = organizationRepository;
            _userContextFactory = userContextFactory;
            _authenticationContext = authenticationContext;
        }

        public OrganizationContext CreateOrganizationContext(int organizationId)
        {
            return new OrganizationContext(_userRepository, _organizationRepository, organizationId);
        }

        public IAccessContext CreateOrganizationAccessContext()
        {
            var activeUserContext = _userContextFactory.Create(
                userId: _authenticationContext.UserId.GetValueOrDefault(-1),
                organizationId: _authenticationContext.ActiveOrganizationId.GetValueOrDefault(-1));

            return new OrganizationAccessContext(activeUserContext);
        }
    }
}