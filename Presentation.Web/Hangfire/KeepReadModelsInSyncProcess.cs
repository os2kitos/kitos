using System;
using System.Threading;
using Hangfire.Server;
using Infrastructure.Services.BackgroundJobs;
using Microsoft.Extensions.DependencyInjection;
using Ninject;
using Presentation.Web.Ninject;

namespace Presentation.Web.Hangfire
{
    public class KeepReadModelsInSyncProcess : IBackgroundProcess, IDisposable
    {
        private readonly StandardKernel _kernel;

        public KeepReadModelsInSyncProcess()
        {
            _kernel = new KernelBuilder().ForHangFire().Build();
        }

        public void Execute(BackgroundProcessContext context)
        {
            using (new HangfireNinjectResolutionScope(_kernel))
            {
                var backgroundJobLauncher = _kernel.GetRequiredService<IBackgroundJobLauncher>();
                using var combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(context.ShutdownToken, context.StoppingToken);
                backgroundJobLauncher.LaunchScheduleDataProcessingAgreementReadUpdates(combinedTokenSource.Token).Wait(CancellationToken.None);
                backgroundJobLauncher.LaunchUpdateDataProcessingAgreementReadModels(combinedTokenSource.Token).Wait(CancellationToken.None);
            }
        }

        public void Dispose()
        {
            _kernel?.Dispose();
        }
    }
}
