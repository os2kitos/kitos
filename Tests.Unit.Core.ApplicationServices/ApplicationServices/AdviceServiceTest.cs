using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using AutoFixture;
using AutoFixture.AutoMoq;
using Core.ApplicationServices;
using Core.ApplicationServices.Helpers;
using Core.DomainModel.Advice;
using Core.DomainServices;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices
{
    public class AdviceServiceTest: WithAutoFixture
    {
        protected override void OnFixtureCreated(Fixture fixture)
        {
            base.OnFixtureCreated(fixture);
            fixture.Customize(new AutoMoqCustomization());
        }

        [Fact]
        public void SendAdvice_GivenImmediateActiveAdvice_AdviceIsSentImmediately()
        {
            var immediateAdvice = new Advice
            {
                Id = A<int>(), 
                Subject = A<string>(),
                AdviceType = AdviceType.Immediate
            };
            SetupAdviceRepository(immediateAdvice);
            var mailClient = Freeze<Mock<MailClient>>();
            mailClient.Setup(m => m.Send(A<MailMessage>()));
            var hangfireHelper = Freeze<Mock<HangfireHelper>>();
            hangfireHelper.Setup(h => h.RemoveFromHangfire(immediateAdvice)).Throws(new ApplicationException("Should not be called"));
            var sut = A<AdviceService>();
            var result = sut.SendAdvice(immediateAdvice.Id);
            Assert.True(result);
        }

        [Fact]
        public void SendAdvice_GivenRecurringExpiringAdvice_AdviceIsSentAndJobIsCancelled()
        {
            var recurringAdvice = new Advice
            {
                Id = A<int>(), 
                Subject = A<string>(),
                AdviceType = AdviceType.Repeat,
                Scheduling = Scheduling.Quarter,
                AlarmDate = DateTime.Now.AddDays(-1),
                StopDate = DateTime.Now.AddDays(-1)
            };
            SetupAdviceRepository(recurringAdvice);
            var mailClient = Freeze<Mock<MailClient>>();
            mailClient.Setup(m => m.Send(A<MailMessage>()));
            var hangfireHelper = Freeze <Mock<HangfireHelper>>();
            hangfireHelper.Setup(h => h.RemoveFromHangfire(recurringAdvice));
            var sut = A<AdviceService>();
            var result = sut.SendAdvice(recurringAdvice.Id);
            Assert.True(result);
        }

        private void SetupAdviceRepository(Advice recurringAdvice)
        {
            var advices = new List<Advice> {recurringAdvice};
            var adviceRepository = Freeze<Mock<IGenericRepository<Advice>>>();
            adviceRepository.Setup(r => r.AsQueryable()).Returns(advices.AsQueryable);
            adviceRepository.Setup(r => r.Update(recurringAdvice));
            adviceRepository.Setup(r => r.Save());
        }
    }
}
