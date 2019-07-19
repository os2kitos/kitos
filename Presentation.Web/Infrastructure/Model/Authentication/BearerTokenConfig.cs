using System.IdentityModel.Tokens;
using System.Text;
using Presentation.Web.Properties;

namespace Presentation.Web.Infrastructure.Model.Authentication
{
    public class BearerTokenConfig
    {
        public const string DefaultOrganizationClaimName = "DefaultOrganization";
        public static string Issuer => Settings.Default.BaseUrl;

        public static InMemorySymmetricSecurityKey SecurityKey =>
            new InMemorySymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    System.Web.Configuration.WebConfigurationManager.AppSettings["SecurityKeyString"]
                )
            );
    }
}