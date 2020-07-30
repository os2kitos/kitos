using System;
using System.Linq;
using Core.ApplicationServices.Authorization.Policies;
using Core.DomainModel;
using Core.DomainModel.Result;
using Core.DomainServices;
using Infrastructure.Services.DataAccess;

namespace Core.ApplicationServices.Authorization
{
    public class AuthorizationContextFactory : IAuthorizationContextFactory
    {
        private readonly IEntityTypeResolver _typeResolver;
        private readonly IGenericRepository<GlobalConfig> _globalConfigurationRepository;
        private readonly IUserRepository _userRepository;
        private static readonly GlobalReadAccessPolicy GlobalReadAccessPolicy = new GlobalReadAccessPolicy();

        public AuthorizationContextFactory(
            IEntityTypeResolver typeResolver,
            IGenericRepository<GlobalConfig> globalConfigurationRepository,
            IUserRepository userRepository)
        {
            _typeResolver = typeResolver;
            _globalConfigurationRepository = globalConfigurationRepository;
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
            Maybe<GlobalConfig> globalConfig =
                _globalConfigurationRepository
                    .AsQueryable()
                    .FirstOrDefault(gc => gc.key == GlobalConfigKeys.OnlyGlobalAdminMayEditReports);

            //NOTE: SupplierAccess is injected here because then it is not "organizationAuthorizationContext but supplierauthorizationcontext"
            var onlyGlobalAdminMayEditReports = globalConfig.Select(x => bool.TrueString.Equals(x.value, StringComparison.OrdinalIgnoreCase)).GetValueOrFallback(false);
            var moduleLevelAccessPolicy = new ModuleModificationPolicy(userContext, onlyGlobalAdminMayEditReports);

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