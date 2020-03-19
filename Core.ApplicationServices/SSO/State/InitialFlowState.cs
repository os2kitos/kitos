using System;
using Core.ApplicationServices.SSO.Model;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Repositories.SSO;
using Core.DomainServices.SSO;

namespace Core.ApplicationServices.SSO.State
{
    public class InitialFlowState : AbstractState
    {
        private readonly IStsBrugerInfoService _stsBrugerInfoService;
        private readonly ISsoUserIdentityRepository _ssoUserIdentityRepository;
        private readonly ISsoOrganizationIdentityRepository _ssoOrganizationIdentityRepository;
        private readonly IUserRepository _userRepository;
        private readonly string _samlKitosReadAccessRoleIdentifier;
        private readonly Saml20IdentityParser _parser;

        public InitialFlowState(SsoFlowConfiguration configuration, 
            IStsBrugerInfoService stsBrugerInfoService,
            ISsoUserIdentityRepository ssoUserIdentityRepository,
            IUserRepository userRepository, 
            ISsoOrganizationIdentityRepository ssoOrganizationIdentityRepository)
        {
            _stsBrugerInfoService = stsBrugerInfoService;
            _ssoUserIdentityRepository = ssoUserIdentityRepository;
            _userRepository = userRepository;
            _ssoOrganizationIdentityRepository = ssoOrganizationIdentityRepository;
            _parser = Saml20IdentityParser.CreateFromContext();
            _samlKitosReadAccessRoleIdentifier = $"{configuration.PrivilegePrefix}/roles/usersystemrole/readaccess/1";
        }

        public override void Handle(FlowEvent @event, FlowContext context)
        {
            if (@event.Equals(FlowEvent.LoginCompleted))
            {
                var externalUserUuid = GetUserExternalUuid();
                if (externalUserUuid.HasValue && CurrentUserHasKitosPrivilege())
                {
                    context.TransitionTo(new PrivilegeVerifiedState(externalUserUuid.Value, _userRepository, _stsBrugerInfoService, _ssoUserIdentityRepository, _ssoOrganizationIdentityRepository));
                    context.HandleUserPrivilegeVerified();
                }
                else
                {
                    context.TransitionTo(new ErrorState());
                }
            }
        }

        private Maybe<Guid> GetUserExternalUuid()
        {
            return _parser.MatchUuid().Select(x => x.Value);
        }

        private bool CurrentUserHasKitosPrivilege()
        {
            return _parser
                .MatchPrivilege(_samlKitosReadAccessRoleIdentifier)
                .HasValue;
        }
    }
}