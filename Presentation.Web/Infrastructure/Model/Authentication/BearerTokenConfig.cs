using System.Text;
using Microsoft.IdentityModel.Tokens;
using Presentation.Web.Properties;
using SecurityKey = Microsoft.IdentityModel.Tokens.SecurityKey;

namespace Presentation.Web.Infrastructure.Model.Authentication
{
    public class BearerTokenConfig
    {
        public static string Issuer => Settings.Default.BaseUrl;

        public static SecurityKey SecurityKey =>
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    System.Web.Configuration.WebConfigurationManager.AppSettings["SecurityKeyString"]
                )
            );
    }
}