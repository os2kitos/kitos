using System;
using System.Linq;
using Core.ApplicationServices.Authentication;
using Core.ApplicationServices.SSO.Factories;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
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
                        var user = CreateAutoProvisonedUser(organizationByCvrResult);
                        _organizationRoleService.MakeUser(user, organizationByCvrResult.Value);

                        context.TransitionTo(_ssoStateFactory.CreateUserLoggedIn(user), 
                            _ => _.HandleUserAutoProvisioned());
                    }
                    else
                    {
                        context.TransitionTo(new ErrorState(), 
                            _ => _.HandleUnableToLocateUser());
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(@event), @event, null);
            }
        }

        private User CreateAutoProvisonedUser(Maybe<Organization> organizationByCvrResult)
        {
            var user = _userRepository.Create();
            user.Email = _stsBrugerInfo.Emails.First();
            user.Name = _stsBrugerInfo.FirstName;
            user.LastName = _stsBrugerInfo.LastName;
            user.DefaultOrganization = organizationByCvrResult.Value;
            user.Salt = string.Format("{0:N}{0:N}", Guid.NewGuid());
            user.Password = "default123"; // TODO: Change this to encrypted version
            _userRepository.Save();
            return user;
        }
    }
}