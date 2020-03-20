using Core.ApplicationServices.SSO.State;
using Core.DomainServices.SSO;

namespace Core.ApplicationServices.SSO
{
    public class SsoFlowApplicationService : ISsoFlowApplicationService
    {
        private readonly IStsBrugerEmailService _emailService;
        private readonly SsoFlowConfiguration _configuration;

        public SsoFlowApplicationService(IStsBrugerEmailService emailService, SsoFlowConfiguration configuration)
        {
            _emailService = emailService;
            _configuration = configuration;
        }

        public AbstractState StartSsoLoginFlow()
        {
            AbstractState initialState = new InitialFlowState(_emailService, _configuration);
            var flowContext = new FlowContext(initialState);
            flowContext.HandleLoginCompleted();
            return flowContext.CurrentState;
        }
    }
}
