using System;
using System.Threading;
using Core.ApplicationServices.Model.Authentication;
using Microsoft.Owin;
using Owin;
using Hangfire;
using Core.BackgroundJobs.Model;
using Hangfire.Common;
using Infrastructure.Services.BackgroundJobs;
using Infrastructure.Services.Http;
using Microsoft.IdentityModel.Tokens;
using Presentation.Web.Hangfire;
using Presentation.Web.Infrastructure.Middleware;
using Presentation.Web.Ninject;
using Presentation.Web.Infrastructure.Filters;
using Presentation.Web.Infrastructure;
using Infrastructure.DataAccess.Tools;
using Presentation.Web.Properties;

[assembly: OwinStartup(typeof(Presentation.Web.Startup))]
namespace Presentation.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            InitializeHangfire(app);

            // Setup token authentication
            app.UseJwtBearerAuthentication(new Microsoft.Owin.Security.Jwt.JwtBearerAuthenticationOptions
            {
                AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode.Active,
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidIssuer = Settings.Default.BaseUrl,
                    ValidateIssuer = true,
                    IssuerSigningKey = BearerTokenConfig.SecurityKey,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                }
            });

            app.UseNinject();
            app.Use<CorrelationIdMiddleware>();
            app.Use<ApiRequestsLoggingMiddleware>();
            app.Use<DenyUsersWithoutApiAccessMiddleware>();
            app.Use<DenyModificationsThroughApiMiddleware>();
            app.Use<DenyTooLargeQueriesMiddleware>();
        }

        private static void InitializeHangfire(IAppBuilder app)
        {
            // Initializing the Hangfire scheduler
            var standardKernel = new KernelBuilder().ForHangFire().Build();

            GlobalConfiguration.Configuration.UseNinjectActivator(standardKernel);
            GlobalConfiguration.Configuration.UseSqlServerStorage(ConnectionStringTools.GetConnectionString("kitos_HangfireDB"));
            GlobalJobFilters.Filters.Add(new AdvisSendFailureFilter(standardKernel));
            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = KitosConstants.MaxHangfireRetries });

            app.UseHangfireDashboard();
            app.UseHangfireServer(new KeepReadModelsInSyncProcess());

            ServiceEndpointConfiguration.ConfigureValidationOfOutgoingConnections();

            var recurringJobManager = new RecurringJobManager();

            /******************
             * RECURRING JOBS *
             *****************/

            recurringJobManager.AddOrUpdate(
                recurringJobId: StandardJobIds.CheckExternalLinks,
                job: Job.FromExpression((IBackgroundJobLauncher launcher) => launcher.LaunchLinkCheckAsync(CancellationToken.None)),
                cronExpression: Cron.Weekly(DayOfWeek.Sunday, 0),
                timeZone: TimeZoneInfo.Local);

            recurringJobManager.AddOrUpdate(
                recurringJobId: StandardJobIds.ScheduleUpdatesForItSystemUsageReadModelsWhichChangesActiveState,
                job: Job.FromExpression((IBackgroundJobLauncher launcher) => launcher.LaunchUpdateStaleSystemUsageRmAsync(CancellationToken.None)),
                cronExpression: Cron.Daily(2), // Every night at 02:00 (if job runs a 00:00 - date validity check doesn't update correctly)
                timeZone: TimeZoneInfo.Local);

            recurringJobManager.AddOrUpdate(
                recurringJobId: StandardJobIds.ScheduleUpdatesForItContractOverviewReadModelsWhichChangesActiveState,
                job: Job.FromExpression((IBackgroundJobLauncher launcher) => launcher.LaunchUpdateStaleContractRmAsync(CancellationToken.None)),
                cronExpression: Cron.Daily(2), // Every night at 02:00
                timeZone: TimeZoneInfo.Local);

            recurringJobManager.AddOrUpdate(
                recurringJobId: StandardJobIds.ScheduleUpdatesForDataProcessingReadModelsWhichChangesActiveState,
                job: Job.FromExpression((IBackgroundJobLauncher launcher) => launcher.LaunchUpdateStaleDataProcessingRegistrationReadModels(CancellationToken.None)),
                cronExpression: Cron.Daily(2), // Every night at 02:00
                timeZone: TimeZoneInfo.Local);

            recurringJobManager.AddOrUpdate(
                recurringJobId: StandardJobIds.ScheduleFkOrgUpdates,
                job: Job.FromExpression((IBackgroundJobLauncher launcher) => launcher.LaunchUpdateFkOrgSync(CancellationToken.None)),
                cronExpression: Cron.Weekly(DayOfWeek.Monday,3), // Every monday at 3 AM
                timeZone: TimeZoneInfo.Local);

            /******************
             * ON-DEMAND JOBS *
             *****************/

            recurringJobManager.AddOrUpdate(
                recurringJobId: StandardJobIds.RebuildDataProcessingReadModels,
                job: Job.FromExpression((IBackgroundJobLauncher launcher) => launcher.LaunchFullReadModelRebuild(ReadModelRebuildScope.DataProcessingRegistration, CancellationToken.None)),
                cronExpression: Cron.Never(), //On demand
                timeZone: TimeZoneInfo.Local);

            recurringJobManager.AddOrUpdate(
                recurringJobId: StandardJobIds.RebuildItSystemUsageReadModels,
                job: Job.FromExpression((IBackgroundJobLauncher launcher) => launcher.LaunchFullReadModelRebuild(ReadModelRebuildScope.ItSystemUsage, CancellationToken.None)),
                cronExpression: Cron.Never(), //On demand
                timeZone: TimeZoneInfo.Local);

            recurringJobManager.AddOrUpdate(
                recurringJobId: StandardJobIds.RebuildItContractReadModels,
                job: Job.FromExpression((IBackgroundJobLauncher launcher) => launcher.LaunchFullReadModelRebuild(ReadModelRebuildScope.ItContract, CancellationToken.None)),
                cronExpression: Cron.Never(), //On demand
                timeZone: TimeZoneInfo.Local);

            recurringJobManager.AddOrUpdate(
                recurringJobId: StandardJobIds.PurgeOrphanedHangfireJobs,
                job: Job.FromExpression((IBackgroundJobLauncher launcher) => launcher.LaunchPurgeOrphanedHangfireJobs(CancellationToken.None)),
                cronExpression: Cron.Never(), //On demand
                timeZone: TimeZoneInfo.Local);

            recurringJobManager.AddOrUpdate(
                recurringJobId: StandardJobIds.CreateInitialPublicMessages,
                job: Job.FromExpression((IBackgroundJobLauncher launcher) => launcher.LaunchCreatePublicMessagesTask(CancellationToken.None)),
                cronExpression: Cron.Never(), //On demand
                timeZone: TimeZoneInfo.Local);

            recurringJobManager.AddOrUpdate(
                recurringJobId: StandardJobIds.CreateInitialPublicMessages,
                job: Job.FromExpression((IBackgroundJobLauncher launcher) => launcher.LaunchCreateMainPublicMessageTask(CancellationToken.None)),
                cronExpression: Cron.Never(), //On demand
                timeZone: TimeZoneInfo.Local);
        }
    }
}
