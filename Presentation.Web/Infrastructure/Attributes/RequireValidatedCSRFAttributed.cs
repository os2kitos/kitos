using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authentication;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Web.Extensions;
using Presentation.Web.Helpers;
using Serilog;

namespace Presentation.Web.Infrastructure.Attributes
{
    public class RequireValidatedCSRFAttributed : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            var error = Maybe<string>.None;

            if (RequiresAntiforgeryCheck(actionContext))
            {
                error = ValidateCSRF(actionContext);
            }

            if (error.HasValue)
            {
                actionContext.Result = new BadRequestObjectResult(error.Value);
            }
            else
            {
                base.OnActionExecuting(actionContext);
            }
        }

        private static Maybe<string> ValidateCSRF(ActionExecutingContext actionContext)
        {
            var request = actionContext.HttpContext.Request;
            var logger = actionContext.HttpContext.RequestServices.GetRequiredService<ILogger>();

            if (!request.Headers.TryGetValue(Constants.CSRFValues.HeaderName, out var xsrfToken))
                return Maybe<string>.Some(Constants.CSRFValues.MissingXsrfHeaderError);

            var tokenHeaderValue = xsrfToken.FirstOrDefault();
            var cookieHeaderValue = request.Cookies[Constants.CSRFValues.CookieName];

            if (cookieHeaderValue == null)
                return Maybe<string>.Some(Constants.CSRFValues.MissingXsrfCookieError);

            if (tokenHeaderValue != cookieHeaderValue)
            {
                logger.Error("XSRF token mismatch");
                return Maybe<string>.Some(Constants.CSRFValues.XsrfValidationFailedError);
            }

            return Maybe<string>.None;
        }

        private static bool RequiresAntiforgeryCheck(ActionExecutingContext actionContext)
        {
            return !IgnoreCSRFCheck(actionContext);
        }

        private static bool IgnoreCSRFCheck(ActionExecutingContext actionContext)
        {
            return IsExternalApiRequest(actionContext) ||
                   IsReadOnlyRequest(actionContext) ||
                   CSRFCheckIsIgnoredOnTargetMethod(actionContext);
        }

        private static bool CSRFCheckIsIgnoredOnTargetMethod(ActionExecutingContext actionContext)
        {
            return actionContext.ActionDescriptor.EndpointMetadata
                .OfType<IgnoreCSRFProtectionAttribute>()
                .Any();
        }

        private static bool IsExternalApiRequest(ActionExecutingContext actionContext)
        {
            var authenticationContext = actionContext.HttpContext.RequestServices
                .GetRequiredService<IAuthenticationContext>();
            return authenticationContext.Method == AuthenticationMethod.KitosToken;
        }

        private static bool IsReadOnlyRequest(ActionExecutingContext actionContext)
        {
            return !actionContext.HttpContext.Request.Method.IsMutation();
        }
    }
}
