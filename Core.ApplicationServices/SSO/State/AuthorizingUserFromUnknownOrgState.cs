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
                    context.Transition(_stateFactory.CreateUserLoggedIn(_user), _ => _.HandleUserHasRoleInOrganization());
                }
                else
                {
                    context.Transition(new ErrorState(), _ => _.HandleNoRoleAndOrganization());
                }
            }
        }
    }
}