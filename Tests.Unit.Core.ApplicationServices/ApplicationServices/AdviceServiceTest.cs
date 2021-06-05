using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Mail;
using Core.ApplicationServices;
using Core.ApplicationServices.ScheduledJobs;
using Core.DomainModel.Advice;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Notification;
using Core.DomainModel.Shared;
using Core.DomainServices;
using Core.DomainServices.Advice;
using Core.DomainServices.Notifications;
using Core.DomainServices.Time;
using Hangfire.Storage.Monitoring;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.Types;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices
{
    public class AdviceServiceTest : WithAutoFixture
    {
        private readonly AdviceService _sut;
        private readonly Mock<IMailClient> _mailClientMock;
        private readonly Mock<IGenericRepository<Advice>> _adviceRepositoryMock;
        private readonly Mock<ITransactionManager> _transactionManager;
        private readonly Mock<IHangfireApi> _hangfireApiMock;
        private readonly Mock<IGenericRepository<ItContract>> _contractRepository;
        private readonly Mock<IGenericRepository<ItProject>> _projectRepository;
        private readonly Mock<IGenericRepository<ItSystemUsage>> _systemUsageRepository;
        private readonly Mock<IGenericRepository<DataProcessingRegistration>> _dataProcessingRepository;
        private readonly Mock<IUserNotificationService> _userNotificationService;
        private readonly Mock<IAdviceRootResolution> _adviceRootResolution;

        public AdviceServiceTest()
        {
            _sut = new AdviceService();
            _mailClientMock = new Mock<IMailClient>();
            _sut.MailClient = _mailClientMock.Object;
            _adviceRepositoryMock = new Mock<IGenericRepository<Advice>>();
            _sut.AdviceRepository = _adviceRepositoryMock.Object;
            _sut.AdviceSentRepository = Mock.Of<IGenericRepository<AdviceSent>>();
            _transactionManager = new Mock<ITransactionManager>();
            _sut.TransactionManager = _transactionManager.Object;
            _hangfireApiMock = new Mock<IHangfireApi>();
            _sut.HangfireApi = _hangfireApiMock.Object;
            _sut.OperationClock = Mock.Of<IOperationClock>(x => x.Now == DateTime.Now);

            _contractRepository = new Mock<IGenericRepository<ItContract>>();
            _sut.ItContractRepository = _contractRepository.Object;
            _projectRepository = new Mock<IGenericRepository<ItProject>>();
            _sut.ItProjectRepository = _projectRepository.Object;
            _systemUsageRepository = new Mock<IGenericRepository<ItSystemUsage>>();
            _sut.ItSystemUsageRepository = _systemUsageRepository.Object;
            _dataProcessingRepository = new Mock<IGenericRepository<DataProcessingRegistration>>();
            _sut.DataProcessingRegistrations = _dataProcessingRepository.Object;
            _userNotificationService = new Mock<IUserNotificationService>();
            _sut.UserNotificationService = _userNotificationService.Object;
            _adviceRootResolution = new Mock<IAdviceRootResolution>();
            _sut.AdviceRootResolution = _adviceRootResolution.Object;

        }

        [Fact]
        public void SendAdvice_GivenNoReceivers_EmailIsNotSent()
        {
            //Arrange
            var immediateAdvice = new Advice
            {
                Id = A<int>(),
                Subject = A<string>(),
                AdviceType = AdviceType.Immediate,
                ObjectOwnerId = A<int>(),
                Type = A<RelatedEntityType>(),
                RelationId = A<int>()
            };
            SetupAdviceRepository(immediateAdvice);
            SetupTransactionManager();
            _adviceRootResolution.Setup(x => x.Resolve(immediateAdvice)).Returns(new ItSystemUsage { OrganizationId = A<int>()});
            _userNotificationService.Setup(x => x.AddUserNotification(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), immediateAdvice.Type.Value, NotificationType.Advice)).Returns(new UserNotification());

            //Act
            var result = _sut.SendAdvice(immediateAdvice.Id);

            //Assert
            Assert.True(result);
            _mailClientMock.Verify(x => x.Send(It.IsAny<MailMessage>()), Times.Never);
        }


        [Fact]
        public void SendAdvice_GivenImmediateActiveAdvice_AdviceIsSentImmediatelyAndJobIsNotRemovedAsThereIsNoJob()
        {
            //Arrange
            var immediateAdvice = new Advice
            {
                Id = A<int>(),
                Subject = A<string>(),
                AdviceType = AdviceType.Immediate,
                Reciepients = CreateDefaultReceivers()
            };
            SetupAdviceRepository(immediateAdvice);
            SetupTransactionManager();

            //Act
            var result = _sut.SendAdvice(immediateAdvice.Id);

            //Assert
            Assert.True(result);
            _mailClientMock.Verify(x => x.Send(It.IsAny<MailMessage>()), Times.Once);
            _hangfireApiMock.Verify(x => x.RemoveRecurringJobIfExists(It.IsAny<string>()), Times.Never);
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
                StopDate = DateTime.Now.AddDays(1),
                Reciepients = CreateDefaultReceivers()
            };
            SetupAdviceRepository(recurringAdvice);
            SetupTransactionManager();

            //Act
            var result = _sut.SendAdvice(recurringAdvice.Id);

            //Assert
            Assert.True(result);
            _mailClientMock.Verify(x => x.Send(It.IsAny<MailMessage>()), Times.Once);
        }

        [Fact]
        public void SendAdvice_GivenRecurringActiveAdvice_And_No_StopDate_At_Worlds_End_AdviceIsSentAndJobIsNotCancelled()
        {
            //Arrange
            var recurringAdvice = new Advice
            {
                Id = A<int>(),
                Subject = A<string>(),
                AdviceType = AdviceType.Repeat,
                Scheduling = Scheduling.Quarter,
                AlarmDate = DateTime.Now.AddDays(-1),
                StopDate = null,
                Reciepients = CreateDefaultReceivers()
            };
            _sut.OperationClock = Mock.Of<IOperationClock>(x => x.Now == DateTime.MaxValue);
            SetupAdviceRepository(recurringAdvice);
            SetupTransactionManager();

            //Act
            var result = _sut.SendAdvice(recurringAdvice.Id);

            //Assert - even at end of days, if no expiration is set, we continue to send the advice
            Assert.True(result);
            _mailClientMock.Verify(x => x.Send(It.IsAny<MailMessage>()), Times.Once);
        }

        [Fact]
        public void SendAdvice_GivenRecurringActiveAdvice_Not_Yet_InScope_NoAdviceIsSentAndJobIsNotCancelled()
        {
            //Arrange
            var recurringAdvice = new Advice
            {
                Id = A<int>(),
                Subject = A<string>(),
                AdviceType = AdviceType.Repeat,
                Scheduling = Scheduling.Quarter,
                AlarmDate = DateTime.Now.AddYears(4),
                StopDate = DateTime.Now.AddYears(5),
                Reciepients = CreateDefaultReceivers()
            };
            SetupAdviceRepository(recurringAdvice);
            SetupTransactionManager();

            //Act
            var result = _sut.SendAdvice(recurringAdvice.Id);

            //Assert
            Assert.True(result);
            _mailClientMock.Verify(x => x.Send(It.IsAny<MailMessage>()), Times.Never);
        }

        [Fact]
        public void SendAdvice_GivenRecurringExpiringAdvice_AdviceIsNotSentAndJobIsCancelled()
        {
            //Arrange
            var id = A<int>();
            var recurringAdvice = new Advice
            {
                Id = id,
                Subject = A<string>(),
                AdviceType = AdviceType.Repeat,
                Scheduling = Scheduling.Quarter,
                AlarmDate = DateTime.Now.AddDays(-1),
                StopDate = DateTime.Now.AddDays(-1),
                JobId = "Advice: " + id,
                Reciepients = CreateDefaultReceivers()
            };
            SetupAdviceRepository(recurringAdvice);
            SetupTransactionManager();
            _hangfireApiMock.Setup(x => x.GetScheduledJobs(0, int.MaxValue)).Returns(new JobList<ScheduledJobDto>(new KeyValuePair<string, ScheduledJobDto>[0]));

            //Act
            var result = _sut.SendAdvice(recurringAdvice.Id);

            //Assert
            Assert.True(result);

            //No email sent for expired advice
            _mailClientMock.Verify(x => x.Send(It.IsAny<MailMessage>()), Times.Never);

            //Removed by main job id
            _hangfireApiMock.Verify(x => x.RemoveRecurringJobIfExists(recurringAdvice.JobId), Times.Once);
            for (int i = 0; i < 12; i++)
            {
                //Possible partitions are also removed
                _hangfireApiMock.Verify(x => x.RemoveRecurringJobIfExists($"{recurringAdvice.JobId}_part_{i}"), Times.Once);
            }
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
                StopDate = DateTime.Now.AddDays(2),
                Reciepients = CreateDefaultReceivers()
            };
            SetupAdviceRepository(recurringAdvice);
            SetupTransactionManager();

            //Act
            var result = _sut.SendAdvice(recurringAdvice.Id);

            //Assert
            Assert.True(result);
            _mailClientMock.Verify(x => x.Send(It.IsAny<MailMessage>()), Times.Never);
        }

        private void SetupAdviceRepository(Advice advice)
        {
            var advices = new List<Advice> { advice };
            _adviceRepositoryMock.Setup(r => r.AsQueryable()).Returns(advices.AsQueryable);
        }

        private void SetupTransactionManager()
        {
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin(IsolationLevel.ReadCommitted)).Returns(transaction.Object);
        }

        private static List<AdviceUserRelation> CreateDefaultReceivers()
        {
            return new List<AdviceUserRelation> { new() { RecieverType = RecieverType.RECIEVER, RecpientType = RecieverType.USER, Name = "test@kitos.dk" } };
        }
    }
}
