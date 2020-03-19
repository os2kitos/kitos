using System;

namespace Core.ApplicationServices.SSO.State
{
    public class AuthorizingUserState : AbstractState
    {
        public override void Handle(FlowEvent @event, FlowContext context)
        {
            if (@event.Equals(FlowEvent.OrganizationFound))
            {
                // TODO
            }
        }
    }
}