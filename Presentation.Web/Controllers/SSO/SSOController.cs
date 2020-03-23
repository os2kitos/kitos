using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
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
        //[RemoveSamlCookieFilter]
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

    public class RemoveSamlCookieFilter : ActionFilterAttribute 
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var request = actionContext.Request;
            var response = actionContext.Request.CreateResponse();

            var currentCookie = request.Headers.GetCookies("oiosamlSession").FirstOrDefault();
            if (currentCookie != null)
            {
                var cookie = new CookieHeaderValue("oiosamlSession", "")
                {
                    Expires = DateTimeOffset.Now.AddDays(-1),
                    Domain = currentCookie.Domain,
                    Path = currentCookie.Path,
                    HttpOnly = currentCookie.HttpOnly,
                    Secure = currentCookie.Secure
                };

                response.Headers.AddCookies(new[] { cookie });
            }
         
            base.OnActionExecuting(actionContext);
        }
    }
}