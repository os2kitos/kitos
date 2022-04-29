using System;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization.Policies;
using Core.DomainServices;
using Infrastructure.Services.DataAccess;


namespace Core.ApplicationServices.Authorization
{
    public class AuthorizationContextFactory : IAuthorizationContextFactory
    {
        private readonly IEntityTypeResolver _typeResolver;
        private readonly IUserRepository _userRepository;
        private static readonly GlobalReadAccessPolicy GlobalReadAccessPolicy = new GlobalReadAccessPolicy();

        public AuthorizationContextFactory(
            IEntityTypeResolver typeResolver,
            IUserRepository userRepository)
        {
            _typeResolver = typeResolver;
            _userRepository = userRepository;
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
                _userRepository
            );
        }
    }
}