using System.Web.Http;
using Core.ApplicationServices.SSO;
using Core.ApplicationServices.SSO.State;
using dk.nita.saml20.identity;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.SSO
{
    [RoutePrefix("SSO")]
    public class SSOController: ApiController
    {
        private readonly ISsoFlowApplicationService _ssoFlowApplicationService;

        public SSOController(ISsoFlowApplicationService ssoFlowApplicationService)
        {
            _ssoFlowApplicationService = ssoFlowApplicationService;
        }

        [InternalApi]
        [HttpGet]
        [Route("")]
        public IHttpActionResult SSO()
        {
            var result = "User not authenticated";
            if (Saml20Identity.IsInitialized())
            {
                var currentIdentityName = Saml20Identity.Current.Name;
                var finalState = _ssoFlowApplicationService.StartSsoLoginFlow();
                // NB: Temporary code
                if (finalState is UserWithNoPrivilegesState)
                {
                    result = $"No Kitos privilegees for user '{currentIdentityName}'";
                }
                else if (finalState is UserSignedInState)
                {
                    result = $"User '{currentIdentityName}' has Kitos read access";
                }
            }
            return Ok(result);
        }
    }
}