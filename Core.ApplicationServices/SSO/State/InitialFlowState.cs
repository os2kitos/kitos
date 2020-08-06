using System;
using Core.ApplicationServices.SSO.Factories;
using Core.ApplicationServices.SSO.Model;
using Infrastructure.Services.Types;
using Serilog;

namespace Core.ApplicationServices.SSO.State
{
    public class InitialFlowState : AbstractState
    {
        private readonly string _samlKitosReadAccessRoleIdentifier;
        private readonly Saml20IdentityParser _parser;
        private readonly ISsoStateFactory _stateFactory;
        private readonly ILogger _logger;

        public InitialFlowState(
            SsoFlowConfiguration configuration,
            Saml20IdentityParser parser,
            ISsoStateFactory stateFactory,
            ILogger logger)
        {
            _parser = parser;
            _stateFactory = stateFactory;
            _logger = logger;
            _samlKitosReadAccessRoleIdentifier = $"{configuration.PrivilegePrefix}/roles/usersystemrole/readaccess/1";
        }

        public override void Handle(FlowEvent @event, FlowContext context)
        {
            if (@event.Equals(FlowEvent.LoginCompleted))
            {
                var externalUserUuid = GetUserExternalUuid();
                var cvrNumber = _parser.MatchCvrNumber();
                if (externalUserUuid.IsNone)
                {
                    _logger.Error("No external UUID passed from STS Adgangsstyring");
                    context.TransitionTo(_stateFactory.CreateErrorState(), _ => _.HandleUnknownError());
                }
                else if (cvrNumber.IsNone)
                {
                    _logger.Error("CVR number not provided from STS Adgangsstyring");
                    context.TransitionTo(_stateFactory.CreateErrorState(), _ => _.HandleUnknownError());
                }
                else if (CurrentUserHasKitosPrivilege())
                {
                    context.TransitionTo(_stateFactory.CreatePrivilegeVerifiedState(externalUserUuid.Value, cvrNumber.Value), _ => _.HandleUserPrivilegeVerified());
                }
                else
                {
                    context.TransitionTo(_stateFactory.CreateErrorState(), _ => _.HandleUserPrivilegeInvalid());
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