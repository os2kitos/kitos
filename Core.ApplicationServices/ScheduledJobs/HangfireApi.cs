using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Hangfire;
using Hangfire.Annotations;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;

namespace Core.ApplicationServices.ScheduledJobs
{
    public class HangfireApi : IHangfireApi
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly JobStorage _jobStorage;

        public HangfireApi(IBackgroundJobClient backgroundJobClient, IRecurringJobManager recurringJobManager, JobStorage jobStorage)
        {
            _backgroundJobClient = backgroundJobClient;
            _recurringJobManager = recurringJobManager;
            _jobStorage = jobStorage;
        }

        public JobList<ScheduledJobDto> GetScheduledJobs(int fromIndex, int maxResponseLength)
        {
            return _jobStorage.GetMonitoringApi().ScheduledJobs(fromIndex, maxResponseLength);
        }

        public List<RecurringJobDto> GetRecurringJobs()
        {
            return _jobStorage.GetConnection().GetRecurringJobs();
        }

        public void DeleteScheduledJob(string jobId)
        {
            _backgroundJobClient.Delete(jobId);
        }

        public void RemoveRecurringJobIfExists(string jobId)
        {
            _recurringJobManager.RemoveIfExists(jobId);
        }

        public void Schedule([InstantHandle, NotNull] Expression<Action> action, DateTimeOffset? runAt)
        {
            if (runAt.HasValue)
            {
                _backgroundJobClient.Schedule(action, runAt.Value);
            }
            else
            {
                _backgroundJobClient.Enqueue(action);
            }
        }

        public void AddOrUpdateRecurringJob(string jobId, [InstantHandle, NotNull] Expression<Action> func, string cron)
        {
            _recurringJobManager.AddOrUpdate(jobId, func, cron);
        }
    }
}
