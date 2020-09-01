using Core.ApplicationServices.SSO.Model;
using Serilog;

namespace Core.ApplicationServices.SSO.State
{
    public class ErrorState : AbstractState
    {
        private readonly ILogger _logger;
        public SsoErrorCode ErrorCode { get; private set; }

        public ErrorState(ILogger logger)
        {
            _logger = logger;
        }

        public override void Handle(FlowEvent @event, FlowContext context)
        {
            _logger.Information("SSO entered {errorStateName} with error: {errorEvent}", nameof(ErrorState), @event);

            switch (@event)
            {
                case FlowEvent.UserPrivilegeInvalid:
                    ErrorCode = SsoErrorCode.MissingPrivilege;
                    break;
                case FlowEvent.NoOrganizationAndRole:
                    ErrorCode = SsoErrorCode.NoOrganizationAndRole;
                    break;
                case FlowEvent.UnableToResolveUserInStsOrganization:
                    ErrorCode = SsoErrorCode.UserNotFoundInSTS;
                    break;
                case FlowEvent.UnableToLocateUser:
                    ErrorCode = SsoErrorCode.UnableToCreateUserWithUnknownOrganization;
                    break;
                default:
                    ErrorCode = SsoErrorCode.Unknown;
                    break;
            }
        }
    }
}