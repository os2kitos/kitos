using Presentation.Web.Infrastructure.Model.Authentication;

namespace Presentation.Web.Infrastructure.Authorization
{
    public class AuthorizationContextFactory : IAuthorizationContextFactory
    {
        private readonly IUserContextFactory _userContextFactory;
        private readonly IAuthenticationContext _authenticationContext;

        public AuthorizationContextFactory(
            IUserContextFactory userContextFactory,
            IAuthenticationContext authenticationContext
            )
        {
            _userContextFactory = userContextFactory;
            _authenticationContext = authenticationContext;
        }

        public IAuthorizationContext Create()
        {
            var activeUserContext = _userContextFactory.Create(
                userId: _authenticationContext.UserId,
                organizationId: _authenticationContext.ActiveOrganizationId.GetValueOrDefault(-1));

            return new OrganizationAuthorizationContext(activeUserContext);
        }
    }
}