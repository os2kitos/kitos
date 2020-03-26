using System;
using System.Linq;
using Core.ApplicationServices.Authentication;
using Core.ApplicationServices.SSO.Factories;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Repositories.Organization;
using Core.DomainServices.Repositories.SSO;
using Core.DomainServices.SSO;

namespace Core.ApplicationServices.SSO.State
{
    public class FirstTimeUserNotFoundState : AbstractState
    {
        private readonly StsBrugerInfo _stsBrugerInfo;
        private readonly ISsoOrganizationIdentityRepository _ssoOrganizationIdentityRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IOrganizationRoleService _organizationRoleService;
        private readonly SsoStateFactory _ssoStateFactory;

        public FirstTimeUserNotFoundState(StsBrugerInfo stsBrugerInfo,
            ISsoOrganizationIdentityRepository ssoOrganizationIdentityRepository,
            IOrganizationRepository organizationRepository,
            IUserRepository userRepository,
            IOrganizationRoleService organizationRoleService,
            SsoStateFactory ssoStateFactory)
        {
            _stsBrugerInfo = stsBrugerInfo;
            _ssoOrganizationIdentityRepository = ssoOrganizationIdentityRepository;
            _organizationRepository = organizationRepository;
            _userRepository = userRepository;
            _organizationRoleService = organizationRoleService;
            _ssoStateFactory = ssoStateFactory;
        }

        public override void Handle(FlowEvent @event, FlowContext context)
        {
            switch (@event)
            {
                case FlowEvent.UnableToLocateUser:
                    var organizationByCvrResult = _organizationRepository.GetByCvr(_stsBrugerInfo.MunicipalityCvr);
                    if (organizationByCvrResult.HasValue)
                    {
                        context.TransitionTo(new ErrorState(), _ => _.HandleUnableToLocateUser());
                    }
                    else
                    {
                        // TODO Create user
                        var user = new User();
                        context.TransitionTo(_ssoStateFactory.CreateUserLoggedIn(user), 
                            _ => _.HandleOrganizationFound());
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(@event), @event, null);
            }
        }
    }
}