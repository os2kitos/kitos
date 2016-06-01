using Ninject.Extensions.Interception;
using Serilog;

namespace Presentation.Web.Infrastructure
{
    public class LogInterceptor : SimpleInterceptor
    {
        private readonly ILogger _logger;

        public LogInterceptor()
        {

            _logger = Log.Logger;
        }

        protected override void BeforeInvoke(IInvocation invocation)
        {

        }

        protected override void AfterInvoke(IInvocation invocation)
        {

            _logger.Information("Method: {Name} called with arguments {@Arguments}",
                    invocation.Request.Target +"."+ invocation.Request.Method.Name,
                    invocation.Request.Arguments);
        }
    }
}