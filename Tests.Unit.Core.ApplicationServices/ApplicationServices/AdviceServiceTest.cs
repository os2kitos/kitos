using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Core.ApplicationServices;
using Core.ApplicationServices.Jobs;
using Core.DomainModel.Advice;
using Core.DomainServices;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices
{
    public class AdviceServiceTest : WithAutoFixture
    {
        private readonly AdviceService _sut;
        private readonly Mock<IMailClient> _mailClientMock;
        private readonly Mock<IAdviceScheduler> _adviceSchedulerMock;
        private readonly Mock<IGenericRepository<Advice>> _adviceRepositoryMock;

        public AdviceServiceTest()
        {
            _sut = new AdviceService();
            _mailClientMock = new Mock<IMailClient>();
            _sut.MailClient = _mailClientMock.Object;
            _adviceSchedulerMock = new Mock<IAdviceScheduler>();
            _sut.AdviceScheduler = _adviceSchedulerMock.Object;
            _adviceRepositoryMock = new Mock<IGenericRepository<Advice>>();
            _sut.AdviceRepository = _adviceRepositoryMock.Object;
            _sut.AdviceSentRepository = Mock.Of<IGenericRepository<AdviceSent>>();
        }

        [Fact]
        public void SendAdvice_GivenImmediateActiveAdvice_AdviceIsSentImmediately()
        {
            //Arrange
            var immediateAdvice = new Advice
            {
                Id = A<int>(),
                Subject = A<string>(),
                AdviceType = AdviceType.Immediate
            };
            SetupAdviceRepository(immediateAdvice);

            //Act
            var result = _sut.SendAdvice(immediateAdvice.Id);

            //Assert
            Assert.True(result);
            _mailClientMock.Verify(x => x.Send(It.IsAny<MailMessage>()), Times.Once);
            _adviceSchedulerMock.Verify(x => x.Remove(immediateAdvice), Times.Never);
        }

        [Fact]
        public void SendAdvice_GivenRecurringExpiringAdvice_AdviceIsSentAndJobIsCancelled()
        {
            //Arrange
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

            //Act
            var result = _sut.SendAdvice(recurringAdvice.Id);

            //Assert
            Assert.True(result);
            _mailClientMock.Verify(x => x.Send(It.IsAny<MailMessage>()), Times.Once);
            _adviceSchedulerMock.Verify(x => x.Remove(recurringAdvice), Times.Once);
        }

        private void SetupAdviceRepository(Advice recurringAdvice)
        {
            var advices = new List<Advice> { recurringAdvice };
            _adviceRepositoryMock.Setup(r => r.AsQueryable()).Returns(advices.AsQueryable);
            _adviceRepositoryMock.Setup(r => r.Update(recurringAdvice));
            _adviceRepositoryMock.Setup(r => r.Save());
        }
    }
}
