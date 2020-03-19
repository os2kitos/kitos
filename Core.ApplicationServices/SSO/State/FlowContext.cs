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
            CurrentState.Handle(FlowEvent.LoginCompleted, this);
        }

        public void HandleUserPrivilegeVerified()
        {
            CurrentState.Handle(FlowEvent.UserPrivilegeVerified, this);
        }

        public void HandleUserPrivilegeInvalid()
        {
            CurrentState.Handle(FlowEvent.UserPrivilegeInvalid, this);
        }

        public void HandleUserSeenBefore()
        {
            CurrentState.Handle(FlowEvent.UserSeenBefore, this);
        }

        public void HandleUserFirstTimeVisit()
        {
            CurrentState.Handle(FlowEvent.UserFirstTimeVisit, this);
        }

        public void HandleOrganizationFound()
        {
            CurrentState.Handle(FlowEvent.OrganizationFound, this);
        }

        public void HandleOrganizationNotFound()
        {
            CurrentState.Handle(FlowEvent.OrganizationNotFound, this);
        }

        public void HandleUserHasRoleInOrganization()
        {
            CurrentState.Handle(FlowEvent.UserHasRoleInOrganization, this);
        }

        public void HandleUserHasNoRoleInOrganization()
        {
            CurrentState.Handle(FlowEvent.UserHasNoRoleInOrganization, this);
        }

        public void HandleRoleAssigned()
        {
            CurrentState.Handle(FlowEvent.RoleAssigned, this);
        }

    }
}