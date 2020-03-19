using Core.ApplicationServices.SSO.Factories;
using Core.DomainModel;
using Core.DomainModel.Organization;

namespace Core.ApplicationServices.SSO.State
{
    public class AuthorizingUserState : AbstractState
    {
        private readonly User _user;
        private readonly Organization _ssoOrganization;
        private readonly ISsoStateFactory _ssoStateFactory;

        public AuthorizingUserState(User user, Organization ssoOrganization, ISsoStateFactory ssoStateFactory)
        {
            _user = user;
            _ssoOrganization = ssoOrganization;
            _ssoStateFactory = ssoStateFactory;
        }

        public override void Handle(FlowEvent @event, FlowContext context)
        {
            if (@event.Equals(FlowEvent.OrganizationFound))
            {
                // TODO
            }
        }
    }
}