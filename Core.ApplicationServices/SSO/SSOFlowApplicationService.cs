using Core.ApplicationServices.SSO.State;

namespace Core.ApplicationServices.SSO
{
    public class SsoFlowApplicationService : ISsoFlowApplicationService
    {
        public AbstractState StartSsoLoginFlow()
        {
            AbstractState initialState = new InitialFlowState();
            var flowContext = new FlowContext(initialState);
            flowContext.HandleLoginCompleted();
            return flowContext.CurrentState;
        }
    }
}
