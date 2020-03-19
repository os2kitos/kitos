using Core.ApplicationServices.SSO.Factories;
using Core.ApplicationServices.SSO.State;

namespace Core.ApplicationServices.SSO
{
    public class SsoFlowApplicationService : ISsoFlowApplicationService
    {
        private readonly ISsoStateFactory _stateFactory;

        public SsoFlowApplicationService(ISsoStateFactory stateFactory)
        {
            _stateFactory = stateFactory;
        }

        public AbstractState StartSsoLoginFlow()
        {
            var initialState = _stateFactory.CreateInitialState();
            var flowContext = new FlowContext(initialState);
            flowContext.HandleLoginCompleted();
            return flowContext.CurrentState;
        }
    }
}
