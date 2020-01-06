using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Presentation.Web.Infrastructure.Attributes
{
    /// <summary>
    /// Deprecates an API route returning 410 - Gone on all calls to the action.
    /// TODO: Remove this class before PR
    /// </summary>
    public class DeprecatedApiAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            actionContext.Response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Gone,
                Content = new StringContent("Endpoint has been deprecated and is removed from the KITOS API")
            };

            base.OnActionExecuting(actionContext);
        }
    }
}