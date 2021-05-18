using Core.DomainModel.Advice;
using Hangfire;

namespace Core.ApplicationServices.Helpers
{
    public class HangfireHelper : IHangfireHelper
    {
        public virtual void RemoveFromHangfire(Advice advice)
        {
            RecurringJob.RemoveIfExists(advice.JobId);
        }
    }
}