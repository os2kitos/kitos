using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.ScheduledJobs;

namespace Core.ApplicationServices.Extensions
{
    public static class HangfireApiExtensions
    {
        public static IEnumerable<(int adviceId, string jobId)> GetScheduledJobsIdInfo(this IHangfireApi api)
        {
            var jobList = api.GetScheduledJobs(0, int.MaxValue);

            //All pending creations (future advices) as well as pending deactivations
            var allScheduledJobs =
                jobList
                    .Where(jobs => jobs.Value.Job.Method.Name is nameof(AdviceService.CreateOrUpdateJob) or nameof(AdviceService.DeactivateById))
                    .ToList();

            foreach (var j in allScheduledJobs)
            {
                var adviceIdAsString = j.Value.Job.Args[0].ToString();
                if (int.TryParse(adviceIdAsString, out var adviceId))
                {
                    yield return (adviceId, j.Key);
                }
            }
        }

        public static IEnumerable<(int adviceId, string jobId)> GetRecurringJobsIdInfo(this IHangfireApi api)
        {
            var jobList = api.GetRecurringJobs();

            var allRecurringJobs =
                jobList
                    .Where(jobs => jobs.Job.Method.Name is nameof(AdviceService.SendAdvice))
                    .ToList();

            foreach (var job in allRecurringJobs)
            {
                var adviceIdAsString = job.Job.Args[0].ToString();
                if (int.TryParse(adviceIdAsString, out var adviceId))
                {
                    yield return (adviceId, job.Id);
                }
            }
        }

        public static void DeleteAdviceFromHangfire(this IHangfireApi api, int adviceEntityId)
        {
            //Remove all pending calls to CreateOrUpdateJob and DeactivateById
            foreach (var scheduledJob in api.GetScheduledJobsIdInfo().Where(ids => MatchAdvice(adviceEntityId, ids)).Distinct().ToList())
            {
                api.DeleteScheduledJob(scheduledJob.jobId);
            }

            //Remove the job by main job id + any partitions (max 12 - one pr. month)
            foreach (var recurringJob in api.GetRecurringJobsIdInfo().Where(ids => MatchAdvice(adviceEntityId, ids)).Distinct().ToList())
            {
                api.RemoveRecurringJobIfExists(recurringJob.jobId);
            }
        }

        private static bool MatchAdvice(int adviceEntityId, (int adviceId, string jobId) ids)
        {
            return ids.adviceId == adviceEntityId;
        }
    }
}
