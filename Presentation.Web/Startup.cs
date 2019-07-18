using Microsoft.Owin;
using Owin;
using Hangfire;
using System.IdentityModel.Tokens;
using Presentation.Web.Infrastructure.Model;
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
                    ValidateAudience = false,
                    ValidIssuer = BearerTokenConfig.Issuer,
                    ValidateIssuer = true,

                    IssuerSigningKey = BearerTokenConfig.SecurityKey,
                    ValidateIssuerSigningKey = true,

                    ValidateLifetime = true,
                }
            });

            // Initializing API Request Logging

            app.UseCustomScopeForRequest();
            app.Use<ApiRequestsLoggingMiddleware>();
        }
    }
}
