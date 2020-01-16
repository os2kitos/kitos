using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Helpers;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using ActionFilterAttribute = System.Web.Http.Filters.ActionFilterAttribute;

namespace Presentation.Web.Infrastructure.Attributes
{

    public class AntiForgeryValidationFilterAttribute : ActionFilterAttribute
    {
        private const string XsrfHeader = "HEADER-XSRF-TOKEN";
        private const string XsrfCookie = "XSRF-TOKEN";

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var headers = actionContext.Request.Headers;

            if (!headers.TryGetValues(XsrfHeader, out var xsrfToken))
            {
                actionContext.Response.StatusCode = HttpStatusCode.BadRequest;
                actionContext.Response.Content = new StringContent("Manglende xsrf token");
                return;
            }

            var tokenHeaderValue = xsrfToken.First();
            var tokenCookie = headers.GetCookies().Select(c => c[XsrfCookie]).FirstOrDefault();

            //var tokenCookie = actionContext.Request.Cookies.FirstOrDefault(c => c.Key == XsrfCookie);

            if (tokenCookie == null)
            {
                actionContext.Response.StatusCode = HttpStatusCode.BadRequest;
                return;
            }

            try
            {
                AntiForgery.Validate(tokenCookie.Value, tokenHeaderValue);
                base.OnActionExecuting(actionContext);
            }
            catch (HttpAntiForgeryException e)
            {

                actionContext.Response.StatusCode = HttpStatusCode.BadRequest;
            }
        }
    }
}