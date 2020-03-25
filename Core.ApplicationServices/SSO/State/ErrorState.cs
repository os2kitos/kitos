using Core.ApplicationServices.SSO.Model;

namespace Core.ApplicationServices.SSO.State
{
    public class ErrorState : AbstractState
    {
        public SsoErrorCode ErrorCode { get; private set; }

        public override void Handle(FlowEvent @event, FlowContext context)
        {
            switch (@event)
            {
                case FlowEvent.UserPrivilegeInvalid:
                    ErrorCode = SsoErrorCode.MissingPrivilege;
                    break;
                case FlowEvent.NoOrganizationAndRole:
                    ErrorCode = SsoErrorCode.NoOrganizationAndRole;
                    break;
                case FlowEvent.UnableToResolveUserInStsOrganisation:
                    ErrorCode = SsoErrorCode.UserNotFoundInSTS;
                    break;
                case FlowEvent.UnsupportedFlow:
                    ErrorCode = SsoErrorCode.UnsupportedFlow;
                    break;
                default:
                    ErrorCode = SsoErrorCode.Unknown;
                    break;
            }
        }
    }
}