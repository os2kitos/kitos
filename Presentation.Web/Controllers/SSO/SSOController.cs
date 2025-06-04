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
    [InternalApi]
    public class SSOController : ApiController
    {
        private readonly ISsoFlowApplicationService _ssoFlowApplicationService;
        private readonly ILogger _logger;

        public SSOController(ISsoFlowApplicationService ssoFlowApplicationService, ILogger logger)
        {
            _ssoFlowApplicationService = ssoFlowApplicationService;
            _logger = logger;
        }

        [HttpGet]
        [Route("")]
        public IHttpActionResult SSO()
        {
            try
            {
                _logger.Debug("Starting SSO flow");
                var finalState = _ssoFlowApplicationService.StartSsoLoginFlow();
                switch (finalState)
                {
                    case ErrorState errorState:
                        _logger.Information("SSO Login failed with error: {errorCode}", errorState.ErrorCode);
                        return SsoError(errorState.ErrorCode);

                    case UserLoggedInState loggedInState:
                        _logger.Information("SSO Login completed with success for user with id: {userId}", loggedInState.User.Id);
                        return LoggedIn();
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unknown error in SSO flow: {errorCode}", e.Message);
            }
            // Shut down gracefully
            return SsoError(SsoErrorCode.Unknown);
        }

        private IHttpActionResult LoggedIn()
        {
            //Redirect to front page
            var uriBuilder = new UriBuilder(Request.RequestUri)
            {
                Path = "ui"
            };
            return Redirect(uriBuilder.Uri);
        }

        private RedirectResult SsoError(SsoErrorCode error)
        {
            var uriBuilder = new UriBuilder(Request.RequestUri)
            {
                Path = "ui",
                Query = $"ssoErrorCode={error:G}"
            };
            return Redirect(uriBuilder.Uri);
        }
    }
}