using System;
using System.Linq.Expressions;
using Hangfire;
using Hangfire.Annotations;
using Hangfire.Storage.Monitoring;

namespace Core.ApplicationServices.ScheduledJobs
{
    public class HangfireApi : IHangfireApi
    {
        public JobList<ScheduledJobDto> GetScheduledJobs(int fromIndex, int maxResponseLength)
        {
            return JobStorage.Current.GetMonitoringApi().ScheduledJobs(fromIndex, maxResponseLength);
        }

        public void DeleteScheduledJob(string jobId)
        {
            BackgroundJob.Delete(jobId);
        }

        public void RemoveRecurringJobIfExists(string jobId)
        {
            RecurringJob.RemoveIfExists(jobId);
        }

        public void Schedule([InstantHandle, NotNull]  Expression<Action> action, DateTimeOffset? runAt)
        {
            if (runAt.HasValue)
            {
                BackgroundJob.Schedule(action, runAt.Value);
            }
            else
            {
                BackgroundJob.Enqueue(action);
            }
        }

        public void AddOrUpdateRecurringJob(string jobId, [InstantHandle, NotNull] Expression<Action> func, string cron)
        {
            RecurringJob.AddOrUpdate(jobId, func, cron);
        }
    }
}
