using System;
using Core.ApplicationServices.Authorization.Policies;
using Core.DomainServices;
using Infrastructure.Services.DataAccess;


namespace Core.ApplicationServices.Authorization
{
    public class AuthorizationContextFactory : IAuthorizationContextFactory
    {
        private readonly IEntityTypeResolver _typeResolver;
        private readonly IUserRepository _userRepository;
        private readonly IAuthorizationModelFactory _authorizationModelFactory;
        private static readonly GlobalReadAccessPolicy GlobalReadAccessPolicy = new ();

        public AuthorizationContextFactory(
            IEntityTypeResolver typeResolver,
            IUserRepository userRepository,
            IAuthorizationModelFactory authorizationModelFactory)
        {
            _typeResolver = typeResolver;
            _userRepository = userRepository;
            _authorizationModelFactory = authorizationModelFactory;
        }

        public IAuthorizationContext Create(IOrganizationalUserContext userContext)
        {
            return userContext is UnauthenticatedUserContext
                ? new UnauthenticatedAuthorizationContext()
                : (IAuthorizationContext)CreateOrganizationAuthorizationContext(userContext);
        }

        private OrganizationAuthorizationContext CreateOrganizationAuthorizationContext(IOrganizationalUserContext userContext)
        {
            //NOTE: SupplierAccess is injected here because then it is not "organizationAuthorizationContext but supplierauthorizationcontext"
            var moduleLevelAccessPolicy = new ModuleModificationPolicy(userContext);

            return new OrganizationAuthorizationContext(
                userContext,
                _typeResolver,
                moduleLevelAccessPolicy,
                GlobalReadAccessPolicy,
                moduleLevelAccessPolicy,
                _userRepository,
                _authorizationModelFactory
            );
        }
    }
}