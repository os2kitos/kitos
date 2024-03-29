﻿using System;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authentication;
using Core.ApplicationServices.SSO.Model;
using Core.ApplicationServices.SSO.State;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Repositories.Organization;
using Core.DomainServices.Repositories.SSO;
using Core.DomainServices.SSO;
using dk.nita.saml20.identity;
using Infrastructure.Services.Cryptography;

using Serilog;

namespace Core.ApplicationServices.SSO.Factories
{
    public class SsoStateFactory : ISsoStateFactory
    {
        private readonly IStsBrugerInfoService _infoService;
        private readonly ISsoUserIdentityRepository _ssoUserIdentityRepository;
        private readonly IStsOrganizationIdentityRepository _stsOrganizationIdentityRepository;
        private readonly Maybe<ISaml20Identity> _samlState;
        private readonly IUserRepository _userRepository;
        private readonly SsoFlowConfiguration _configuration;
        private readonly IApplicationAuthenticationState _applicationAuthenticationState;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IOrganizationRoleService _organizationRoleService;
        private readonly ICryptoService _cryptoService;
        private readonly ILogger _logger;

        public SsoStateFactory(
            IStsBrugerInfoService infoService,
            ISsoUserIdentityRepository ssoUserIdentityRepository,
            IStsOrganizationIdentityRepository stsOrganizationIdentityRepository,
            Maybe<ISaml20Identity> samlState,
            IUserRepository userRepository,
            SsoFlowConfiguration configuration,
            IApplicationAuthenticationState applicationAuthenticationState,
            IOrganizationRepository organizationRepository,
            IOrganizationRoleService organizationRoleService,
            ILogger logger, ICryptoService cryptoService)
        {
            _infoService = infoService;
            _ssoUserIdentityRepository = ssoUserIdentityRepository;
            _stsOrganizationIdentityRepository = stsOrganizationIdentityRepository;
            _samlState = samlState;
            _userRepository = userRepository;
            _configuration = configuration;
            _applicationAuthenticationState = applicationAuthenticationState;
            _organizationRepository = organizationRepository;
            _organizationRoleService = organizationRoleService;
            _logger = logger;
            _cryptoService = cryptoService;
        }

        public AbstractState CreateInitialState()
        {
            if (_samlState.IsNone)
            {
                throw new InvalidOperationException("Error: No SAML state");
            }
            return new InitialFlowState(_configuration, Saml20IdentityParser.CreateFrom(_samlState.Value), this, _logger);
        }

        public AbstractState CreatePrivilegeVerifiedState(Guid userExternalUuid, string cvrNumber)
        {
            return new PrivilegeVerifiedState(userExternalUuid, cvrNumber, _userRepository, _infoService, _ssoUserIdentityRepository, this);
        }

        public AbstractState CreateUserLoggedIn(User user)
        {
            return new UserLoggedInState(user, _applicationAuthenticationState);
        }

        public AbstractState CreateUserIdentifiedState(User user, StsBrugerInfo stsBrugerInfo)
        {
            return new UserIdentifiedState(user, stsBrugerInfo, _ssoUserIdentityRepository, _stsOrganizationIdentityRepository, _organizationRepository, this, _logger);
        }

        public AbstractState CreateAuthorizingUserState(User user, Organization organization)
        {
            return new AuthorizingUserState(user, organization, _organizationRoleService, this);
        }

        public AbstractState CreateAuthorizingUserFromUnknownOrgState(User user)
        {
            return new AuthorizingUserFromUnknownOrgState(user, this);
        }

        public AbstractState CreateAssigningRoleState(User user, Organization ssoOrganization)
        {
            return new AssigningRoleState(user, ssoOrganization, _organizationRoleService, this);
        }

        public AbstractState CreateFirstTimeUserNotFoundState(StsBrugerInfo stsBrugerInfo)
        {
            return new FirstTimeUserNotFoundState(stsBrugerInfo, _organizationRepository, _userRepository, _organizationRoleService, _cryptoService, this);
        }

        public AbstractState CreateErrorState()
        {
            return new ErrorState(_logger);
        }
    }
}
