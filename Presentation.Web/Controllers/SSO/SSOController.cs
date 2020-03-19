using System;
using System.Web.Http;
using System.Web.Http.Results;
using Core.ApplicationServices.SSO;
using Core.ApplicationServices.SSO.State;
using dk.nita.saml20.identity;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.SSO;

namespace Presentation.Web.Controllers.SSO
{
    [RoutePrefix("SSO")]
    public class SSOController : ApiController
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

                switch (finalState)
                {
                    case ErrorState _:
                        return SsoError(SsoErrorCode.MissingPrivilege);
                    case UserLoggedInState _:
                        result = $"User '{currentIdentityName}' has Kitos read access";
                        break;
                }
            }
            return Ok(result);
        }

        private RedirectResult SsoError(SsoErrorCode error)
        {
            var uriBuilder = new UriBuilder(Request.RequestUri)
            {
                Path = "Home/SsoError",
                Query = $"ssoErrorCode={error:G}"
            };
            return Redirect(uriBuilder.Uri);
        }
    }
}