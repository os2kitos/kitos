using System;
using Microsoft.Owin;
using Owin;
using Hangfire;
using System.IdentityModel.Tokens;
using System.Net;
using System.Threading.Tasks;
using Core.BackgroundJobs.Model;
using Core.BackgroundJobs.Services;
using Hangfire.Common;
using Ninject;
using Presentation.Web.Infrastructure.Middleware;
using Presentation.Web.Infrastructure.Model.Authentication;

[assembly: OwinStartup(typeof(Presentation.Web.Startup))]
namespace Presentation.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            InitializeHangfire(app);

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
        }

        private static void InitializeHangfire(IAppBuilder app)
        {
            // Initializing the Hangfire scheduler
            GlobalConfiguration.Configuration.UseSqlServerStorage("kitos_HangfireDB");

            //TODO: move to app setting if hangfire dashboard is enabled *- default OFF
            app.UseHangfireDashboard();
            app.UseHangfireServer();

            //Allow all versions of ssl for outgoing connections
            //NOTE: Once we solve https://os2web.atlassian.net/browse/KITOSUDV-679 we can move the certificate validation to the client handler inside EndpointValidationService.cs in stead of overriding the global configuration
            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Ssl3 |
                SecurityProtocolType.Tls |
                SecurityProtocolType.Tls11 |
                SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;

            new RecurringJobManager().AddOrUpdate(
                recurringJobId: StandardJobIds.CheckExternalLinks,
                job: Job.FromExpression((IBackgroundJobLauncher launcher) => launcher.LaunchLinkCheckAsync()),
                cronExpression: Cron.Weekly(DayOfWeek.Sunday, 0),
                timeZone: TimeZoneInfo.Local);
        }
    }
}
