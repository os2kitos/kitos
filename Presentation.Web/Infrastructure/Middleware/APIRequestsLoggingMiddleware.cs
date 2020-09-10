using System;
using System.Linq;
using System.Threading.Tasks;
using Core.ApplicationServices.Authentication;
using Microsoft.Owin;
using Ninject;
using Serilog;

namespace Presentation.Web.Infrastructure.Middleware
{
    public class ApiRequestsLoggingMiddleware : OwinMiddleware
    {
        private const int INVALID_ID = -1;
        public ApiRequestsLoggingMiddleware(OwinMiddleware next) : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            var kernel = context.GetNinjectKernel();
            var logger = kernel.Get<ILogger>();
            var authenticationContext = kernel.Get<IAuthenticationContext>();
            if (authenticationContext.Method == AuthenticationMethod.KitosToken)
            {
                var requestStart = DateTime.UtcNow;
                var route = context.Request.Path;
                var method = context.Request.Method;
                var queryParameters = GetQueryParameters(context.Request.Query);
                var userId = authenticationContext.UserId.GetValueOrDefault(INVALID_ID);
                logger.Information("Route: {route} Method: {method} QueryParameters: {queryParameters} UserID: {userID} RequestStartUTC: {requestStart}", route, method, queryParameters, userId, requestStart);
                try
                {
                    await Next.Invoke(context);
                }
                finally
                {
                    var requestEnd = DateTime.UtcNow;
                    logger.Information("Route: {route} Method: {method} QueryParameters: {queryParameters} UserID: {userID} RequestEndUTC: {requestEnd}", route, method, queryParameters, userId, requestEnd);
                }
            }
            else
            {
                await Next.Invoke(context);
            }
        }

        private static string GetQueryParameters(IReadableStringCollection query)
        {
            if (query.Any())
            {
                var parameters = query.Select(i => i.Key).Aggregate((i, j) => i + ", " + j);
                return parameters;
            }
            return string.Empty;
        }
    }
}