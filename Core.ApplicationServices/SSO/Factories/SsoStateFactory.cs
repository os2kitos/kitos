using System;
using Core.ApplicationServices.Authentication;
using Core.ApplicationServices.SSO.Model;
using Core.ApplicationServices.SSO.State;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Repositories.Organization;
using Core.DomainServices.Repositories.SSO;
using Core.DomainServices.SSO;
using dk.nita.saml20.identity;
using Serilog;

namespace Core.ApplicationServices.SSO.Factories
{
    public class SsoStateFactory : ISsoStateFactory
    {
        private readonly IStsBrugerInfoService _infoService;
        private readonly ISsoUserIdentityRepository _ssoUserIdentityRepository;
        private readonly ISsoOrganizationIdentityRepository _ssoOrganizationIdentityRepository;
        private readonly Maybe<ISaml20Identity> _samlState;
        private readonly IUserRepository _userRepository;
        private readonly SsoFlowConfiguration _configuration;
        private readonly IApplicationAuthenticationState _applicationAuthenticationState;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IOrganizationRoleService _organizationRoleService;
        private readonly ILogger _logger;

        public SsoStateFactory(
            IStsBrugerInfoService infoService,
            ISsoUserIdentityRepository ssoUserIdentityRepository,
            ISsoOrganizationIdentityRepository ssoOrganizationIdentityRepository,
            Maybe<ISaml20Identity> samlState,
            IUserRepository userRepository,
            SsoFlowConfiguration configuration,
            IApplicationAuthenticationState applicationAuthenticationState,
            IOrganizationRepository organizationRepository,
            IOrganizationRoleService organizationRoleService,
            ILogger logger)
        {
            _infoService = infoService;
            _ssoUserIdentityRepository = ssoUserIdentityRepository;
            _ssoOrganizationIdentityRepository = ssoOrganizationIdentityRepository;
            _samlState = samlState;
            _userRepository = userRepository;
            _configuration = configuration;
            _applicationAuthenticationState = applicationAuthenticationState;
            _organizationRepository = organizationRepository;
            _organizationRoleService = organizationRoleService;
            _logger = logger;
        }

        public AbstractState CreateInitialState()
        {
            if (_samlState.IsNone)
            {
                throw new InvalidOperationException("Error: No SAML state");
            }
            return new InitialFlowState(_configuration, Saml20IdentityParser.CreateFrom(_samlState.Value), this);
        }

        public AbstractState CreatePrivilegeVerifiedState(Guid userExternalUuid)
        {
            return new PrivilegeVerifiedState(userExternalUuid, _userRepository, _infoService, _ssoUserIdentityRepository, this);
        }

        public AbstractState CreateUserLoggedIn(User user)
        {
            return new UserLoggedInState(user, _applicationAuthenticationState);
        }

        public AbstractState CreateUserIdentifiedState(User user, StsBrugerInfo stsBrugerInfo)
        {
            return new UserIdentifiedState(user, stsBrugerInfo, _ssoUserIdentityRepository, _ssoOrganizationIdentityRepository, _organizationRepository,this,_logger);
        }

        public AbstractState CreateAuthorizingUserState(User user, Organization organization)
        {
            return new AuthorizingUserState(user, organization, _organizationRoleService,this);
        }

        public AbstractState CreateAuthorizingUserFromUnknownOrgState(User user)
        {
            return new AuthorizingUserFromUnknownOrgState(user, this);
        }

        public AbstractState CreateAssigningRoleState(User user, Organization ssoOrganization)
        {
            return new AssigningRoleState(user,ssoOrganization,_organizationRoleService,this);
        }
    }
}
