using System.Threading.Tasks;
using Core.ApplicationServices.Authentication;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace Presentation.Web.Infrastructure.Middleware
{
    public class DenyUsersWithoutApiAccessMiddleware : IMiddleware
    {
        private readonly ILogger _logger;
        private readonly IAuthenticationContext _authenticationContext;

        public DenyUsersWithoutApiAccessMiddleware(ILogger logger, IAuthenticationContext authenticationContext)
        {
            _logger = logger;
            _authenticationContext = authenticationContext;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (_authenticationContext.Method == AuthenticationMethod.KitosToken && !_authenticationContext.HasApiAccess)
            {
                _logger.Warning("User with id: {userID} made an API call without having API access",
                    _authenticationContext.UserId);
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Du har ikke tilladelse til at kalde API endpoints");
            }
            else
            {
                await next(context);
            }
        }
    }
}
