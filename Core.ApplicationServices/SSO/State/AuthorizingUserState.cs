using System.Linq;
using Core.ApplicationServices.SSO.Factories;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices;

namespace Core.ApplicationServices.SSO.State
{
    public class AuthorizingUserState : AbstractState
    {
        private readonly User _user;
        private readonly Organization _ssoOrganization;
        private readonly IOrganizationRoleService _organizationRoleService;
        private readonly ISsoStateFactory _ssoStateFactory;

        public AuthorizingUserState(User user, Organization ssoOrganization, IOrganizationRoleService organizationRoleService, ISsoStateFactory ssoStateFactory)
        {
            _user = user;
            _ssoOrganization = ssoOrganization;
            _organizationRoleService = organizationRoleService;
            _ssoStateFactory = ssoStateFactory;
        }

        public override void Handle(FlowEvent @event, FlowContext context)
        {
            if (@event.Equals(FlowEvent.OrganizationFound))
            {
                var rolesInOrganization = _organizationRoleService.GetRolesInOrganization(_user, _ssoOrganization.Id);
                if (rolesInOrganization.Any())
                {
                    context.TransitionTo(_ssoStateFactory.CreateUserLoggedIn(_user), _ => _.HandleUserHasRoleInOrganization());
                }
                else
                {
                    context.TransitionTo(_ssoStateFactory.CreateAssigningRoleState(_user, _ssoOrganization), _ => _.HandleUserHasNoRoleInOrganization());
                }
            }
        }
    }
}