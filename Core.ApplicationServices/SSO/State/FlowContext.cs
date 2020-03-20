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

        public void TransitionTo(AbstractState nextState)
        {
            CurrentState = nextState;
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

        public void HandleUserFirstTimeVisit()
        {
            Handle(FlowEvent.UserFirstTimeVisit);
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
            Handle(FlowEvent.UnableToResolveUserInStsOrganisation);
        }

        public void HandleUnsupportedFlow()
        {
            //TODO: Remove in KITOSUDV-627: User creation flow (runtime provisioning
            Handle(FlowEvent.UnsupportedFlow);
        }

        public void HandleUnknownError()
        {
            Handle(FlowEvent.UnknownError);
        }

        private void Handle(FlowEvent eventToHandle)
        {
            CurrentState.Handle(eventToHandle, this);
        }
    }
}