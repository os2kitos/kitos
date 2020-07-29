using System;
using System.Threading.Tasks;
using Core.BackgroundJobs.Model;
using Core.BackgroundJobs.Model.ExternalLinks;
using Core.DomainModel.Result;
using Infrastructure.Services.BackgroundJobs;
using Serilog;

namespace Core.BackgroundJobs.Services
{
    public class BackgroundJobLauncher : IBackgroundJobLauncher
    {
        private readonly ILogger _logger;
        private readonly CheckExternalLinksBackgroundJob _checkExternalLinksJob;

        public BackgroundJobLauncher(
            ILogger logger,
            CheckExternalLinksBackgroundJob checkExternalLinksJob)
        {
            _logger = logger;
            _checkExternalLinksJob = checkExternalLinksJob;
        }

        public async Task LaunchLinkCheckAsync()
        {
            await Launch(_checkExternalLinksJob);
        }

        private async Task Launch(IAsyncBackgroundJob job)
        {
            var jobId = job.Id;

            LogJobStarted(jobId);
            try
            {
                var result = await job.ExecuteAsync();
                LogJobResult(jobId, result);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error during execution of job {jobId}", jobId);
                throw;
            }
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
