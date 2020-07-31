using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Core.ApplicationServices.Authentication;
using Serilog;

namespace Presentation.Web.Infrastructure.Attributes
{
    public class RequireTopOnOdataThroughKitosTokenAttribute : ActionFilterAttribute
    {

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var authContext = (IAuthenticationContext)actionContext.ControllerContext.Configuration.DependencyResolver.GetService(typeof(IAuthenticationContext));
            var logger = (ILogger)actionContext.ControllerContext.Configuration.DependencyResolver.GetService(typeof(ILogger));
            if (actionContext.Request.RequestUri.AbsoluteUri.Contains("/odata/") && authContext.Method == AuthenticationMethod.KitosToken)
            {
                if (!actionContext.Request.RequestUri.AbsoluteUri.Contains("$top="))
                {
                    logger.Warning("Request asks for data via ODATA without \"top\" argument to limit entries");
                }
                base.OnActionExecuting(actionContext);
            }
            else
            {
                base.OnActionExecuting(actionContext);
            }
        }

    }
}