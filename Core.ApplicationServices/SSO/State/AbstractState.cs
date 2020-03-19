namespace Core.ApplicationServices.SSO.State
{
    public abstract class AbstractState
    {
        public abstract void Handle(FlowEvent @event, FlowContext context);
    }
}
