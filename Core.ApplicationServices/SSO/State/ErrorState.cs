using System;
namespace Core.ApplicationServices.SSO.State
{
    public class ErrorState : AbstractState
    {
        public override void Handle(FlowEvent @event, FlowContext context)
        {
            switch (@event)
            {
                case FlowEvent.UserPrivilegeInvalid:
                case FlowEvent.NoOrganizationAndRole:
                    // Do nothing
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@event), @event, null);
            }
        }
    }
}