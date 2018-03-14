using Microsoft.Owin;
using Owin;
using Hangfire;
using IdentityServer3.AccessTokenValidation;
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

            string key = "401b09eab3c013d4ca54922bb802bec8fd5318192b0a75f201d8b3727429090fb337591abd3e44453b954555b7a0812e1081c39b740293f765eae731f5a65ed1";

            var test = ssoConfig.SigningKey;
            // Create Security key  using private key above:
            /* // not that latest version of JWT using Microsoft namespace instead of System */
            var securityKey = new System.IdentityModel.Tokens.InMemorySymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            // Initializing the Hangfire scheduler
            GlobalConfiguration.Configuration.UseSqlServerStorage("kitos_HangfireDB");
            app.UseHangfireDashboard();
            app.UseHangfireServer();
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
