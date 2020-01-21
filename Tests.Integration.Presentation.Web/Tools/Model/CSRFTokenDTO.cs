using System.Net;

namespace Tests.Integration.Presentation.Web.Tools.Model
{
    public class CSRFTokenDTO
    {
        public string FormToken { get; set; }
        public Cookie CookieToken { get; set; }
    }
}
