using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Core.ApplicationServices.Authentication;

namespace Presentation.Web.Infrastructure.Attributes
{
    public class InternalApiAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var authContext = (IAuthenticationContext)actionContext.ControllerContext.Configuration.DependencyResolver.GetService(typeof(IAuthenticationContext));

            if (authContext.Method == AuthenticationMethod.KitosToken)
            {
                actionContext.Response = new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.Forbidden,
                    Content = new StringContent("Det er ikke tilladt at benytte dette endpoint")
                };

            }
            base.OnActionExecuting(actionContext);
        }
    }
}
