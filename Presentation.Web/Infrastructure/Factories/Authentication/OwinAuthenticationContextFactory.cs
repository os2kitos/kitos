using System.Security.Claims;
using System.Security.Principal;
using Core.Abstractions.Extensions;
using Core.ApplicationServices.Authentication;
using Core.DomainModel;
using Core.DomainServices;
using Infrastructure.Services.Http;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace Presentation.Web.Infrastructure.Factories.Authentication
{
    public class OwinAuthenticationContextFactory : IAuthenticationContextFactory
    {
        private static readonly IAuthenticationContext AnonymousAuthentication =
            new AuthenticationContext(AuthenticationMethod.Anonymous, false);

        private readonly ILogger _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserRepository _userRepository;

        public OwinAuthenticationContextFactory(
            ILogger logger,
            IHttpContextAccessor httpContextAccessor,
            IUserRepository userRepository)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _userRepository = userRepository;
        }

        public IAuthenticationContext Create()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return AnonymousAuthentication;

            var principal = httpContext.User;
            if (principal?.Identity?.IsAuthenticated != true)
                return AnonymousAuthentication;

            return GetAuthenticatedUser(principal)
                .FromNullable()
                .Select(user => new AuthenticationContext(MapAuthenticationMethod(principal), MapApiAccess(user), user.Id))
                .Match(authUser => (IAuthenticationContext)authUser, () => AnonymousAuthentication);
        }

        private static bool MapApiAccess(User user)
        {
            return user.HasApiAccess == true;
        }

        private AuthenticationMethod MapAuthenticationMethod(IPrincipal user)
        {
            var authenticationMethod = user.Identity?.AuthenticationType;
            return authenticationMethod switch
            {
                AuthenticationSchemes.Bearer => AuthenticationMethod.KitosToken,
                "JWT" => AuthenticationMethod.KitosToken,
                "Cookies" => AuthenticationMethod.Forms,
                _ => LogUnknownAndReturnAnonymous(authenticationMethod)
            };
        }

        private AuthenticationMethod LogUnknownAndReturnAnonymous(string? authMethod)
        {
            _logger.Error("Unknown authentication method {authenticationMethod}", authMethod);
            return AuthenticationMethod.Anonymous;
        }

        private User? GetAuthenticatedUser(IPrincipal user)
        {
            if (user.Identity?.IsAuthenticated == true)
            {
                var id = GetUserId(user);
                if (id.HasValue)
                    return _userRepository.GetById(id.Value);
            }
            return null;
        }

        private int? ParseUserIdInteger(string toParse)
        {
            if (int.TryParse(toParse, out var asInt))
                return asInt;
            _logger.Debug("Could not parse user id to int: {toParse}", toParse);
            return null;
        }

        private int? GetUserId(IPrincipal user)
        {
            // JsonWebTokenHandler (.NET 8+) does not apply inbound claim type mapping,
            // so the JWT "name" claim may be stored as "name" rather than ClaimTypes.Name.
            // Try both forms, then fall back to Identity.Name (which uses NameClaimType).
            string? userId = null;
            if (user is ClaimsPrincipal cp)
            {
                userId = cp.FindFirstValue("name")
                    ?? cp.FindFirstValue(ClaimTypes.Name);
            }
            userId ??= user.Identity?.Name;
            return userId == null ? null : ParseUserIdInteger(userId);
        }
    }
}
