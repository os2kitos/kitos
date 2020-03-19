using System;

namespace Core.ApplicationServices.SSO.State
{
    public class UserLoggedInState : AbstractState
    {
        public override void Handle(FlowEvent @event, FlowContext context)
        {
            switch (@event)
            {
                case FlowEvent.UserSeenBefore:
                case FlowEvent.UserHasRoleInOrganization:
                case FlowEvent.RoleAssigned:
                    // TODO: Set current user as incoming user
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@event), @event, null);
            }
        }
    }
}