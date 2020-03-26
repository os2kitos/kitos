using System;
using NotImplementedException = System.NotImplementedException;

namespace Core.ApplicationServices.SSO.State
{
    public class FlowContext
    {
        public AbstractState CurrentState { get; private set; }

        public FlowContext(AbstractState currentState)
        {
            CurrentState = currentState;
        }

        public void TransitionTo(AbstractState nextState, Action<FlowContext> withAction)
        {
            CurrentState = nextState;
            withAction?.Invoke(this);
        }

        public void HandleLoginCompleted()
        {
            Handle(FlowEvent.LoginCompleted);
        }

        public void HandleUserPrivilegeVerified()
        {
            Handle(FlowEvent.UserPrivilegeVerified);
        }

        public void HandleUserPrivilegeInvalid()
        {
            Handle(FlowEvent.UserPrivilegeInvalid);
        }

        public void HandleUserSeenBefore()
        {
            Handle(FlowEvent.UserSeenBefore);
        }

        public void HandleUserFirstTimeSsoVisit()
        {
            Handle(FlowEvent.UserFirstTimeSsoVisit);
        }

        public void HandleOrganizationFound()
        {
            Handle(FlowEvent.OrganizationFound);
        }

        public void HandleOrganizationNotFound()
        {
            Handle(FlowEvent.OrganizationNotFound);
        }

        public void HandleUserHasRoleInOrganization()
        {
            Handle(FlowEvent.UserHasRoleInOrganization);
        }

        public void HandleUserHasNoRoleInOrganization()
        {
            Handle(FlowEvent.UserHasNoRoleInOrganization);
        }

        public void HandleRoleAssigned()
        {
            Handle(FlowEvent.RoleAssigned);
        }

        public void HandleUnableToResolveUserInStsOrganisation()
        {
            Handle(FlowEvent.UnableToResolveUserInStsOrganization);
        }

        public void HandleUnknownError()
        {
            Handle(FlowEvent.UnknownError);
        }

        private void Handle(FlowEvent eventToHandle)
        {
            CurrentState.Handle(eventToHandle, this);
        }

        public void HandleNoRoleAndOrganization()
        {
            Handle(FlowEvent.NoOrganizationAndRole);
        }

        public void HandleExistingSsoUserWithoutRoles()
        {
            Handle(FlowEvent.ExistingSsoUserWithoutRoles);
        }

        public void HandleUnableToLocateUser()
        {
            Handle(FlowEvent.UnableToLocateUser);
        }
    }
}