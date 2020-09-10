using System;
using System.Linq;
using System.Text;
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
                    _logger.Warning("No external UUID passed from STS Adgangsstyring");
                    context.TransitionTo(_stateFactory.CreateErrorState(), _ => _.HandleUnknownError());
                }
                else if (cvrNumber.IsNone)
                {
                    _logger.Warning("CVR number not provided from STS Adgangsstyring");
                    context.TransitionTo(_stateFactory.CreateErrorState(), _ => _.HandleUnknownError());
                }
                else if (CurrentUserHasKitosPrivilege())
                {
                    _logger.Debug("User with UUID {uuid} and CVR {cvr} did have privilege", externalUserUuid.Value, cvrNumber.Value);
                    context.TransitionTo(_stateFactory.CreatePrivilegeVerifiedState(externalUserUuid.Value, cvrNumber.Value), _ => _.HandleUserPrivilegeVerified());
                }
                else
                {
                    var privileges = GetPrivilegesString();
                    _logger.Information("Missing privilege for user with UUID {uuid} and CVR {cvr}. Failed with XML privileges {xmlPrivilegesBase64}", externalUserUuid.Value, cvrNumber.Value, privileges);

                    context.TransitionTo(_stateFactory.CreateErrorState(), _ => _.HandleUserPrivilegeInvalid());
                }
            }
        }

        private string GetPrivilegesString()
        {
            try
            {
                var privileges =
                    Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Join(" -- ",
                        _parser.Navigator.GetPrivilegeNodes().Select(x => x.InnerXml))));
                return privileges;
            }
            catch (Exception e)
            {
                _logger.Error(e,"Failed to joind privileges");
            }

            return "ERROR creating privileges as string";

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