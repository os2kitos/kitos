using System;
using System.Web.Http;
using System.Web.Http.Results;
using Core.ApplicationServices.SSO;
using Core.ApplicationServices.SSO.Model;
using Core.ApplicationServices.SSO.State;
using Presentation.Web.Infrastructure.Attributes;
using Serilog;

namespace Presentation.Web.Controllers.SSO
{
    [RoutePrefix("SSO")]
    public class SSOController : ApiController
    {
        private readonly ISsoFlowApplicationService _ssoFlowApplicationService;
        private readonly ILogger _logger;

        public SSOController(ISsoFlowApplicationService ssoFlowApplicationService, ILogger logger)
        {
            _ssoFlowApplicationService = ssoFlowApplicationService;
            _logger = logger;
        }

        [InternalApi]
        [HttpGet]
        [Route("")]
        public IHttpActionResult SSO()
        {
            try
            {
                var finalState = _ssoFlowApplicationService.StartSsoLoginFlow();
                switch (finalState)
                {
                    case ErrorState errorState:
                        return SsoError(errorState.ErrorCode);

                    case UserLoggedInState _:
                        return LoggedIn();
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unknown error in SSO flow");
            }
            // Shut down gracefully
            return SsoError(SsoErrorCode.Unknown);
        }

        private IHttpActionResult LoggedIn()
        {
            //Redirect to front page
            var uriBuilder = new UriBuilder(Request.RequestUri)
            {
                Path = "Home/SsoAuthenticated"
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