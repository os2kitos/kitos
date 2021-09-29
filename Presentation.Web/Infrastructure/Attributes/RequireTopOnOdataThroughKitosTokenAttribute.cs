using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Core.ApplicationServices.Authentication;

namespace Presentation.Web.Infrastructure.Attributes
{
    public class RequireTopOnOdataThroughKitosTokenAttribute : ActionFilterAttribute
    {

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var authContext = (IAuthenticationContext)actionContext.ControllerContext.Configuration.DependencyResolver.GetService(typeof(IAuthenticationContext));
            if (actionContext.Request.RequestUri.AbsoluteUri.Contains("/odata/") && authContext.Method == AuthenticationMethod.KitosToken)
            {
                if (!actionContext.Request.RequestUri.AbsoluteUri.Contains("$top="))
                {
                    actionContext.Response = new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Content = new StringContent("Pagination required. Missing 'top' query parameter on ODATA request")
                    };
                }
            }
            base.OnActionExecuting(actionContext);
        }

    }
}