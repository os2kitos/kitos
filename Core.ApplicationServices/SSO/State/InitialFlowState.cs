using System;
using Core.ApplicationServices.SSO.Factories;
using Core.ApplicationServices.SSO.Model;
using Core.DomainModel.Result;

namespace Core.ApplicationServices.SSO.State
{
    public class InitialFlowState : AbstractState
    {
        private readonly string _samlKitosReadAccessRoleIdentifier;
        private readonly Saml20IdentityParser _parser;
        private readonly ISsoStateFactory _stateFactory;

        public InitialFlowState(
            SsoFlowConfiguration configuration, 
            Saml20IdentityParser parser,
            ISsoStateFactory stateFactory)
        {
            _parser = parser;
            _stateFactory = stateFactory;
            _samlKitosReadAccessRoleIdentifier = $"{configuration.PrivilegePrefix}/roles/usersystemrole/readaccess/1";
        }

        public override void Handle(FlowEvent @event, FlowContext context)
        {
            if (@event.Equals(FlowEvent.LoginCompleted))
            {
                var externalUserUuid = GetUserExternalUuid();
                if (externalUserUuid.HasValue && CurrentUserHasKitosPrivilege())
                {
                    context.TransitionTo(_stateFactory.CreatePrivilegeVerifiedState(externalUserUuid.Value), 
                        _ => _.HandleUserPrivilegeVerified());
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