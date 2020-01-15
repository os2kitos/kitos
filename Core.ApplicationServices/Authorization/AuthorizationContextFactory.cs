namespace Core.ApplicationServices.Authorization
{
    public class AuthorizationContextFactory : IAuthorizationContextFactory
    {
        private static readonly GlobalReadAccessPolicy GlobalReadAccessPolicy = new GlobalReadAccessPolicy();

        public IAuthorizationContext Create(IOrganizationalUserContext userContext)
        {
            //NOTE: SupplierAccess is injected here because then it is not "organizationAuthorizationContext but supplierauthorizationcontext"
            return userContext is UnauthenticatedUserContext
                ? new UnauthenticatedAuthorizationContext()
                : (IAuthorizationContext)new OrganizationAuthorizationContext(userContext, new ModuleLevelAccessRule(userContext), GlobalReadAccessPolicy);
        }
    }
}