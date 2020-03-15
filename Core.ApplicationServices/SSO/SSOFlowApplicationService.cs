using Core.ApplicationServices.SSO.State;

namespace Core.ApplicationServices.SSO
{
    public class SsoFlowApplicationService : ISsoFlowApplicationService
    {
        public AbstractState StartSsoLoginFlow()
        {
            AbstractState resultingState = new InitialFlowState();
            var flowContext = new FlowContext(resultingState);
            flowContext.HandleLoginCompleted();
            return resultingState;
        }
    }
}
