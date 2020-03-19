using System;

namespace Core.ApplicationServices.SSO.State
{
    public class AuthorizingUserFromUnknownOrgState : AbstractState
    {
        public override void Handle(FlowEvent @event, FlowContext context)
        {
            if (@event.Equals(FlowEvent.OrganizationNotFound))
            {
                // TODO
            }
        }
    }
}