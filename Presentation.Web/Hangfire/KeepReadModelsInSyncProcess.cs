using System;
using System.Threading;
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
                backgroundJobLauncher.LaunchScheduleDataProcessingAgreementReadUpdates(combinedTokenSource.Token).Wait(CancellationToken.None);
                backgroundJobLauncher.LaunchUpdateDataProcessingAgreementReadModels(combinedTokenSource.Token).Wait(CancellationToken.None);
            }

            CoolDown(combinedTokenSource);
        }

        private static void CoolDown(CancellationTokenSource combinedTokenSource)
        {
            var secondsPassed = 0;
            do
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                secondsPassed++;
            } while (secondsPassed < 5 && combinedTokenSource.IsCancellationRequested == false);
        }
    }
}
