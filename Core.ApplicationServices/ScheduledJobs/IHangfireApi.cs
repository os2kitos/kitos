using System;
using System.Linq.Expressions;
using Hangfire.Annotations;
using Hangfire.Storage.Monitoring;

namespace Core.ApplicationServices.ScheduledJobs
{
    /// <summary>
    /// Simple wrapper api on top of the static hangfire api
    /// </summary>
    public interface IHangfireApi
    {
        JobList<ScheduledJobDto> GetScheduledJobs(int fromIndex, int maxResponseLength);
        void DeleteScheduledJob(string jobId);
        void RemoveRecurringJobIfExists(string jobId);
        void Schedule([InstantHandle, NotNull] Expression<Action> action, DateTimeOffset? runAt = null);
        void AddOrUpdateRecurringJob(string jobId, [InstantHandle, NotNull] Expression<Action> func, string cron);
    }
}
