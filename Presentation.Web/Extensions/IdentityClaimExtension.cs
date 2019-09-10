using System.Linq;
using System.Security.Claims;

namespace Presentation.Web.Extensions
{
    public static class IdentityClaimExtension
    {
        public static Claim GetClaimOrNull(this ClaimsIdentity claimHolder, string claimName)
        {
            return claimHolder.FindAll(x => x.Type == claimName).FirstOrDefault();
        }
    }
}