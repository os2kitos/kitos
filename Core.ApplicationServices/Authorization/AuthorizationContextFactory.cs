using Core.ApplicationServices.Authorization.Policies;
using Infrastructure.Services.DataAccess;

namespace Core.ApplicationServices.Authorization
{
    public class AuthorizationContextFactory : IAuthorizationContextFactory
    {
        private readonly IEntityTypeResolver _typeResolver;
        private static readonly GlobalReadAccessPolicy GlobalReadAccessPolicy = new GlobalReadAccessPolicy();

        public AuthorizationContextFactory(IEntityTypeResolver typeResolver)
        {
            _typeResolver = typeResolver;
        }

        public IAuthorizationContext Create(IOrganizationalUserContext userContext)
        {
            //NOTE: SupplierAccess is injected here because then it is not "organizationAuthorizationContext but supplierauthorizationcontext"
            return userContext is UnauthenticatedUserContext
                ? new UnauthenticatedAuthorizationContext()
                : (IAuthorizationContext)new OrganizationAuthorizationContext(userContext, _typeResolver, new ModuleLevelAccessPolicy(userContext), GlobalReadAccessPolicy);
        }
    }
}