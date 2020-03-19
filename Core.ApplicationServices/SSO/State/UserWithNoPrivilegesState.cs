using System;
namespace Core.ApplicationServices.SSO.State
{
    public class UserWithNoPrivilegesState : AbstractState
    {
        public override void Handle(FlowEvent @event, FlowContext context)
        {
            throw new NotSupportedException();
        }
    }
}