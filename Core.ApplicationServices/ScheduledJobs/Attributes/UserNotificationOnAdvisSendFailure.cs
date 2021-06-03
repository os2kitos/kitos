using Core.ApplicationServices.Notification;
using Hangfire.Common;
using Hangfire.States;

namespace Core.ApplicationServices.ScheduledJobs.Attributes
{
    public class AdvisSendFailureAttribute : JobFilterAttribute, IElectStateFilter
    {
        public void OnStateElection(ElectStateContext context)
        {
            var failedState = context.CandidateState as FailedState;
            if (failedState != null)
            {
                var jobId = context.BackgroundJob.Id;
            }
        }
    }

    public class AdvisSendFailureFilter : IElectStateFilter
    {

        public readonly IAdviceService _adviceService;
        public readonly IUserNotificationService _userNotificationService;

        public AdvisSendFailureFilter(IAdviceService adviceService, IUserNotificationService userNotificationService)
        {
            _adviceService = adviceService;
            _userNotificationService = userNotificationService;
        }

        public void OnStateElection(ElectStateContext context)
        {
            var failedState = context.CandidateState as FailedState;
            if (failedState != null)
            {
                var jobId = context.BackgroundJob.Id;


                var failedAdvice = _adviceService.GetAdviceByJobId(jobId);
                if (failedAdvice.IsNone)
                {
                    return;
                }
                var advice = failedAdvice.Value;

                if (advice.ObjectOwnerId == null || advice.RelationId == null || advice.Type == null)
                {
                    return;
                }

                _userNotificationService.AddUserNotification(advice.ObjectOwnerId.Value, advice.Name, "Afsendelse af advis fejlede", advice.RelationId.Value, advice.Type.Value, DomainModel.Notification.NotificationType.advice);
            }
        }
    }
}
