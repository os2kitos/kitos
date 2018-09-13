using Microsoft.Owin;
using Owin;
using Hangfire;
using IdentityServer3;
using System.IdentityModel.Tokens;
using Presentation.Web.Infrastructure;
using System.Text;

[assembly: OwinStartup(typeof(Presentation.Web.Startup))]

namespace Presentation.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888

            var ssoConfig = new TokenValidator().GetKeyFromConfig();
            string key = System.Web.Configuration.WebConfigurationManager.AppSettings["SecurityKeyString"];

            // Create Security key  using private key above:
            var securityKey = new System.IdentityModel.Tokens.InMemorySymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            // Initializing the Hangfire scheduler
            GlobalConfiguration.Configuration.UseSqlServerStorage("kitos_HangfireDB");
            app.UseHangfireDashboard();
            app.UseHangfireServer();
            //setup token authentication
            app.UseJwtBearerAuthentication(new Microsoft.Owin.Security.Jwt.JwtBearerAuthenticationOptions
            {
                AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode.Active,
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidAudience = ssoConfig.Audience,
                    ValidateAudience = false,

                    ValidIssuer = ssoConfig.Issuer,
                    ValidateIssuer = true,

                    IssuerSigningKey = securityKey,
                    ValidateIssuerSigningKey = true,

                    ValidateLifetime = true,
                }
            });
        }
    }
}
