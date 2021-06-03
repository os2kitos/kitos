using System;
using System.Linq;
using Core.ApplicationServices;
using Core.DomainModel.Notification;
using Core.DomainServices.Notifications;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.Extensions.DependencyInjection;
using Ninject;
using Presentation;
using Presentation.Web;
using Presentation.Web.Infrastructure;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Filters;
using Presentation.Web.Ninject;

namespace Presentation.Web.Infrastructure.Filters
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
                if(context.GetJobParameter<bool>("NoMoreRetries"))
                {
                    if (IsSendAdvice(context))
                    {
                        // Get argument input to method. In this case it's only one argument and it is the Id of the advice.
                        int adviceId = Convert.ToInt32(context.BackgroundJob.Job.Args.First());
                        if(adviceId > 0)
                        {
                            using (new HangfireNinjectResolutionScope(_kernel))
                            {
                                var advisService = _kernel.GetService<IAdviceService>();
                                var userNotificationService = _kernel.GetService<IUserNotificationService>();

                                var failedAdvice = advisService.GetAdviceById(adviceId);
                                if (failedAdvice.IsNone)
                                {
                                    return;
                                }
                                var advice = failedAdvice.Value;

                                if (advice.ObjectOwnerId == null || advice.RelationId == null || advice.Type == null)
                                {
                                    return;
                                }

                                userNotificationService.AddUserNotification(1, advice.ObjectOwnerId.Value, advice.Name, "Afsendelse af advis fejlede", advice.RelationId.Value, advice.Type.Value, NotificationType.Advice);
                            }
                        }
                    }
                }
                else if (context.GetJobParameter<int>("RetryCount") >= 2)
                {
                    context.SetJobParameter<bool>("NoMoreRetries", true);
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
