using System;
using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.Organization;
using Core.DomainServices.Authorization;

namespace Presentation.Web.Infrastructure.Authorization.Controller.General
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

        public CrossOrganizationDataReadAccessLevel GetCrossOrganizationReadAccess()
        {
            var userId = _userId();

            if (_authenticationService.IsGlobalAdmin(userId))
            {
                return CrossOrganizationDataReadAccessLevel.All;
            }

            return _authenticationService.HasReadAccessOutsideContext(userId)
                ? CrossOrganizationDataReadAccessLevel.Public
                : CrossOrganizationDataReadAccessLevel.None;
        }

        public OrganizationDataReadAccessLevel GetOrganizationReadAccessLevel(int organizationId)
        {
            if (_authenticationService.HasReadAccessOutsideContext(_userId()) ||
                _authenticationService.GetCurrentOrganizationId(_userId()) == organizationId)
            {
                //The legacy authorization was a binary decision. Even if municipality users should not see local data from other orgs the check allowed id and was rescued of the way KITOS UI asked for data.
                return OrganizationDataReadAccessLevel.All;
            }
            return OrganizationDataReadAccessLevel.None;
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
            if (entity is IHasAccessModifier)
            {
                switch (entity)
                {
                    case IContractModule _:
                        return _authenticationService.CanExecute(_userId(), Feature.CanSetContractElementsAccessModifierToPublic);
                    case IOrganizationModule _:
                        return _authenticationService.CanExecute(_userId(), Feature.CanSetOrganizationAccessModifierToPublic);
                }

                return _authenticationService.CanExecute(_userId(), Feature.CanSetAccessModifierToPublic);
            }

            return false;
        }
    }
}