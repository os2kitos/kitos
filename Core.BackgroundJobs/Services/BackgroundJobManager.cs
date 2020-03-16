using Core.BackgroundJobs.Model;
using Hangfire;
using Infrastructure.Services.BackgroundJobs;

namespace Core.BackgroundJobs.Services
{
    public class BackgroundJobManager : IBackgroundJobManager
    {
        public void TriggerLinkCheck()
        {
            RecurringJob.Trigger(StandardJobIds.CheckExternalLinks);
        }
    }
}
