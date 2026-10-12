using System;
using System.Linq;
using System.Threading.Tasks;
using Core.ApplicationServices.Authentication;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace Presentation.Web.Infrastructure.Middleware
{
    public class ApiRequestsLoggingMiddleware : IMiddleware
    {
        private const int INVALID_ID = -1;
        private readonly ILogger _logger;
        private readonly IAuthenticationContext _authenticationContext;

        public ApiRequestsLoggingMiddleware(ILogger logger, IAuthenticationContext authenticationContext)
        {
            _logger = logger;
            _authenticationContext = authenticationContext;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (_authenticationContext.Method == AuthenticationMethod.KitosToken)
            {
                var requestStart = DateTime.UtcNow;
                var route = context.Request.Path;
                var method = context.Request.Method;
                var queryParameters = GetQueryParameters(context.Request.Query);
                var sanitizedRoute = SanitizeForLogging(route);
                var sanitizedQueryParameters = SanitizeForLogging(queryParameters);
                var userId = _authenticationContext.UserId.GetValueOrDefault(INVALID_ID);
                _logger.Information("Route: {route} Method: {method} QueryParameters: {queryParameters} UserID: {userID} RequestStartUTC: {requestStart}", sanitizedRoute, method, sanitizedQueryParameters, userId, requestStart);
                try
                {
                    await next(context);
                }
                finally
                {
                    var requestEnd = DateTime.UtcNow;
                    _logger.Information("Route: {route} Method: {method} QueryParameters: {queryParameters} UserID: {userID} RequestEndUTC: {requestEnd}", sanitizedRoute, method, sanitizedQueryParameters, userId, requestEnd);
                }
            }
            else
            {
                await next(context);
            }
        }

        private static string GetQueryParameters(IQueryCollection query)
        {
            if (query.Any())
            {
                return query.Select(i => i.Key).Aggregate((i, j) => i + ", " + j);
            }
            return string.Empty;
        }

        private static string SanitizeForLogging(string input)
        {
            if (input == null)
            {
                return string.Empty;
            }

            // Remove newline characters to prevent log forging via user-controlled input.
            return input.Replace("\r", string.Empty)
                        .Replace("\n", string.Empty);
        }
    }
}
