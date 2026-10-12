using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Mail;
using Core.ApplicationServices;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.ScheduledJobs;
using Core.DomainModel;
using Core.DomainModel.Advice;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Notification;
using Core.DomainModel.Shared;
using Core.DomainServices;
using Core.DomainServices.Advice;
using Core.DomainServices.Notifications;
using Core.DomainServices.Time;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using Infrastructure.DataAccess;
using Infrastructure.Services.DataAccess;
using Microsoft.EntityFrameworkCore;
using Moq;
using Serilog;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices
{
    public class AdviceServiceTest : WithAutoFixture
    {
        private readonly AdviceService _sut;
        private readonly Mock<IMailClient> _mailClientMock;
        private readonly Mock<IGenericRepository<Advice>> _adviceRepositoryMock;
        private readonly Mock<IGenericRepository<ItContractRight>> _itContractRightsRepositoryMock;
        private readonly Mock<IGenericRepository<ItSystemRight>> _itSystemRightsRepositoryMock;
        private readonly Mock<IGenericRepository<DataProcessingRegistrationRight>> _dataProcessingRegistrationRightsRepositoryMock;
        private readonly Mock<ITransactionManager> _transactionManager;
        private readonly Mock<IHangfireApi> _hangfireApiMock;
        private readonly Mock<IUserNotificationService> _userNotificationService;
        private readonly Mock<IAdviceRootResolution> _adviceRootResolution;
        private readonly Mock<IOperationClock> _operationClockMock;
        private readonly Mock<IGenericRepository<AdviceSent>> _adviceSentRepositoryMock;

        public AdviceServiceTest()
        {
            _operationClockMock = new Mock<IOperationClock>();
            _operationClockMock.Setup(x => x.Now).Returns(DateTime.Now);
            _mailClientMock = new Mock<IMailClient>();
            _adviceRepositoryMock = new Mock<IGenericRepository<Advice>>();
            _itContractRightsRepositoryMock = new Mock<IGenericRepository<ItContractRight>>();
            _itSystemRightsRepositoryMock = new Mock<IGenericRepository<ItSystemRight>>();
            _dataProcessingRegistrationRightsRepositoryMock = new Mock<IGenericRepository<DataProcessingRegistrationRight>>();
            _adviceSentRepositoryMock = new Mock<IGenericRepository<AdviceSent>>();
            _transactionManager = new Mock<ITransactionManager>();
            _hangfireApiMock = new Mock<IHangfireApi>();
            _userNotificationService = new Mock<IUserNotificationService>();
            _adviceRootResolution = new Mock<IAdviceRootResolution>();
            _sut = new AdviceService(
                _mailClientMock.Object,
                _adviceRepositoryMock.Object,
                _adviceSentRepositoryMock.Object,
                _itContractRightsRepositoryMock.Object,
                _itSystemRightsRepositoryMock.Object,
                _dataProcessingRegistrationRightsRepositoryMock.Object,
                Mock.Of<ILogger>(),
                _transactionManager.Object,
                Mock.Of<IOrganizationalUserContext>(),
                _hangfireApiMock.Object,
                _operationClockMock.Object,
                _userNotificationService.Object,
                _adviceRootResolution.Object,
                Mock.Of<IGenericRepository<DataProcessingRegistration>>(),
                Mock.Of<IGenericRepository<ItContract>>(),
                Mock.Of<IGenericRepository<ItSystemUsage>>()
            );

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
                RelationId = A<int>(),
                IsActive = true
            };
            SetupAdviceRepository(immediateAdvice);
            SetupTransactionManager();
            _adviceRootResolution.Setup(x => x.Resolve(immediateAdvice)).Returns(new ItSystemUsage { OrganizationId = A<int>() });
            _userNotificationService.Setup(x => x.AddUserNotification(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), immediateAdvice.Type, NotificationType.Advice)).Returns(new UserNotification());

            //Act
            var result = _sut.SendAdvice(immediateAdvice.Id);

            //Assert
            Assert.True(result);
            _mailClientMock.Verify(x => x.Send(It.IsAny<MailMessage>()), Times.Never);
        }

        [Theory]
        [InlineData(RelatedEntityType.itContract)]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        public void SendAdvice_GivenRoleRecipient_PostgreSqlProjectsActiveUserEmails(RelatedEntityType entityType)
        {
            using var context = new KitosContext(new DbContextOptionsBuilder<KitosContext>()
                .UseNpgsql("Host=localhost;Database=kitos;Username=postgres")
                .Options);
            string? sql = null;
            void CaptureSql(string query) => sql = query;
            _itContractRightsRepositoryMock.Setup(x => x.AsQueryable())
                .Returns(new SqlCapturingQueryable<ItContractRight>(context.Set<ItContractRight>(), CaptureSql));
            _itSystemRightsRepositoryMock.Setup(x => x.AsQueryable())
                .Returns(new SqlCapturingQueryable<ItSystemRight>(context.Set<ItSystemRight>(), CaptureSql));
            _dataProcessingRegistrationRightsRepositoryMock.Setup(x => x.AsQueryable())
                .Returns(new SqlCapturingQueryable<DataProcessingRegistrationRight>(
                    context.Set<DataProcessingRegistrationRight>(), CaptureSql));
            var advice = new Advice
            {
                Id = A<int>(),
                Subject = A<string>(),
                AdviceType = AdviceType.Immediate,
                Type = entityType,
                RelationId = 17,
                Reciepients = new List<AdviceUserRelation>
                {
                    new()
                    {
                        RecieverType = RecieverType.RECIEVER,
                        RecpientType = RecipientType.ROLE,
                        ItContractRoleId = 23,
                        ItSystemRoleId = 23,
                        DataProcessingRegistrationRoleId = 23
                    }
                },
                IsActive = true
            };
            SetupAdviceRepository(advice);
            SetupTransactionManager();

            Assert.Throws<QueryCapturedException>(() => _sut.SendAdvice(advice.Id));

            Assert.NotNull(sql);
            Assert.Matches("SELECT [a-z][a-z0-9]*\\.\"Email\"\\r?\\nFROM", sql);
            Assert.Contains("JOIN", sql);
            Assert.Contains("\"User\"", sql);
            Assert.Contains("@advice_RelationId='17'", sql);
            Assert.Matches("\"ObjectId\" = @advice_RelationId", sql);
            Assert.Matches("@\\w*RoleId='23'", sql);
            Assert.Matches("\"RoleId\" = @\\w*RoleId", sql);
            Assert.Matches("NOT \\(?[a-z][a-z0-9]*\\.\"Deleted\"\\)?", sql);
        }

        [Fact]
        public void SendAdvice_GivenContractRoleRecipient_EmailIsSentToAssignedUser()
        {
            //Arrange
            const int contractId = 17;
            const int contractRoleId = 23;
            const string userEmail = "contract-user@kitos.dk";
            var advice = new Advice
            {
                Id = A<int>(),
                Subject = A<string>(),
                AdviceType = AdviceType.Immediate,
                Type = RelatedEntityType.itContract,
                RelationId = contractId,
                Reciepients = new List<AdviceUserRelation>
                {
                    new()
                    {
                        RecieverType = RecieverType.RECIEVER,
                        RecpientType = RecipientType.ROLE,
                        ItContractRoleId = contractRoleId
                    }
                },
                IsActive = true
            };
            var assignedRight = new ItContractRight
            {
                ObjectId = contractId,
                RoleId = contractRoleId,
                User = new User { Email = userEmail }
            };
            _itContractRightsRepositoryMock.Setup(x => x.AsQueryable())
                .Returns(new[] { assignedRight }.AsQueryable);
            SetupAdviceRepository(advice);
            SetupTransactionManager();

            //Act
            var result = _sut.SendAdvice(advice.Id);

            //Assert
            Assert.True(result);
            _mailClientMock.Verify(x => x.Send(It.Is<MailMessage>(message =>
                message.To.Any(recipient => recipient.Address == userEmail))), Times.Once);
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
                Reciepients = CreateDefaultReceivers(),
                IsActive = true
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
        public void SendAdvice_GivenImmediateAdvice_Which_Is_Inactive_No_Email_Sent()
        {
            //Arrange
            var immediateAdvice = new Advice
            {
                Id = A<int>(),
                Subject = A<string>(),
                AdviceType = AdviceType.Immediate,
                Reciepients = CreateDefaultReceivers(),
                IsActive = false
            };
            SetupAdviceRepository(immediateAdvice);
            SetupTransactionManager();
            _hangfireApiMock.Setup(x => x.GetScheduledJobs(0, int.MaxValue)).Returns(new JobList<ScheduledJobDto>(Array.Empty<KeyValuePair<string, ScheduledJobDto>>()));
            _hangfireApiMock.Setup(x => x.GetRecurringJobs()).Returns(new List<RecurringJobDto>());

            //Act
            var result = _sut.SendAdvice(immediateAdvice.Id);

            //Assert
            Assert.True(result);

            _mailClientMock.Verify(x => x.Send(It.IsAny<MailMessage>()), Times.Never());
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
                Reciepients = CreateDefaultReceivers(),
                IsActive = true
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
        public void SendAdvice_GivenRecurringAdvice_Which_Is_Inactive_Email_Not_Sent()
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
                Reciepients = CreateDefaultReceivers(),
                IsActive = false
            };
            SetupAdviceRepository(recurringAdvice);
            SetupTransactionManager();
            _hangfireApiMock.Setup(x => x.GetScheduledJobs(0, int.MaxValue)).Returns(new JobList<ScheduledJobDto>(Array.Empty<KeyValuePair<string, ScheduledJobDto>>()));
            _hangfireApiMock.Setup(x => x.GetRecurringJobs()).Returns(new List<RecurringJobDto>());

            //Act
            var result = _sut.SendAdvice(recurringAdvice.Id);

            //Assert
            Assert.True(result);
            _mailClientMock.Verify(x => x.Send(It.IsAny<MailMessage>()), Times.Never());
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
                Reciepients = CreateDefaultReceivers(),
                IsActive = true
            };
            _operationClockMock.Setup(x => x.Now).Returns(DateTime.MaxValue);
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
                Reciepients = CreateDefaultReceivers(),
                IsActive = true
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
            _hangfireApiMock.Setup(x => x.GetScheduledJobs(0, int.MaxValue)).Returns(new JobList<ScheduledJobDto>(Array.Empty<KeyValuePair<string, ScheduledJobDto>>()));
            _hangfireApiMock.Setup(x => x.GetRecurringJobs()).Returns(new List<RecurringJobDto>());

            //Act
            var result = _sut.SendAdvice(recurringAdvice.Id);

            //Assert
            Assert.True(result);
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
                Reciepients = CreateDefaultReceivers(),
                IsActive = true
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
        public void SendAdvice_SavesAdviceSentWithAdviceOwner()
        {
            //Arrange
            var ownerId = A<int>();
            var immediateAdvice = new Advice
            {
                Id = A<int>(),
                Subject = A<string>(),
                AdviceType = AdviceType.Immediate,
                Reciepients = CreateDefaultReceivers(),
                IsActive = true,
                ObjectOwnerId = ownerId
            };
            SetupAdviceRepository(immediateAdvice);
            SetupTransactionManager();

            //Act
            var result = _sut.SendAdvice(immediateAdvice.Id);

            //Assert
            Assert.True(result);
            _adviceSentRepositoryMock.Verify(x => x.Insert(It.Is<AdviceSent>(sent =>
                sent.AdviceId == immediateAdvice.Id &&
                sent.ObjectOwnerId == ownerId &&
                sent.LastChangedByUserId == ownerId)), Times.Once);
        }

        private void SetupAdviceRepository(Advice advice)
        {
            var advices = new List<Advice> { advice };
            _adviceRepositoryMock.Setup(r => r.AsQueryable()).Returns(advices.AsQueryable);
        }

        private void SetupTransactionManager()
        {
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);
        }

        private static List<AdviceUserRelation> CreateDefaultReceivers()
        {
            return new List<AdviceUserRelation> { new() { RecieverType = RecieverType.RECIEVER, RecpientType = RecipientType.USER, Email = "test@kitos.dk" } };
        }

        // Preserve Npgsql query translation, but stop before database execution.
        private sealed class SqlCapturingQueryable<T>(IQueryable<T> query, Action<string> captureSql) : IQueryable<T>
        {
            public Type ElementType => typeof(T);
            public Expression Expression => query.Expression;
            public IQueryProvider Provider => new SqlCapturingQueryProvider(query.Provider, captureSql);

            public IEnumerator<T> GetEnumerator()
            {
                captureSql(query.ToQueryString());
                throw new QueryCapturedException();
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private sealed class SqlCapturingQueryProvider(IQueryProvider provider, Action<string> captureSql) : IQueryProvider
        {
            public IQueryable<TElement> CreateQuery<TElement>(Expression expression) =>
                new SqlCapturingQueryable<TElement>(provider.CreateQuery<TElement>(expression), captureSql);

            public IQueryable CreateQuery(Expression expression) =>
                throw new NotSupportedException("Only generic query composition is supported.");

            public object? Execute(Expression expression) =>
                throw new NotSupportedException("Database execution is not supported.");

            public TResult Execute<TResult>(Expression expression) =>
                throw new NotSupportedException("Database execution is not supported.");
        }

        private sealed class QueryCapturedException : Exception;
    }
}
