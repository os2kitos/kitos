using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Helpers;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using Core.ApplicationServices.Authentication;
using Infrastructure.Services.Types;
using Presentation.Web.Extensions;
using Presentation.Web.Helpers;
using Serilog;
using ActionFilterAttribute = System.Web.Http.Filters.ActionFilterAttribute;

namespace Presentation.Web.Infrastructure.Attributes
{
    public class RequireValidatedCSRFAttributed : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {

            var error = Maybe<string>.None;

            if (RequiresAntiforgeryCheck(actionContext))
            {
                error = ValidateCSRF(actionContext);
            }

            if (error.HasValue)
            {
                FailWith(actionContext, error.Value);
            }
            else
            {
                base.OnActionExecuting(actionContext);
            }
        }

        private static Maybe<string> ValidateCSRF(HttpActionContext actionContext)
        {
            var request = actionContext.Request;
            var logger = GetService<ILogger>(actionContext);
            var headers = request.Headers;

            if (!headers.TryGetValues(Constants.CSRFValues.HeaderName, out var xsrfToken))
            {
                return Maybe<string>.Some(Constants.CSRFValues.MissingXsrfHeaderError);
            }

            var tokenHeaderValue = xsrfToken.First();
            var cookieHeaderValue = GetCookie(request);

            if (cookieHeaderValue?.Value == null)
            {
                return Maybe<string>.Some(Constants.CSRFValues.MissingXsrfCookieError);
            }

            try
            {
                AntiForgery.Validate(cookieHeaderValue.Value, tokenHeaderValue);
            }
            catch (HttpAntiForgeryException e)
            {
                logger.Error(e.Message);
                return Maybe<string>.Some(Constants.CSRFValues.XsrfValidationFailedError);
            }

            return Maybe<string>.None;
        }

        private static bool RequiresAntiforgeryCheck(HttpActionContext actionContext)
        {
            return IgnoreCSRFCheck(actionContext) == false;
        }

        private static CookieState GetCookie(HttpRequestMessage request)
        {
            return request
                .Headers
                .GetCookies()
                .SelectMany(x => x.Cookies)
                .FirstOrDefault(c => c.Name == Constants.CSRFValues.CookieName);
        }

        private static bool IgnoreCSRFCheck(HttpActionContext actionContext)
        {
            return IsExternalApiRequest(actionContext) ||
                   IsReadOnlyRequest(actionContext) ||
                   CSRFCheckIsIgnoredOnTargetMethod(actionContext);
        }

        private static bool CSRFCheckIsIgnoredOnTargetMethod(HttpActionContext actionContext)
        {

            return
                actionContext.ActionDescriptor.GetCustomAttributes<IgnoreCSRFProtectionAttribute>().Any() ||
                actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<IgnoreCSRFProtectionAttribute>().Any();
        }

        private static bool IsExternalApiRequest(HttpActionContext actionContext)
        {
            var authenticationContext = GetService<IAuthenticationContext>(actionContext);
            return authenticationContext.Method == AuthenticationMethod.KitosToken;
        }

        private static bool IsReadOnlyRequest(HttpActionContext actionContext)
        {
            return !actionContext.Request.Method.Method.IsMutation();
        }

        private static void FailWith(HttpActionContext context, string errorMessage)
        {
            context.Response = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(errorMessage)
            };
        }

        private static T GetService<T>(HttpActionContext actionContext)
        {
            return (T)actionContext.ControllerContext.Configuration.DependencyResolver.GetService(typeof(T));
        }
    }
}