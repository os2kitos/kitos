using System.Threading.Tasks;
using Core.ApplicationServices.Authentication;
using Microsoft.AspNetCore.Http;
using Presentation.Web.Extensions;
using Presentation.Web.Helpers;
using Serilog;

namespace Presentation.Web.Infrastructure.Middleware
{
    public class DenyModificationsThroughApiMiddleware : IMiddleware
    {
        private readonly ILogger _logger;
        private readonly IAuthenticationContext _authenticationContext;

        public DenyModificationsThroughApiMiddleware(ILogger logger, IAuthenticationContext authenticationContext)
        {
            _logger = logger;
            _authenticationContext = authenticationContext;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (IsKitosTokenAuthenticated(_authenticationContext) && IsIllegalMutationAttempt(context))
            {
                var sanitizedMethod = context.Request.Method
                    .Replace("\r", string.Empty)
                    .Replace("\n", string.Empty);
                var sanitizedPath = context.Request.Path.ToString()
                    .Replace("\r", string.Empty)
                    .Replace("\n", string.Empty);
                _logger.Warning("User with id: {userID} attempted to mutate resource: {url} by method {method}",
                    _authenticationContext.UserId, sanitizedPath, sanitizedMethod);
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Write operations are not allowed on this API");
            }
            else
            {
                await next(context);
            }
        }

        private static bool IsKitosTokenAuthenticated(IAuthenticationContext authenticationContext)
        {
            return authenticationContext.Method == AuthenticationMethod.KitosToken;
        }

        private static bool IsIllegalMutationAttempt(HttpContext context)
        {
            return context.Request.Method.IsMutation() && IsNotExternalApiUsage(context);
        }

        private static bool IsNotExternalApiUsage(HttpContext context)
        {
            return !context.Request.Path.ToString().IsExternalApiPath();
        }
    }
}
