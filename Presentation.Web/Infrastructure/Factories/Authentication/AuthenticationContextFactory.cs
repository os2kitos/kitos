using System.Linq;
using System.Security.Claims;
using Microsoft.Owin;
using Presentation.Web.Infrastructure.Model.Authentication;
using Serilog;

namespace Presentation.Web.Infrastructure.Factories.Authentication
{
    public class AuthenticationContextFactory : IAuthenticationContextFactory
    {
        private readonly ILogger _logger;
        private readonly IOwinContext _owinContext;

        public AuthenticationContextFactory(ILogger logger, IOwinContext owinContext)
        {
            _logger = logger;
            _owinContext = owinContext;
        }

        public IAuthenticationContext Create()
        {
            var user = _owinContext.Authentication.User;
            return IsAuthenticated(user)
                ? new AuthenticationContext(MapAuthenticationMethod(user), MapUserId(user), MapOrganizationId(user))
                : new AuthenticationContext(AuthenticationMethod.Anonymous);
        }

        private int? MapOrganizationId(ClaimsPrincipal user)
        {
            var method = MapAuthenticationMethod(user);
            if (method == AuthenticationMethod.KitosToken)
            {
                // Create extension method for this
                var orgID =
                    (user.Identity as ClaimsIdentity)?
                    .FindAll(x => x.Type == BearerTokenConfig.DefaultOrganizationClaimName)
                    .FirstOrDefault();

                if (orgID != null)
                {
                    if (int.TryParse(orgID.Value, out var id))
                    {
                        return id;
                    }
                    _logger.Error("Found DefaultOrganizationClaim, but could not parse it to an integer: {orgIdValue}", orgID.Value);
                }
            }
            return default(int?);
        }

        private int? MapUserId(ClaimsPrincipal user)
        {
            var userId = user.Identity.Name;
            if (int.TryParse(userId, out var id))
            {
                return id;
            }
            
            _logger.Error("Could not parse to int: {userId}", userId);
            return default(int);
            
        }

        private AuthenticationMethod MapAuthenticationMethod(ClaimsPrincipal user)
        {
            var authenticationMethod = user.Identity.AuthenticationType;
            switch (authenticationMethod)
            {
                case "JWT":
                    return AuthenticationMethod.KitosToken;
                    break;
                case "Forms":
                    return AuthenticationMethod.Forms;
                    break;
                default:
                    _logger.Error("Unknown authentication method {authenticationMethod}", authenticationMethod);
                    return AuthenticationMethod.Anonymous;
            }
        }

        private bool IsAuthenticated(ClaimsPrincipal user)
        {
            return user.Identity.IsAuthenticated;
        }
    }
}