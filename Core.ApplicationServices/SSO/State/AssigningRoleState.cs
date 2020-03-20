using Core.ApplicationServices.SSO.Factories;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices;

namespace Core.ApplicationServices.SSO.State
{
    public class AssigningRoleState : AbstractState
    {
        private readonly User _user;
        private readonly Organization _ssoOrganization;
        private readonly IOrganizationRoleService _organizationRoleService;
        private readonly ISsoStateFactory _ssoStateFactory;

        public AssigningRoleState(User user, Organization ssoOrganization, IOrganizationRoleService organizationRoleService, ISsoStateFactory ssoStateFactory)
        {
            _user = user;
            _ssoOrganization = ssoOrganization;
            _organizationRoleService = organizationRoleService;
            _ssoStateFactory = ssoStateFactory;
        }

        public override void Handle(FlowEvent @event, FlowContext context)
        {
            if (@event.Equals(FlowEvent.UserHasNoRoleInOrganization))
            {
                _organizationRoleService.MakeUser(_user, _ssoOrganization);
                context.Transition(_ssoStateFactory.CreateUserLoggedIn(_user), _ => _.HandleRoleAssigned());
            }
        }
    }
}