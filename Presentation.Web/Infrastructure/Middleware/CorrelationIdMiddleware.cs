using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace Presentation.Web.Infrastructure.Middleware
{
    public class CorrelationIdMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var correlationId = Guid.NewGuid();
            using (LogContext.PushProperty("CorrelationId", correlationId.ToString()))
            {
                await next(context);
            }
        }
    }
}
