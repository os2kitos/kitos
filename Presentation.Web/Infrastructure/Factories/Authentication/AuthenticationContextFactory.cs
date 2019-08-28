using System.Security.Claims;
using System.Security.Principal;
using Core.DomainModel;
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

        public AuthenticationContextFactory(ILogger logger, IOwinContext owinContext, IUserRepository userRepository)
        {
            _logger = logger;
            _owinContext = owinContext;
            _userRepository = userRepository;
        }

        public IAuthenticationContext Create()
        {
            var principal = _owinContext.Authentication.User;
            var user = GetAuthenticatedUser(principal);
            return user != null
                ? new AuthenticationContext(MapAuthenticationMethod(principal), MapApiAccess(user), user.Id, MapOrganizationId(user, principal))
                : new AuthenticationContext(AuthenticationMethod.Anonymous, false);
        }

        private bool MapApiAccess(User user)
        {
            return user.HasApiAccess == true;
        }

        private int? MapOrganizationId(User user, IPrincipal principal)
        {
            var method = MapAuthenticationMethod(principal);
            if (method == AuthenticationMethod.KitosToken)
            {
                var orgId = (principal.Identity as ClaimsIdentity).GetClaimOrNull(BearerTokenConfig.DefaultOrganizationClaimName);

                if (orgId != null)
                {
                    if (int.TryParse(orgId.Value, out var id))
                    {
                        return id;
                    }
                    _logger.Error("Found Claim {claimName}, but could not parse it to an integer", BearerTokenConfig.DefaultOrganizationClaimName);
                }
            }
            else if (method == AuthenticationMethod.Forms)
            {
                return user.DefaultOrganizationId;
            }
            return default(int?);
        }

        private AuthenticationMethod MapAuthenticationMethod(IPrincipal user)
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

        private User GetAuthenticatedUser(IPrincipal user)
        {
            if (user.Identity.IsAuthenticated)
            {
                var id = GetUserId(user);
                if (id.HasValue)
                {
                    return _userRepository.GetById(id.Value);
                }
            }

            return null;
        }

        private int? ParseInteger(string toParse)
        {
            if (int.TryParse(toParse, out var asInt))
            {
                return asInt;
            }
            _logger.Error("Could not parse to int: {toParse}", toParse);
            return null;
        }

        private int? GetUserId(IPrincipal user)
        {
            var userId = user.Identity.Name;
            var id = ParseInteger(userId);
            return id;
        }
    }
}