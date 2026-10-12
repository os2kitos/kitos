using System;
using Core.ApplicationServices.SSO;
using Core.ApplicationServices.SSO.Model;
using Core.ApplicationServices.SSO.State;
using Microsoft.AspNetCore.Mvc;
using Presentation.Web.Infrastructure.Attributes;
using Serilog;

namespace Presentation.Web.Controllers.SSO
{
    [Route("SSO")]
    [InternalApi]
    public class SSOController : ControllerBase
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
        public IActionResult SSO()
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
            return SsoError(SsoErrorCode.Unknown);
        }

        private IActionResult LoggedIn()
        {
            return Redirect("/ui");
        }

        private RedirectResult SsoError(SsoErrorCode error)
        {
            return Redirect($"/ui?ssoErrorCode={error:G}");
        }
    }
}
