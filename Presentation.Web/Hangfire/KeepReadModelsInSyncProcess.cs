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
            _kernel = new KernelBuilder().ForThread().Build();
        }

        public void Execute(BackgroundProcessContext context)
        {
            var thread = new Thread(() =>
            {
                //Using a NEW thread to isolate ninject resolutions for the thread scope. This is the best option we have for continous batch jobs which should use ninject and clean up after each execution
                var backgroundJobLauncher = _kernel.GetRequiredService<IBackgroundJobLauncher>();
                using var combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(context.ShutdownToken,context.StoppingToken);
                backgroundJobLauncher.LaunchScheduleDataProcessingAgreementReadUpdates(combinedTokenSource.Token).Wait(CancellationToken.None);
                backgroundJobLauncher.LaunchUpdateDataProcessingAgreementReadModels(combinedTokenSource.Token).Wait(CancellationToken.None);
            })
            {
                IsBackground = true,
            };
            thread.Start();

            //Intentional context switch
            Thread.Sleep(TimeSpan.FromSeconds(1));

            thread.Join();
        }
    }
}
