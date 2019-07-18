using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Owin;
using Ninject;
using Serilog;

namespace Presentation.Web.Infrastructure
{
    public class ApiRequestsLoggingMiddleware : OwinMiddleware
    {

        private ILogger _logger = Log.Logger;

        public ApiRequestsLoggingMiddleware(OwinMiddleware next) : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            _logger = context.GetNinjectKernel().Get<ILogger>();
            if (context.Request.Headers.ContainsKey("Authorization"))
            {
                var startTime = DateTime.Now;
                var route = context.Request.Path;
                var method = context.Request.Method;
                var queryParameters = getQueryParameters(context.Request.Query);
                var userID = context.Request.User.Identity.Name;
                await Next.Invoke(context);
                var processingTime = DateTime.Now - startTime;
                _logger.Information("Route: {route} Method: {method} QueryParameters: {queryParameters} UserID: {userID} ProcessingTime: {processingTime}ms", route, method, queryParameters, userID, processingTime.TotalMilliseconds);
            }
            else
            {
                await Next.Invoke(context);
            }
        }

        private static string getQueryParameters(IReadableStringCollection query)
        {
            if (query.Any())
            {
                var parameters = query.Select(i => i.Key).Aggregate((i, j) => i + ", " + j);
                return parameters;
            }
            return "";
        }
    }
}