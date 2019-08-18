using System.Linq;
using System.Security.Claims;

namespace Presentation.Web.Extensions
{
    public class IdentityClaimExtension
    {
        public Claim GetClaimOrNull(ClaimsIdentity claimHolder, string claimName)
        {
            return claimHolder.FindAll(x => x.Type == claimName).FirstOrDefault();
        }
    }
}