using Core.ApplicationServices.SSO.State;
using Core.DomainServices.SSO;

namespace Core.ApplicationServices.SSO
{
    public class SsoFlowApplicationService : ISsoFlowApplicationService
    {
        private readonly IStsBrugerInfoService _infoService;
        private readonly SsoFlowConfiguration _configuration;

        public SsoFlowApplicationService(IStsBrugerInfoService infoService, SsoFlowConfiguration configuration)
        {
            _infoService = infoService;
            _configuration = configuration;
        }

        public AbstractState StartSsoLoginFlow()
        {
            AbstractState initialState = new InitialFlowState(_infoService, _configuration);
            var flowContext = new FlowContext(initialState);
            flowContext.HandleLoginCompleted();
            return flowContext.CurrentState;
        }
    }
}
