using System.IdentityModel.Tokens;
using System.Text;
using Presentation.Web.Properties;

namespace Presentation.Web.Infrastructure.Model
{
    public class BearerTokenConfig
    {
        public static string Issuer => Settings.Default.BaseUrl;

        public static InMemorySymmetricSecurityKey SecurityKey =>
            new InMemorySymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    System.Web.Configuration.WebConfigurationManager.AppSettings["SecurityKeyString"]
                )
            );
    }
}