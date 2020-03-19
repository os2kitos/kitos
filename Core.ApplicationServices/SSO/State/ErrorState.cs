using System;
using Core.ApplicationServices.SSO.Model;
using Core.DomainModel.Result;

namespace Core.ApplicationServices.SSO.State
{
    public class ErrorState : AbstractState
    {
        public Maybe<SsoErrorCode> ErrorCode { get; private set; }
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(@event), @event, null);
            }
        }
    }
}