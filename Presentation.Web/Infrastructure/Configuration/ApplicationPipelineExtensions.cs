using Core.BackgroundJobs.Model;
using Hangfire;
using Hangfire.Common;
using Infrastructure.DataAccess;
using Infrastructure.Services.BackgroundJobs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Presentation.Web.Infrastructure.Middleware;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Presentation.Web.Infrastructure.Configuration
{
    public static class ApplicationPipelineExtensions
    {
        public static WebApplication UseKitosPipeline(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
                app.UseDeveloperExceptionPage();

            // Suppress OperationCanceledException from client disconnects.
            // These occur when a client aborts mid-response (e.g. during large OData serialization)
            // and are not server errors — logging them as 500 is misleading noise.
            app.Use(async (context, next) =>
            {
                try
                {
                    await next(context);
                }
                catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
                {
                    // Client disconnected — not a server fault, no action needed.
                    if (!context.Response.HasStarted)
                        context.Response.StatusCode = 499; // Client Closed Request
                }
            });

            // Enable buffering early so the body stream can be rewound and re-read by WriteModelMapperBase
            // (CurrentRequestStream.GetInputStreamCopy sets Position=0; only works if stream is buffered)
            app.Use(async (context, next) =>
            {
                context.Request.EnableBuffering();
                await next();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v2/swagger.json", "KITOS API V2");
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "KITOS API V1");
            });

            app.UseSerilogRequestLogging();

            // Forward X-Forwarded-For and X-Forwarded-Proto headers from reverse proxies (IIS Express,
            // IIS, Nginx, etc.) so that Request.IsHttps correctly reflects the original connection.
            // This must run before UseHttpsRedirection and the SAML handlers.
            if (!app.Environment.IsDevelopment())
            {
                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });
                app.UseHttpsRedirection();
            }

            app.UseStaticFiles();
            app.UseMiddleware<NormalizeODataQueryStringMiddleware>();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            // Custom middleware (after auth so IAuthenticationContext is resolved with populated HttpContext.User)
            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseMiddleware<ApiRequestsLoggingMiddleware>();
            app.UseMiddleware<DenyUsersWithoutApiAccessMiddleware>();
            app.UseMiddleware<DenyModificationsThroughApiMiddleware>();
            app.UseMiddleware<DenyTooLargeQueriesMiddleware>();

            return app;
        }

        public static WebApplication MapKitosEndpoints(this WebApplication app)
        {
            app.UseHangfireDashboard();
            app.MapControllers();
            // Conventional route for MVC controllers (e.g. HomeController → "/" redirects to "/ui",
            // OldHomeController renders the legacy AngularJS shell at "/old")
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            MapSSOEndpoints(app);

            return app;
        }

        public static WebApplication InitializeHangfireJobs(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

            recurringJobManager.AddOrUpdate(
                recurringJobId: StandardJobIds.CheckExternalLinks,
                job: Job.FromExpression((IBackgroundJobLauncher launcher) => launcher.LaunchLinkCheckAsync(CancellationToken.None)),
                cronExpression: Cron.Weekly(DayOfWeek.Sunday, 0),
                options: new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });

            recurringJobManager.AddOrUpdate(
                recurringJobId: StandardJobIds.ScheduleUpdatesForItSystemUsageReadModelsWhichChangesActiveState,
                job: Job.FromExpression((IBackgroundJobLauncher launcher) => launcher.LaunchUpdateStaleSystemUsageRmAsync(CancellationToken.None)),
                cronExpression: Cron.Daily(2),
                options: new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });

            recurringJobManager.AddOrUpdate(
                recurringJobId: StandardJobIds.ScheduleUpdatesForItContractOverviewReadModelsWhichChangesActiveState,
                job: Job.FromExpression((IBackgroundJobLauncher launcher) => launcher.LaunchUpdateStaleContractRmAsync(CancellationToken.None)),
                cronExpression: Cron.Daily(2),
                options: new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });

            recurringJobManager.AddOrUpdate(
                recurringJobId: StandardJobIds.ScheduleUpdatesForDataProcessingReadModelsWhichChangesActiveState,
                job: Job.FromExpression((IBackgroundJobLauncher launcher) => launcher.LaunchUpdateStaleDataProcessingRegistrationReadModels(CancellationToken.None)),
                cronExpression: Cron.Daily(2),
                options: new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });

            recurringJobManager.AddOrUpdate(
                recurringJobId: StandardJobIds.ScheduleFkOrgUpdates,
                job: Job.FromExpression((IBackgroundJobLauncher launcher) => launcher.LaunchUpdateFkOrgSync(CancellationToken.None)),
                cronExpression: Cron.Weekly(DayOfWeek.Monday, 3),
                options: new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });

            recurringJobManager.AddOrUpdate(
                recurringJobId: StandardJobIds.RebuildDataProcessingReadModels,
                job: Job.FromExpression((IBackgroundJobLauncher launcher) => launcher.LaunchFullReadModelRebuild(ReadModelRebuildScope.DataProcessingRegistration, CancellationToken.None)),
                cronExpression: Cron.Never(),
                options: new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });

            recurringJobManager.AddOrUpdate(
                recurringJobId: StandardJobIds.RebuildItSystemUsageReadModels,
                job: Job.FromExpression((IBackgroundJobLauncher launcher) => launcher.LaunchFullReadModelRebuild(ReadModelRebuildScope.ItSystemUsage, CancellationToken.None)),
                cronExpression: Cron.Never(),
                options: new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });

            recurringJobManager.AddOrUpdate(
                recurringJobId: StandardJobIds.RebuildItContractReadModels,
                job: Job.FromExpression((IBackgroundJobLauncher launcher) => launcher.LaunchFullReadModelRebuild(ReadModelRebuildScope.ItContract, CancellationToken.None)),
                cronExpression: Cron.Never(),
                options: new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });

            recurringJobManager.AddOrUpdate(
                recurringJobId: StandardJobIds.RebuildItContractSupplierReadModels,
                job: Job.FromExpression((IBackgroundJobLauncher launcher) => launcher.LaunchFullReadModelRebuild(ReadModelRebuildScope.ItContractSupplier, CancellationToken.None)),
                cronExpression: Cron.Never(),
                options: new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });

            recurringJobManager.AddOrUpdate(
                recurringJobId: StandardJobIds.PurgeOrphanedHangfireJobs,
                job: Job.FromExpression((IBackgroundJobLauncher launcher) => launcher.LaunchPurgeOrphanedHangfireJobs(CancellationToken.None)),
                cronExpression: Cron.Never(),
                options: new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });

            recurringJobManager.AddOrUpdate(
                recurringJobId: StandardJobIds.CreateInitialPublicMessages,
                job: Job.FromExpression((IBackgroundJobLauncher launcher) => launcher.LaunchCreatePublicMessagesTask(CancellationToken.None)),
                cronExpression: Cron.Never(),
                options: new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });

            recurringJobManager.AddOrUpdate(
                recurringJobId: StandardJobIds.CreateMainPublicMessage,
                job: Job.FromExpression((IBackgroundJobLauncher launcher) => launcher.LaunchCreateMainPublicMessageTask(CancellationToken.None)),
                cronExpression: Cron.Never(),
                options: new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });

            return app;
        }

        public static WebApplication EnsurePostgreSqlExtensions(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<KitosContext>();
            if (dbContext.Database.IsNpgsql())
            {
                dbContext.Database.OpenConnection();
                try
                {
                    dbContext.Database.ExecuteSqlRaw("CREATE SCHEMA IF NOT EXISTS public; CREATE EXTENSION IF NOT EXISTS citext SCHEMA public;");
                    if (dbContext.Database.GetDbConnection() is NpgsqlConnection npgsqlConnection)
                    {
                        npgsqlConnection.ReloadTypes();
                        // Drop all pooled connectors so every subsequent request opens connections
                        // with fresh type metadata after the extension bootstrap.
                        NpgsqlConnection.ClearAllPools();
                    }
                }
                finally
                {
                    dbContext.Database.CloseConnection();
                }
            }

            return app;
        }

        private static void MapSSOEndpoints(WebApplication app)
        {
            // Legacy .ashx handler routes: LoginHandler.ashx initiates external (SAML SSO) login
            // by forwarding to the SAML sign-on endpoint.
            app.MapGet("/LoginHandler.ashx", ctx =>
            {
                ctx.Response.Redirect("/Login.ashx");
                return Task.CompletedTask;
            }).AllowAnonymous();

            // SAML .ashx handlers — migrated to net10.0 in the local OIOSAML.Net project.
            // Login.ashx handles both the SP-initiated AuthnRequest (GET) and the IdP response (POST).
            app.MapMethods("/Login.ashx", ["GET", "POST"], ctx =>
            {
                try
                {
                    Log.Information("SAML /Login.ashx invoked. Method: {Method}, QueryString: {QueryString}, ContentType: {ContentType}",
                        ctx.Request.Method, ctx.Request.QueryString.Value, ctx.Request.ContentType);

                    new dk.nita.saml20.protocol.Saml20SignonHandler().ProcessRequest(ctx);

                    Log.Information("SAML /Login.ashx ProcessRequest completed. StatusCode: {StatusCode}", ctx.Response.StatusCode);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Unhandled exception in SAML endpoint {Endpoint}. Method: {Method}, QueryString: {QueryString}",
                        "/Login.ashx", ctx.Request.Method, ctx.Request.QueryString.Value);
                    ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
                }

                return Task.CompletedTask;
            }).AllowAnonymous();

            app.MapMethods("/Logout.ashx", ["GET", "POST"], ctx =>
            {
                try
                {
                    Log.Information("SAML /Logout.ashx invoked. Method: {Method}, QueryString: {QueryString}, ContentType: {ContentType}",
                        ctx.Request.Method, ctx.Request.QueryString.Value, ctx.Request.ContentType);

                    new dk.nita.saml20.protocol.Saml20LogoutHandler().ProcessRequest(ctx);

                    Log.Information("SAML /Logout.ashx ProcessRequest completed. StatusCode: {StatusCode}", ctx.Response.StatusCode);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Unhandled exception in SAML endpoint {Endpoint}. Method: {Method}, QueryString: {QueryString}",
                        "/Logout.ashx", ctx.Request.Method, ctx.Request.QueryString.Value);
                    ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
                }

                return Task.CompletedTask;
            }).AllowAnonymous();

            app.MapGet("/Metadata.ashx", ctx =>
            {
                try
                {
                    Log.Information("SAML /Metadata.ashx invoked. QueryString: {QueryString}",
                        ctx.Request.QueryString.Value);

                    new dk.nita.saml20.protocol.Saml20MetadataHandler().ProcessRequest(ctx);

                    Log.Information("SAML /Metadata.ashx ProcessRequest completed. StatusCode: {StatusCode}", ctx.Response.StatusCode);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Unhandled exception in SAML endpoint {Endpoint}. QueryString: {QueryString}",
                        "/Metadata.ashx", ctx.Request.QueryString.Value);
                    ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
                }

                return Task.CompletedTask;
            }).AllowAnonymous();
        }
    }
}


