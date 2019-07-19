using Microsoft.Owin;

namespace Presentation.Web.Infrastructure.Model
{
    public class AuthenticationHelper
    {
        public bool IsTokenAuthentication(IOwinContext context)
        {
            return context.Authentication.User.Identity.AuthenticationType == "JWT";
        }
    }
}