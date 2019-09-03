using Core.ApplicationServices.Authentication;

namespace Core.ApplicationServices.Authorization
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
            var isAuthenticated =
                _authenticationContext.UserId.HasValue &&
                _authenticationContext.ActiveOrganizationId.HasValue;

            if (isAuthenticated)
            {
                var activeUserContext = _userContextFactory.Create(
                    userId: _authenticationContext.UserId.Value,
                    organizationId: _authenticationContext.ActiveOrganizationId.Value);
                return new OrganizationAuthorizationContext(activeUserContext);
            }

            return new UnauthenticatedAuthorizationContext();
        }
    }
}