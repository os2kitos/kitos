using Core.BackgroundJobs.Model;
using Hangfire;
using Infrastructure.Services.BackgroundJobs;

namespace Core.BackgroundJobs.Services
{
    public class BackgroundJobScheduler : IBackgroundJobScheduler
    {
        public void ScheduleLinkCheck()
        {
            RecurringJob.Trigger(StandardJobIds.CheckExternalLinks);
        }
    }
}
