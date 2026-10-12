using System;
using System.Threading;
using Hangfire.Server;
using Infrastructure.Services.BackgroundJobs;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Presentation.Web.Hangfire
{
    public class KeepReadModelsInSyncProcess : IBackgroundProcess
    {
        private readonly IServiceProvider _serviceProvider;

        public KeepReadModelsInSyncProcess(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Execute(BackgroundProcessContext context)
        {
            using var combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(context.ShutdownToken, context.StoppingToken);
            using var scope = _serviceProvider.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger>();
            try
            {
                var backgroundJobLauncher = scope.ServiceProvider.GetRequiredService<IBackgroundJobLauncher>();
                PurgeDuplicateUpdates(backgroundJobLauncher, combinedTokenSource);
                ScheduleUpdatesCausedByDependencyChanges(backgroundJobLauncher, combinedTokenSource);
                ProcessPendingUpdates(backgroundJobLauncher, combinedTokenSource);
            }
            catch (Exception e)
            {
                logger.Error(e, "Error in KeepReadModelsInSyncProcess");
            }

            CoolDown();
        }

        private static void ProcessPendingUpdates(IBackgroundJobLauncher backgroundJobLauncher, CancellationTokenSource combinedTokenSource)
        {
            backgroundJobLauncher.LaunchUpdateDataProcessingRegistrationReadModels(combinedTokenSource.Token).Wait(CancellationToken.None);
            backgroundJobLauncher.LaunchUpdateItSystemUsageOverviewReadModels(combinedTokenSource.Token).Wait(CancellationToken.None);
            backgroundJobLauncher.LaunchUpdateItContractOverviewReadModels(combinedTokenSource.Token).Wait(CancellationToken.None);
            backgroundJobLauncher.LaunchUpdateItContractSupplierOverviewReadModels(combinedTokenSource.Token).Wait(CancellationToken.None);
        }

        private static void PurgeDuplicateUpdates(IBackgroundJobLauncher backgroundJobLauncher, CancellationTokenSource combinedTokenSource)
        {
            //Ensures that duplicated update requests are filtered out before dependency and read model processing
            backgroundJobLauncher.LaunchPurgeDuplicatedReadModelUpdates(combinedTokenSource.Token).Wait(CancellationToken.None);
        }

        private static void ScheduleUpdatesCausedByDependencyChanges(IBackgroundJobLauncher backgroundJobLauncher, CancellationTokenSource combinedTokenSource)
        {
            backgroundJobLauncher.LaunchScheduleDataProcessingRegistrationReadModelUpdates(combinedTokenSource.Token).Wait(CancellationToken.None);
            backgroundJobLauncher.LaunchScheduleItSystemUsageOverviewReadModelUpdates(combinedTokenSource.Token).Wait(CancellationToken.None);
            backgroundJobLauncher.LaunchScheduleItContractOverviewReadModelUpdates(combinedTokenSource.Token).Wait(CancellationToken.None);
            backgroundJobLauncher.LaunchScheduleItContractSupplierOverviewReadModelUpdates(combinedTokenSource.Token).Wait(CancellationToken.None);
        }

        private static void CoolDown()
        {
            Thread.Sleep(TimeSpan.FromSeconds(1));
        }
    }
}
