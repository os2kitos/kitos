using Core.ApplicationServices.SSO.Factories;
using Core.DomainModel;

namespace Core.ApplicationServices.SSO.State
{
    public class AuthorizingUserFromUnknownOrgState : AbstractState
    {
        private readonly User _user;
        private readonly ISsoStateFactory _stateFactory;

        public AuthorizingUserFromUnknownOrgState(
            User user,
            ISsoStateFactory stateFactory)
        {
            _user = user;
            _stateFactory = stateFactory;
        }

        public override void Handle(FlowEvent @event, FlowContext context)
        {
            if (@event.Equals(FlowEvent.OrganizationNotFound))
            {
                if (_user.CanAuthenticate())
                {
                    context.TransitionTo(_stateFactory.CreateUserLoggedIn(_user), _ => _.HandleUserHasRoleInOrganization());
                }
                else
                {
                    context.TransitionTo(_stateFactory.CreateErrorState(), _ => _.HandleNoRoleAndOrganization());
                }
            }
        }
    }
}