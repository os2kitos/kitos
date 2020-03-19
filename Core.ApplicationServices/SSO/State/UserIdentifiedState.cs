using System;

namespace Core.ApplicationServices.SSO.State
{
    public class UserIdentifiedState : AbstractState
    {
        public override void Handle(FlowEvent @event, FlowContext context)
        {
            if (@event.Equals(FlowEvent.UserFirstTimeVisit))
            {
                // TODO: Remember user by uuid and organisation uuid
                // TODO: Lookup organization in Kitos db
                // TODO: Change state based on found organization
            }
        }
    }
}