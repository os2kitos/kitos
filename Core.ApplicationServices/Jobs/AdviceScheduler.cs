using Core.DomainModel.Advice;
using Hangfire;

namespace Core.ApplicationServices.Jobs
{
    public class AdviceScheduler : IAdviceScheduler
    {
        public void Remove(Advice advice)
        {
            RecurringJob.RemoveIfExists(advice.JobId);
        }
    }
}