using System;
using Core.DomainModel;

namespace Core.ApplicationServices.SSO.State
{
    public class UserLoggedInState : AbstractState
    {
        private readonly User _user;

        public UserLoggedInState(User user)
        {
            _user = user;
        }

        //TODO: Require the user!
        public override void Handle(FlowEvent @event, FlowContext context)
        {
            switch (@event)
            {
                case FlowEvent.UserSeenBefore:
                case FlowEvent.UserHasRoleInOrganization:
                case FlowEvent.RoleAssigned:
                    // TODO: Authenticate the user
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@event), @event, null);
            }
        }
    }
}