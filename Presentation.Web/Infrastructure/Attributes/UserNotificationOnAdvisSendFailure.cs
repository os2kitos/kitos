using System.Linq;
using Core.ApplicationServices;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.Extensions.DependencyInjection;
using Ninject;
using Presentation.Web.Ninject;

namespace Presentation.Web.Infrastructure.Attributes
{
    public class AdvisSendFailureFilter : IElectStateFilter, IJobFilter
    {
        private static readonly string MatchType = typeof(AdviceService).FullName;
        protected static readonly string MatchMethod = typeof(AdviceService)
            .GetMethods()
            .First(x => x.Name.Equals(nameof(AdviceService.SendAdvice)))
            .Name;

        private readonly StandardKernel _kernel;

        public AdvisSendFailureFilter(StandardKernel kernel)
        {
            _kernel = kernel;
        }

        public void OnStateElection(ElectStateContext context)
        {


            if (context.CandidateState is FailedState failedState)
            {
                if (IsSendAdvice(context))
                {
                    var jobId = context.BackgroundJob.Id;

                    using (new HangfireNinjectResolutionScope(_kernel))
                    {
                        //TOD: JMO
                        var advisService = _kernel.GetService<IAdviceService>();
                        //var failedAdvice = _adviceService.GetAdviceByJobId(jobId);
                        //if (failedAdvice.IsNone)
                        //{
                        //    return;
                        //}
                        //var advice = failedAdvice.Value;

                        //if (advice.ObjectOwnerId == null || advice.RelationId == null || advice.Type == null)
                        //{
                        //    return;
                        //}

                        //_userNotificationService.AddUserNotification(advice.ObjectOwnerId.Value, advice.Name, "Afsendelse af advis fejlede", advice.RelationId.Value, advice.Type.Value, DomainModel.Notification.NotificationType.advice);
                    }
                }
            }
        }

        private static bool IsSendAdvice(StateContext context)
        {
            return context.BackgroundJob?.Job?.Type?.FullName?.Equals(MatchType) == true &&
                   context.BackgroundJob?.Job?.Method?.Name?.Equals(MatchMethod) == true;
        }

        public bool AllowMultiple => false;
        public int Order => 10;
    }
}
