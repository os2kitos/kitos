using Core.ApplicationServices.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Presentation.Web.Infrastructure.Attributes
{
    public class InternalApiAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            var authContext = actionContext.HttpContext.RequestServices
                .GetRequiredService<IAuthenticationContext>();

            if (authContext.Method == AuthenticationMethod.KitosToken)
            {
                actionContext.Result = new ObjectResult("Internal endpoint. Please use the public API.")
                {
                    StatusCode = 403
                };
                return;
            }
            base.OnActionExecuting(actionContext);
        }
    }
}
