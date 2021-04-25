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
                ProcessDataProcessingModule(backgroundJobLauncher, combinedTokenSource);
                ProcessItSystemUsageModule(backgroundJobLauncher, combinedTokenSource);
            }

            CoolDown();
        }

        private static void ProcessItSystemUsageModule(IBackgroundJobLauncher backgroundJobLauncher, CancellationTokenSource combinedTokenSource)
        {

            PurgeDuplicateUpdates(backgroundJobLauncher, combinedTokenSource).Wait(CancellationToken.None); //Make sure we dont have duplicates in queue before processing the dependency change effects
            backgroundJobLauncher.LaunchScheduleItSystemUsageOverviewReadModelUpdates(combinedTokenSource.Token).Wait(CancellationToken.None);
            
            PurgeDuplicateUpdates(backgroundJobLauncher, combinedTokenSource).Wait(CancellationToken.None); //Make sure we dont have duplicates in queue after processing the dependency change effects
            backgroundJobLauncher.LaunchUpdateItSystemUsageOverviewReadModels(combinedTokenSource.Token).Wait(CancellationToken.None);
        }

        private static Task PurgeDuplicateUpdates(IBackgroundJobLauncher backgroundJobLauncher, CancellationTokenSource combinedTokenSource)
        {
            return backgroundJobLauncher.LaunchPurgeDuplicatedReadModelUpdates(combinedTokenSource.Token);
        }

        private static void ProcessDataProcessingModule(IBackgroundJobLauncher backgroundJobLauncher,
            CancellationTokenSource combinedTokenSource)
        {
            PurgeDuplicateUpdates(backgroundJobLauncher, combinedTokenSource).Wait(CancellationToken.None); //Make sure we dont have duplicates in queue before processing the dependency change effects
            backgroundJobLauncher.LaunchScheduleDataProcessingRegistrationReadModelUpdates(combinedTokenSource.Token).Wait(CancellationToken.None);

            PurgeDuplicateUpdates(backgroundJobLauncher, combinedTokenSource).Wait(CancellationToken.None); //Make sure we dont have duplicates in queue after processing the dependency change effects
            backgroundJobLauncher.LaunchUpdateDataProcessingRegistrationReadModels(combinedTokenSource.Token).Wait(CancellationToken.None);
        }

        private static void CoolDown()
        {
            Thread.Sleep(TimeSpan.FromSeconds(1));
        }
    }
}
