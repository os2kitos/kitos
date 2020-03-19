using System.Collections.Generic;

namespace Core.ApplicationServices.SSO.State
{
    public class LookupStsUserEmailState : AbstractState
    {
        private readonly IEnumerable<string> _stsBrugerEmails;

        public LookupStsUserEmailState(IEnumerable<string> stsBrugerEmails)
        {
            _stsBrugerEmails = stsBrugerEmails;
        }

        public override void Handle(FlowEvent @event, FlowContext context)
        {
            if (@event.Equals(FlowEvent.UserHasValidAccessRole))
            {
                // TODO: Lookup stsBrugerEmails in Kitos User database
                context.TransitionTo(new UserSignedInState());
            }
        }
    }
}