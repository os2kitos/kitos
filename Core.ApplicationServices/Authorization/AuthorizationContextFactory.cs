namespace Core.ApplicationServices.Authorization
{
    public class AuthorizationContextFactory : IAuthorizationContextFactory
    {
        public IAuthorizationContext Create(IOrganizationalUserContext userContext)
        {
            return userContext is UnauthenticatedUserContext
                ? new UnauthenticatedAuthorizationContext()
                : (IAuthorizationContext) new OrganizationAuthorizationContext(userContext);
        }
    }
}