using System.Text;
using System.Web.Http;
using dk.nita.saml20.identity;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.SSO
{
    [RoutePrefix("SSO")]
    public class SSOController: ApiController
    {
        [InternalApi]
        [HttpGet]
        [Route("")]
        public IHttpActionResult SSO()
        {
            // TODO: Check incoming SAML response
            var result = "User not authenticated using SAML";
            if (Saml20Identity.IsInitialized())
            {
                var currentIdentityName = Saml20Identity.Current.Name;
                result = $"Welcome '{currentIdentityName}' ({GetCurrentSaml20IdentityClaims()})";
            }
            return Ok(result);
        }

        private static string GetCurrentSaml20IdentityClaims()
        {
            var stringBuilder = new StringBuilder();
            foreach (var currentClaim in Saml20Identity.Current.Claims)
            {
                stringBuilder.AppendLine(currentClaim.Value);
            }
            return stringBuilder.ToString();
        }
    }
}