using System;
using System.Threading;
using Hangfire.Server;
using Hangfire.Storage;
using Infrastructure.Services.BackgroundJobs;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Presentation.Web.Hangfire
{
    public class KeepReadModelsInSyncProcess : IBackgroundProcess
    {
        private readonly IServiceProvider _serviceProvider;
        private const string SyncLockResource = "kitos:read-model-sync";
        private static readonly TimeSpan LockWaitTimeout = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(1);

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
                using var connection = context.Storage.GetConnection();
                using var syncLock = TryAcquireSyncLock(connection);
                if (syncLock != null && !combinedTokenSource.IsCancellationRequested)
                {
                    RunSyncCycle(scope.ServiceProvider, combinedTokenSource);
                }
            }
            catch (Exception e)
            {
                logger.Error(e, "Error in KeepReadModelsInSyncProcess");
            }

            combinedTokenSource.Token.WaitHandle.WaitOne(PollInterval);
        }

        private static IDisposable? TryAcquireSyncLock(IStorageConnection connection)
        {
            try
            {
                // Purging and consuming must share a lock across all application instances.
                return connection.AcquireDistributedLock(SyncLockResource, LockWaitTimeout);
            }
            catch (DistributedLockTimeoutException)
            {
                return null; // Another server is already processing the queue.
            }
        }

        private static void RunSyncCycle(IServiceProvider serviceProvider, CancellationTokenSource tokenSource)
        {
            var backgroundJobLauncher = serviceProvider.GetRequiredService<IBackgroundJobLauncher>();
            PurgeDuplicateUpdates(backgroundJobLauncher, tokenSource);
            ScheduleUpdatesCausedByDependencyChanges(backgroundJobLauncher, tokenSource);
            ProcessPendingUpdates(backgroundJobLauncher, tokenSource);
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

    }
}
