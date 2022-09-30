using Core.DomainModel.Advice;
using Core.DomainModel.Shared;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using System;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;
using Presentation.Web.Models.API.V1;

namespace Tests.Integration.Presentation.Web.Advice
{
    public abstract class AdviceTestBase : WithAutoFixture, IAsyncLifetime
    {
        public async Task InitializeAsync()
        {
            Root = await ItContractHelper.CreateContract(A<string>(), OrganizationId);
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        private ItContractDTO _root;
        protected const int OrganizationId = TestEnvironment.DefaultOrganizationId;

        protected Core.DomainModel.Advice.Advice CreateDefaultAdvice(Scheduling schedule, AdviceType type, AdviceUserRelation recipient)
        {
            return new Core.DomainModel.Advice.Advice
            {
                RelationId = Root.Id,
                Type = RelatedEntityType.itContract,
                Body = A<string>(),
                Subject = A<string>(),
                Scheduling = schedule,
                AdviceType = type,
                Reciepients = new List<AdviceUserRelation>()
                {
                    recipient
                },
                AlarmDate = DateTime.Now,
                StopDate = GetRandomDateAfterToday(),
            };
        }


        protected AdviceUserRelation CreateDefaultEmailRecipient(string name)
        {
            return new AdviceUserRelation
            {
                Email = name,
                RecieverType = RecieverType.RECIEVER,
                RecpientType = RecipientType.USER
            };
        }

        protected string CreateWellformedEmail()
        {
            //Make sure special chars are part of the test email
            return $"{A<string>()}_a.b-c@test.dk";
        }

        protected DateTime GetRandomDateAfterToday()
        {
            return DateTime.Now.AddDays(Math.Abs(A<int>()));
        }

        protected static async Task WaitForAsync(Func<Task<bool>> check, TimeSpan howLong)
        {
            bool conditionMet;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            do
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(250));
                conditionMet = await check();
            } while (conditionMet == false && stopwatch.Elapsed <= howLong);

            Assert.True(conditionMet, $"Failed to meet required condition within {howLong.TotalMilliseconds} milliseconds");
        }

        protected ItContractDTO Root
        {
            get => _root;
            set => _root = value;
        }
    }
}
