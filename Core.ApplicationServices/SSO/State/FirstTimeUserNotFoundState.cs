using System;
using System.Linq;
using Core.ApplicationServices.SSO.Factories;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Repositories.Organization;
using Core.DomainServices.SSO;
using Infrastructure.Services.Cryptography;

namespace Core.ApplicationServices.SSO.State
{
    public class FirstTimeUserNotFoundState : AbstractState
    {
        private readonly StsBrugerInfo _stsBrugerInfo;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IOrganizationRoleService _organizationRoleService;
        private readonly ICryptoService _cryptoService;
        private readonly SsoStateFactory _ssoStateFactory;

        public FirstTimeUserNotFoundState(StsBrugerInfo stsBrugerInfo,
            IOrganizationRepository organizationRepository,
            IUserRepository userRepository,
            IOrganizationRoleService organizationRoleService,
            ICryptoService cryptoService,
            SsoStateFactory ssoStateFactory)
        {
            _stsBrugerInfo = stsBrugerInfo;
            _organizationRepository = organizationRepository;
            _userRepository = userRepository;
            _organizationRoleService = organizationRoleService;
            _cryptoService = cryptoService;
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
                        var organization = organizationByCvrResult.Value;
                        var user = CreateAutoProvisonedUser(organization);
                        _organizationRoleService.MakeUser(user, organization);

                        context.TransitionTo(_ssoStateFactory.CreateUserLoggedIn(user), 
                            _ => _.HandleUserAutoProvisioned());
                    }
                    else
                    {
                        context.TransitionTo(_ssoStateFactory.CreateErrorState(), 
                            _ => _.HandleUnableToLocateUser());
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(@event), @event, null);
            }
        }

        private User CreateAutoProvisonedUser(Organization organizationByCvrResult)
        {
            var user = _userRepository.Create();
            user.Email = _stsBrugerInfo.Emails.First();
            user.Name = _stsBrugerInfo.FirstName;
            user.LastName = _stsBrugerInfo.LastName;
            user.DefaultOrganization = organizationByCvrResult;
            user.Salt = string.Format("{0:N}{0:N}", Guid.NewGuid());
            user.Password = _cryptoService.Encrypt(string.Empty);
            _userRepository.Save();
            return user;
        }
    }
}