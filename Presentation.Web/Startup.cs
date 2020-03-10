using System;
using Microsoft.Owin;
using Owin;
using Hangfire;
using System.IdentityModel.Tokens;
using Core.BackgroundJobs.Model;
using Core.BackgroundJobs.Services;
using Hangfire.Common;
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

            //TODO: Move configurations to repository
            new RecurringJobManager().AddOrUpdate(
                recurringJobId: StandardJobIds.CheckExternalLinks,
                job: Job.FromExpression((IBackgroundJobLauncher launcher) => launcher.LaunchLinkCheckAsync()),
                //TODO: Use this cronExpression: Cron.Weekly(DayOfWeek.Sunday, 0),
                cronExpression: Cron.Minutely(),
                timeZone: TimeZoneInfo.Local);
        }
    }
}
