using System;
using System.Web.Http;
using System.Web.Http.Results;
using Core.ApplicationServices.SSO;
using Core.ApplicationServices.SSO.Model;
using Core.ApplicationServices.SSO.State;
using Presentation.Web.Infrastructure.Attributes;

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
            var finalState = _ssoFlowApplicationService.StartSsoLoginFlow();
            switch (finalState)
            {
                case ErrorState errorState:
                    return SsoError(errorState.ErrorCode);

                case UserLoggedInState _:
                    return LoggedIn();

                default:
                    return SsoError(SsoErrorCode.Unknown);
            }
        }

        private IHttpActionResult LoggedIn()
        {
            //Redirect to front page
            var uriBuilder = new UriBuilder(Request.RequestUri)
            {
                Path = string.Empty,
            };
            return Redirect(uriBuilder.Uri);
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