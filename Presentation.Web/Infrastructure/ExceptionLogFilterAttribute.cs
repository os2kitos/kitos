using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;

namespace Presentation.Web.Infrastructure
{
    public class ExceptionLogFilterAttribute : ExceptionFilterAttribute
    {
        readonly ILogger _logger = Log.Logger.ForContext<ExceptionLogFilterAttribute>();

        public override void OnException(ExceptionContext context)
        {
            _logger.Error(context.Exception, $"Exception occurred in {context.RouteData.Values["controller"]}");
            base.OnException(context);
        }
    }
}
