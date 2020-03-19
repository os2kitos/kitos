using Core.ApplicationServices.SSO.State;
using Core.DomainServices.Repositories.SSO;
using Core.DomainServices.SSO;

namespace Core.ApplicationServices.SSO
{
    public class SsoFlowApplicationService : ISsoFlowApplicationService
    {
        private readonly IStsBrugerInfoService _infoService;
        private readonly ISsoUserIdentityRepository _ssoUserIdentityRepository;
        private readonly SsoFlowConfiguration _configuration;

        public SsoFlowApplicationService(IStsBrugerInfoService infoService, SsoFlowConfiguration configuration, ISsoUserIdentityRepository ssoUserIdentityRepository)
        {
            _infoService = infoService;
            _configuration = configuration;
            _ssoUserIdentityRepository = ssoUserIdentityRepository;
        }

        public AbstractState StartSsoLoginFlow()
        {
            AbstractState initialState = new InitialFlowState(_infoService, _configuration, _ssoUserIdentityRepository);
            var flowContext = new FlowContext(initialState);
            flowContext.HandleLoginCompleted();
            return flowContext.CurrentState;
        }
    }
}
