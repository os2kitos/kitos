using Core.BackgroundJobs.Model;
using Hangfire;
using Infrastructure.Services.BackgroundJobs;

namespace Core.BackgroundJobs.Services
{
    public class BackgroundJobScheduler : IBackgroundJobScheduler
    {
        private readonly IRecurringJobManager _recurringJobManager;

        public BackgroundJobScheduler(IRecurringJobManager recurringJobManager)
        {
            _recurringJobManager = recurringJobManager;
        }

        public void ScheduleLinkCheckForImmediateExecution()
        {
            _recurringJobManager.Trigger(StandardJobIds.CheckExternalLinks);
        }
    }
}
