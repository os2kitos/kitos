using Presentation.Web.Infrastructure.Model.Authentication;

namespace Presentation.Web.Access
{
    public class AccessContextFactory : IAccessContextFactory
    {
        private readonly IUserContextFactory _userContextFactory;
        private readonly IAuthenticationContext _authenticationContext;

        public AccessContextFactory(
            IUserContextFactory userContextFactory,
            IAuthenticationContext authenticationContext
            )
        {
            _userContextFactory = userContextFactory;
            _authenticationContext = authenticationContext;
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