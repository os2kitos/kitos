using Core.ApplicationServices.SSO.State;
using Core.DomainServices;
using Core.DomainServices.Repositories.SSO;
using Core.DomainServices.SSO;

namespace Core.ApplicationServices.SSO
{
    public class SsoFlowApplicationService : ISsoFlowApplicationService
    {
        private readonly IStsBrugerInfoService _infoService;
        private readonly ISsoUserIdentityRepository _ssoUserIdentityRepository;
        private readonly ISsoOrganizationIdentityRepository _ssoOrganizationIdentityRepository;
        private readonly IUserRepository _userRepository;
        private readonly SsoFlowConfiguration _configuration;

        public SsoFlowApplicationService(SsoFlowConfiguration configuration, 
            IStsBrugerInfoService infoService, 
            ISsoUserIdentityRepository ssoUserIdentityRepository, 
            IUserRepository userRepository, 
            ISsoOrganizationIdentityRepository ssoOrganizationIdentityRepository)
        {
            _infoService = infoService;
            _configuration = configuration;
            _ssoUserIdentityRepository = ssoUserIdentityRepository;
            _userRepository = userRepository;
            _ssoOrganizationIdentityRepository = ssoOrganizationIdentityRepository;
        }

        public AbstractState StartSsoLoginFlow()
        {
            AbstractState initialState = new InitialFlowState(_configuration, _infoService, _ssoUserIdentityRepository, _userRepository, _ssoOrganizationIdentityRepository);
            var flowContext = new FlowContext(initialState);
            flowContext.HandleLoginCompleted();
            return flowContext.CurrentState;
        }
    }
}
