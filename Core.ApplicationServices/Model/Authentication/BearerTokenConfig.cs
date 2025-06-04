using System.Text;
using Microsoft.IdentityModel.Tokens;
using SecurityKey = Microsoft.IdentityModel.Tokens.SecurityKey;

namespace Core.ApplicationServices.Model.Authentication
{
    public class BearerTokenConfig
    {
        public static SecurityKey SecurityKey =>
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    global::System.Web.Configuration.WebConfigurationManager.AppSettings["SecurityKeyString"]
                )
            );
    }
}