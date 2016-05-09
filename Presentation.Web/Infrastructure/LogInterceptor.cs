using Ninject.Extensions.Interception;
using Ninject.Extensions.Logging;

namespace Presentation.Web.Infrastructure
{
    public class LogInterceptor : SimpleInterceptor
    {
        private readonly ILogger _logger;

        public LogInterceptor(ILogger logger)
        {

            _logger = logger;
        }

        protected override void BeforeInvoke(IInvocation invocation)
        {

        }

        protected override void AfterInvoke(IInvocation invocation)
        {

            _logger.Info("Method: {Name} called with arguments {@Arguments}",
                    invocation.Request.Target +"."+ invocation.Request.Method.Name,
                    invocation.Request.Arguments);
        }
    }
}