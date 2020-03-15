using Core.ApplicationServices.SSO.State;

namespace Core.ApplicationServices.SSO
{
    public interface ISsoFlowApplicationService
    {
        AbstractState StartSsoLoginFlow();
    }
}