using System.Net;

namespace Tests.Integration.Presentation.Web.Tools.Model
{
    public class CSRFTokenDTO
    {
        public required string FormToken { get; set; }
        public required Cookie CookieToken { get; set; }
    }
}
