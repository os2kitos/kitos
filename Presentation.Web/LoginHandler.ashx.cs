using System.Web;

namespace Presentation.Web
{
    /// <summary>
    /// LoginHandler redirects to the Login.ashx page with the forceAuthn parameter set to true.
    /// </summary>
    public class LoginHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.Redirect("Login.ashx?forceAuthn=true");
        }

        public bool IsReusable => false;
    }
}