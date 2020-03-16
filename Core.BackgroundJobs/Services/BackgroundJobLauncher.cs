using System.Threading.Tasks;
using Core.BackgroundJobs.Factory;
using Core.BackgroundJobs.Model;
using Core.DomainModel.Result;
using Infrastructure.Services.BackgroundJobs;
using Serilog;

namespace Core.BackgroundJobs.Services
{
    public class BackgroundJobLauncher : IBackgroundJobLauncher
    {
        private readonly IBackgroundJobFactory _backgroundJobFactory;
        private readonly ILogger _logger;

        public BackgroundJobLauncher(IBackgroundJobFactory backgroundJobFactory, ILogger logger)
        {
            _backgroundJobFactory = backgroundJobFactory;
            _logger = logger;
        }

        public async Task LaunchLinkCheckAsync()
        {
            await Launch(_backgroundJobFactory.CreateExternalReferenceCheck());
        }

        private async Task Launch(IAsyncBackgroundJob job)
        {
            var jobId = job.Id;

            LogJobStarted(jobId);
            var result = await job.ExecuteAsync();

            LogJobResult(jobId, result);
        }

        private void LogJobStarted(string jobId)
        {
            _logger.Information("'{jobName}' STARTING", jobId);
        }

        private void LogJobResult(string jobId, Result<string, OperationError> result)
        {
            if (result.Ok)
            {
                LogJobSucceeded(jobId, result);
            }
            else
            {
                LogJobFailed(jobId, result);
            }
        }

        private void LogJobFailed(string jobId, Result<string, OperationError> result)
        {
            _logger.Error("'{jobName}' FAILED with '{error}'", jobId, result.Error);
        }

        private void LogJobSucceeded(string jobId, Result<string, OperationError> result)
        {
            _logger.Information("'{jobName}' SUCCEEDED with '{message}'", jobId, result.Value);
        }
    }
}
