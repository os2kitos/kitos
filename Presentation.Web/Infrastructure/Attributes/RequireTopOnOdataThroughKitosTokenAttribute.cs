using Core.ApplicationServices.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Presentation.Web.Infrastructure.Attributes
{
    public class RequireTopOnOdataThroughKitosTokenAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            var authContext = actionContext.HttpContext.RequestServices
                .GetRequiredService<IAuthenticationContext>();

            if (actionContext.HttpContext.Request.Path.Value?.Contains("/odata/") == true &&
                authContext.Method == AuthenticationMethod.KitosToken)
            {
                var topPresent = actionContext.HttpContext.Request.Query.ContainsKey("$top");
                if (!topPresent)
                {
                    actionContext.Result = new BadRequestObjectResult("Pagination required. Missing 'top' query parameter on ODATA request");
                    return;
                }
            }
            base.OnActionExecuting(actionContext);
        }
    }
}
