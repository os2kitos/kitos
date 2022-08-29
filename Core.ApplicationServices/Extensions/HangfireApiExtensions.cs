using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.ScheduledJobs;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;

namespace Core.ApplicationServices.Extensions
{
    public static class HangfireApiExtensions
    {
        public static IEnumerable<(int adviceId, string jobId)> GetScheduledJobsIdInfo(this IHangfireApi api)
        {
            var jobList = api.GetScheduledJobs(0, int.MaxValue);

            return jobList.GetScheduledJobsIdInfo();
        }

        public static IEnumerable<(int adviceId, string jobId)> GetScheduledJobsIdInfo(this JobList<ScheduledJobDto> jobList)
        {
            //All pending creations (future advices) as well as pending deactivations
            var allScheduledJobs =
                jobList
                    .Where(jobs =>
                        jobs.Value.Job.Method.Name is nameof(AdviceService.CreateOrUpdateJob)
                            or nameof(AdviceService.DeactivateById))
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

            return jobList.GetRecurringJobsIdInfo();
        }

        public static IEnumerable<(int adviceId, string jobId)> GetRecurringJobsIdInfo(this List<RecurringJobDto> jobList)
        {
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
            var scheduledJobsIdInfo = api.GetScheduledJobsIdInfo();
            var recurringJobsIdInfo = api.GetRecurringJobsIdInfo();

            api.DeleteAdviceFromHangfire(adviceEntityId, scheduledJobsIdInfo, recurringJobsIdInfo);
        }

        public static void DeleteAdviceFromHangfire(this IHangfireApi api, int adviceEntityId, IEnumerable<(int adviceId, string jobId)> scheduledJobsIdInfo, IEnumerable<(int adviceId, string jobId)> recurringJobsIdInfo)
        {
            //Remove all pending calls to CreateOrUpdateJob and DeactivateById
            foreach (var scheduledJob in scheduledJobsIdInfo.Where(ids => MatchAdvice(adviceEntityId, ids)).Distinct().ToList())
            {
                api.DeleteScheduledJob(scheduledJob.jobId);
            }

            //Remove the job by main job id + any partitions (max 12 - one pr. month)
            foreach (var recurringJob in recurringJobsIdInfo.Where(ids => MatchAdvice(adviceEntityId, ids)).Distinct().ToList())
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
