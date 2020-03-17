using Core.ApplicationServices.SSO.State;
using Core.DomainServices.SSO;

namespace Core.ApplicationServices.SSO
{
    public class SsoFlowApplicationService : ISsoFlowApplicationService
    {
        private readonly IStsBrugerEmailService _emailService;

        public SsoFlowApplicationService(IStsBrugerEmailService emailService)
        {
            _emailService = emailService;
        }

        public AbstractState StartSsoLoginFlow()
        {
            AbstractState initialState = new InitialFlowState(_emailService);
            var flowContext = new FlowContext(initialState);
            flowContext.HandleLoginCompleted();
            return flowContext.CurrentState;
        }
    }
}
