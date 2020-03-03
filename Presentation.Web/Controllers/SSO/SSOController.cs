using System.Web.Http;

namespace Presentation.Web.Controllers.SSO
{
    public class SSOController: ApiController
    {
        [HttpGet]
        public IHttpActionResult SSO()
        {
            // TODO: Check incoming SAML response
            return Redirect("LoggedIn.html");
        }
    }
}