using System;
using Core.ApplicationServices.Authentication;
using Core.DomainModel;

namespace Core.ApplicationServices.SSO.State
{
    public class UserLoggedInState : AbstractState
    {
        private readonly User _user;
        private readonly IApplicationAuthenticationState _authenticationState;

        public UserLoggedInState(User user, IApplicationAuthenticationState authenticationState)
        {
            _user = user;
            _authenticationState = authenticationState;
        }

        public override void Handle(FlowEvent @event, FlowContext context)
        {
            switch (@event)
            {
                case FlowEvent.UserSeenBefore:
                case FlowEvent.UserHasRoleInOrganization:
                case FlowEvent.RoleAssigned:
                case FlowEvent.UserAutoProvisioned:
                    _authenticationState.SetAuthenticatedUser(_user, AuthenticationScope.Session);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@event), @event, null);
            }
        }
    }
}