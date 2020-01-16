using Microsoft.Owin;
using Owin;
using Hangfire;
using System.IdentityModel.Tokens;
using Presentation.Web.Infrastructure.Middleware;
using Presentation.Web.Infrastructure.Model.Authentication;

[assembly: OwinStartup(typeof(Presentation.Web.Startup))]
namespace Presentation.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
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

            app.UseNinject(); 
            app.Use<ApiRequestsLoggingMiddleware>();
            app.Use<DenyUsersWithoutApiAccessMiddleware>();
            app.Use<DenyModificationsThroughApiMiddleware>();
            //app.Use<CSRFProtectionMiddleware>();
        }
    }
}
