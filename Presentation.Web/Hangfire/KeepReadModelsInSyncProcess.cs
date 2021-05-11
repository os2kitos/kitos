using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire.Server;
using Infrastructure.Services.BackgroundJobs;
using Microsoft.Extensions.DependencyInjection;
using Ninject;
using Presentation.Web.Ninject;

namespace Presentation.Web.Hangfire
{
    public class KeepReadModelsInSyncProcess : IBackgroundProcess
    {
        private readonly StandardKernel _kernel;

        public KeepReadModelsInSyncProcess()
        {
            _kernel = new KernelBuilder().ForHangFire().Build();
        }

        public void Execute(BackgroundProcessContext context)
        {
            using var combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(context.ShutdownToken, context.StoppingToken);
            using (new HangfireNinjectResolutionScope(_kernel))
            {
                var backgroundJobLauncher = _kernel.GetRequiredService<IBackgroundJobLauncher>();
                PurgeDuplicateUpdates(backgroundJobLauncher, combinedTokenSource);
                ScheduleUpdatesCausedByDependencyChanges(backgroundJobLauncher, combinedTokenSource);
                ProcessPendingUpdates(backgroundJobLauncher, combinedTokenSource);
            }

            CoolDown();
        }

        private static void ProcessPendingUpdates(IBackgroundJobLauncher backgroundJobLauncher, CancellationTokenSource combinedTokenSource)
        {
            backgroundJobLauncher.LaunchUpdateDataProcessingRegistrationReadModels(combinedTokenSource.Token).Wait(CancellationToken.None);
            backgroundJobLauncher.LaunchUpdateItSystemUsageOverviewReadModels(combinedTokenSource.Token).Wait(CancellationToken.None);
        }

        private static void PurgeDuplicateUpdates(IBackgroundJobLauncher backgroundJobLauncher, CancellationTokenSource combinedTokenSource)
        {
            //Ensures that duplicated update requests are filtered out before dependency and read model processing
            backgroundJobLauncher.LaunchPurgeDuplicatedReadModelUpdates(combinedTokenSource.Token).Wait(CancellationToken.None);
        }

        private static void ScheduleUpdatesCausedByDependencyChanges(IBackgroundJobLauncher backgroundJobLauncher,
            CancellationTokenSource combinedTokenSource)
        {
            backgroundJobLauncher.LaunchScheduleDataProcessingRegistrationReadModelUpdates(combinedTokenSource.Token).Wait(CancellationToken.None);
            backgroundJobLauncher.LaunchScheduleItSystemUsageOverviewReadModelUpdates(combinedTokenSource.Token).Wait(CancellationToken.None);
        }

        private static void CoolDown()
        {
            Thread.Sleep(TimeSpan.FromSeconds(1));
        }
    }
}
