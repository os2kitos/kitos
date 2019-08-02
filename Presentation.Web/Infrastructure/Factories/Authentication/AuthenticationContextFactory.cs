using System.Security.Claims;
using Core.DomainServices;
using Microsoft.Owin;
using Presentation.Web.Extensions;
using Presentation.Web.Infrastructure.Model.Authentication;
using Serilog;

namespace Presentation.Web.Infrastructure.Factories.Authentication
{
    public class AuthenticationContextFactory : IAuthenticationContextFactory
    {
        private readonly ILogger _logger;
        private readonly IOwinContext _owinContext;
        private readonly IUserRepository _userRepository;
        private readonly IdentityClaimExtension identityClaimExtension = new IdentityClaimExtension();

        public AuthenticationContextFactory(ILogger logger, IOwinContext owinContext, IUserRepository userRepository)
        {
            _logger = logger;
            _owinContext = owinContext;
            _userRepository = userRepository;
        }

        public IAuthenticationContext Create()
        {
            var user = _owinContext.Authentication.User;
            return IsAuthenticated(user)
                ? new AuthenticationContext(MapAuthenticationMethod(user), MapApiAccess(user), MapUserId(user), MapOrganizationId(user))
                : new AuthenticationContext(AuthenticationMethod.Anonymous, false);
        }

        private bool MapApiAccess(ClaimsPrincipal user)
        {
            var dbUser = _userRepository.GetById(int.Parse(user.Identity.Name));
            return dbUser.HasApiAccess;
        }

        private int? MapOrganizationId(ClaimsPrincipal user)
        {
            var method = MapAuthenticationMethod(user);
            if (method == AuthenticationMethod.KitosToken)
            {
                var orgId = identityClaimExtension.GetClaimOrNull((user.Identity as ClaimsIdentity), BearerTokenConfig.DefaultOrganizationClaimName);

                if (orgId != null)
                {
                    if (int.TryParse(orgId.Value, out var id))
                    {
                        return id;
                    }
                    _logger.Error("Found Claim {claimName}, but could not parse it to an integer", BearerTokenConfig.DefaultOrganizationClaimName);
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