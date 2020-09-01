using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Serilog.Context;

namespace Presentation.Web.Infrastructure.Middleware
{
    public class CorrelationIdMiddleware : OwinMiddleware
    {
        public CorrelationIdMiddleware(OwinMiddleware next) : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            var kernel = context.GetNinjectKernel();
            var correlationId = Guid.NewGuid();
            using (LogContext.PushProperty("CorrelationId", correlationId.ToString()))
            {
                await Next.Invoke(context);
            }
        }
    }
}