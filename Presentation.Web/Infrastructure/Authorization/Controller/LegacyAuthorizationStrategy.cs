using System;
using Core.ApplicationServices;
using Core.DomainModel;

namespace Presentation.Web.Infrastructure.Authorization.Controller
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

        public bool AllowOrganizationReadAccess(int organizationId)
        {
            var loggedIntoOrgId = _authenticationService.GetCurrentOrganizationId(_userId());
            return loggedIntoOrgId == organizationId || _authenticationService.HasReadAccessOutsideContext(_userId());
        }

        public bool AllowRead(IEntity entity)
        {
            return _authenticationService.HasReadAccess(_userId(), entity);
        }

        public bool AllowCreate<T>(IEntity entity)
        {
            //Old strategy was hard coded in a lot of controllers and otherwise they created an instance and asked for modificationaccess
            return AllowModify(entity);
        }

        public bool AllowCreate<T>()
        {
            return false;
        }

        public bool AllowModify(IEntity entity)
        {
            return _authenticationService.HasWriteAccess(_userId(), entity);
        }

        public bool AllowDelete(IEntity entity)
        {
            return AllowModify(entity);
        }

        public bool AllowEntityVisibilityControl(IEntity entity)
        {
            return _authenticationService.CanExecute(_userId(), Feature.CanSetAccessModifierToPublic);
        }
    }
}