using System;
using Core.ApplicationServices;
using Core.DomainModel;

namespace Presentation.Web.Infrastructure.Authorization
{
    public class LegacyAuthorizationStrategy : IControllerAuthorizationStrategy
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly Func<int> _userId;

        public LegacyAuthorizationStrategy(IAuthenticationService authenticationService, Func<int> userId)
        {
            _authenticationService = authenticationService;
            _userId = userId;
        }

        public bool ApplyBaseQueryPostProcessing { get; } = false;

        public bool AllowOrganizationAccess(int organizationId)
        {
            var loggedIntoOrgId = _authenticationService.GetCurrentOrganizationId(_userId());
            return loggedIntoOrgId == organizationId || _authenticationService.HasReadAccessOutsideContext(_userId());
        }

        public bool AllowReadAccess(IEntity entity)
        {
            return _authenticationService.HasReadAccess(_userId(), entity);
        }

        public bool AllowWriteAccess(IEntity entity)
        {
            return _authenticationService.HasWriteAccess(_userId(), entity);
        }

        public bool AllowEntityVisibilityControl(IEntity entity)
        {
            return _authenticationService.CanExecute(_userId(), Feature.CanSetAccessModifierToPublic);
        }
    }
}