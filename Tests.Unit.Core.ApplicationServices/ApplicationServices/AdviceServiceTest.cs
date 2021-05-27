﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Mail;
using Core.ApplicationServices;
using Core.ApplicationServices.Jobs;
using Core.DomainModel.Advice;
using Core.DomainServices;
using Infrastructure.Services.DataAccess;
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
        private readonly Mock<ITransactionManager> _transactionManager;

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
            _transactionManager = new Mock<ITransactionManager>();
            _sut.TransactionManager = _transactionManager.Object;
        }

        [Fact]
        public void SendAdvice_GivenImmediateActiveAdvice_AdviceIsSentImmediatelyAndJobIsNotRemovedAsThereIsNoJob()
        {
            //Arrange
            var immediateAdvice = new Advice
            {
                Id = A<int>(),
                Subject = A<string>(),
                AdviceType = AdviceType.Immediate
            };
            SetupAdviceRepository(immediateAdvice);
            SetupTransactionManager();

            //Act
            var result = _sut.SendAdvice(immediateAdvice.Id);

            //Assert
            Assert.True(result);
            _mailClientMock.Verify(x => x.Send(It.IsAny<MailMessage>()), Times.Once);
            _adviceSchedulerMock.Verify(x => x.Remove(immediateAdvice), Times.Never);
        }

        [Fact]
        public void SendAdvice_GivenRecurringActiveAdvice_AdviceIsSentAndJobIsNotCancelled()
        {
            //Arrange
            var recurringAdvice = new Advice
            {
                Id = A<int>(),
                Subject = A<string>(),
                AdviceType = AdviceType.Repeat,
                Scheduling = Scheduling.Quarter,
                AlarmDate = DateTime.Now.AddDays(-1),
                StopDate = DateTime.Now.AddDays(1)
            };
            SetupAdviceRepository(recurringAdvice);
            SetupTransactionManager();

            //Act
            var result = _sut.SendAdvice(recurringAdvice.Id);

            //Assert
            Assert.True(result);
            _mailClientMock.Verify(x => x.Send(It.IsAny<MailMessage>()), Times.Once);
            _adviceSchedulerMock.Verify(x => x.Remove(recurringAdvice), Times.Never);
        }

        [Fact]
        public void SendAdvice_GivenRecurringExpiringAdvice_AdviceIsNotSentAndJobIsCancelled()
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
            _mailClientMock.Verify(x => x.Send(It.IsAny<MailMessage>()), Times.Never);
            _adviceSchedulerMock.Verify(x => x.Remove(recurringAdvice), Times.Once);
        }

        [Fact]
        public void SendAdvice_GivenRecurringActiveAdviceWithLaterThanTodayAlarmDate_AdviceIsNotSentAndJobIsNotCancelled()
        {
            //Arrange
            var recurringAdvice = new Advice
            {
                Id = A<int>(),
                Subject = A<string>(),
                AdviceType = AdviceType.Repeat,
                Scheduling = Scheduling.Quarter,
                AlarmDate = DateTime.Now.AddDays(1),
                StopDate = DateTime.Now.AddDays(2)
            };
            SetupAdviceRepository(recurringAdvice);

            //Act
            var result = _sut.SendAdvice(recurringAdvice.Id);

            //Assert
            Assert.True(result);
            _mailClientMock.Verify(x => x.Send(It.IsAny<MailMessage>()), Times.Never);
            _adviceSchedulerMock.Verify(x => x.Remove(recurringAdvice), Times.Never);
        }

        private void SetupAdviceRepository(Advice advice)
        {
            var advices = new List<Advice> { advice };
            _adviceRepositoryMock.Setup(r => r.AsQueryable()).Returns(advices.AsQueryable);
            _adviceRepositoryMock.Setup(r => r.Update(advice));
            _adviceRepositoryMock.Setup(r => r.Save());
        }

        private void SetupTransactionManager()
        {
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin(IsolationLevel.Serializable)).Returns(transaction.Object);
        }
    }
}
