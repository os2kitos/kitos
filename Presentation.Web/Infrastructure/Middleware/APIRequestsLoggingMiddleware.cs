using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Owin;
using Ninject;
using Presentation.Web.Infrastructure.Model.Authentication;
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
                var startTime = DateTime.Now;
                var route = context.Request.Path;
                var method = context.Request.Method;
                var queryParameters = GetQueryParameters(context.Request.Query);

                var userId = authenticationContext.UserId.GetValueOrDefault(INVALID_ID);
                var activeOrganizationId = authenticationContext.ActiveOrganizationId.GetValueOrDefault(INVALID_ID);

                await Next.Invoke(context);
                var processingTime = DateTime.Now - startTime;
                logger.Information("Route: {route} Method: {method} QueryParameters: {queryParameters} UserID: {userID} ProcessingTime: {processingTime}ms ActiveOrganizationId: {activeOrganizationId}", route, method, queryParameters, userId, processingTime.TotalMilliseconds, activeOrganizationId);
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