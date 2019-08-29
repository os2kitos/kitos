using System.Web.Http.Filters;
using Serilog;

namespace Presentation.Web.Infrastructure
{
    public class ExceptionLogFilterAttribute : ExceptionFilterAttribute
    {
        readonly ILogger _logger = Log.Logger.ForContext<ExceptionLogFilterAttribute>();
        public override void OnException(HttpActionExecutedContext context)
        {
            _logger.Error(context.Exception, $"Exception occured in {context.ActionContext.ControllerContext.ControllerDescriptor.ControllerName}");
            base.OnException(context);
        }
    }
}