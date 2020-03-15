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

        public void HandleUserHasValidAccessRoleInSamlToken()
        {
            CurrentState.Handle(FlowEvent.UserHasValidAccessRole, this);
        }

        public void HandleUserWithEmailExists()
        {
            CurrentState.Handle(FlowEvent.UserExists, this);
        }

        public void HandleUserAlreadyAssociated()
        {
            CurrentState.Handle(FlowEvent.UserAlreadyAssociated, this);
        }

        public void HandleUserInKnownOrganization()
        {
            CurrentState.Handle(FlowEvent.UserInKnownOrganization, this);
        }

        public void HandleUserHasRole()
        {
            CurrentState.Handle(FlowEvent.UserHasRole, this);
        }
    }
}