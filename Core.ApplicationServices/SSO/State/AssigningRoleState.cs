using System;

namespace Core.ApplicationServices.SSO.State
{
    public class AssigningRoleState : AbstractState
    {
        public override void Handle(FlowEvent @event, FlowContext context)
        {
            if (@event.Equals(FlowEvent.UserHasNoRoleInOrganization))
            {
                // TODO
            }
        }
    }
}