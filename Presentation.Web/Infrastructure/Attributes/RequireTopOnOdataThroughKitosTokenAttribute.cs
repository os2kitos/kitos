using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Core.ApplicationServices.Authentication;
using Core.ApplicationServices.Model;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;

namespace Presentation.Web.Infrastructure.Attributes
{
    public class RequireTopOnOdataThroughKitosTokenAttribute : ActionFilterAttribute
    {
        private const int MaxPageSize = 100;

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var authContext = (IAuthenticationContext)actionContext.ControllerContext.Configuration.DependencyResolver.GetService(typeof(IAuthenticationContext));
            if (actionContext.Request.RequestUri.AbsoluteUri.Contains("/odata/") && authContext.Method == AuthenticationMethod.KitosToken)
            {
                var enableQueryAttributes = actionContext.ActionDescriptor.GetCustomAttributes<EnableQueryAttribute>().First();
                enableQueryAttributes.PageSize = MaxPageSize;
            }
            base.OnActionExecuting(actionContext);
            
        }
    }
}