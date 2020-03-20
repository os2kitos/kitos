using Core.ApplicationServices.SSO.Factories;
using Core.DomainModel;
using Core.DomainServices.Repositories.Organization;
using Core.DomainServices.Repositories.SSO;
using Core.DomainServices.SSO;
using Serilog;

namespace Core.ApplicationServices.SSO.State
{
    public class AuthorizingUserFromUnknownOrgState : AbstractState
    {
        private readonly User _user;
        private readonly StsBrugerInfo _externalUser;

        public AuthorizingUserFromUnknownOrgState(
            User user,
            StsBrugerInfo externalUser)
        {
            _user = user;
            _externalUser = externalUser;
        }

        public override void Handle(FlowEvent @event, FlowContext context)
        {
            if (@event.Equals(FlowEvent.OrganizationNotFound))
            {
                // TODO - check for role in any org
            }
        }
    }
}