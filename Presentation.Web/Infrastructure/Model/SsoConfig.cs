using System.IdentityModel.Tokens;

namespace Presentation.Web.Infrastructure.Model
{
    public class SsoConfig
    {
        public SecurityKey SigningKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }
}