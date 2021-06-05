using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Notification;
using Core.DomainModel.Shared;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainServices.Notifications;
using Core.DomainServices.Repositories.Contract;
using Core.DomainServices.Repositories.GDPR;
using Core.DomainServices.Repositories.Notification;
using Core.DomainServices.Repositories.Project;
using Core.DomainServices.Repositories.SystemUsage;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.Types;
using Moq;
using System.Data;
using Tests.Toolkit.Patterns;
using Xunit;
using Core.DomainModel.GDPR;
using Core.DomainModel.Result;
using Core.DomainServices.Time;
using System;

namespace Tests.Unit.Core.DomainServices.Notification
{
    public class UserNotificationServiceTest : WithAutoFixture
    {
        private readonly UserNotificationService _sut;
        private readonly Mock<IUserNotificationRepository> _userNotificationRepository;
        private readonly Mock<ITransactionManager> _transactionManager;
        private readonly Mock<IItSystemUsageRepository> _systemUsageRepository;
        private readonly Mock<IItContractRepository> _contractRepository;
        private readonly Mock<IItProjectRepository> _projectRepository;
        private readonly Mock<IDataProcessingRegistrationRepository> _dataProcessingRepository;
        private readonly Mock<IOperationClock> _operationClock;

        public UserNotificationServiceTest()
        {
            _userNotificationRepository = new Mock<IUserNotificationRepository>();
            _transactionManager = new Mock<ITransactionManager>();
            _systemUsageRepository = new Mock<IItSystemUsageRepository>();
            _contractRepository = new Mock<IItContractRepository>();
            _projectRepository = new Mock<IItProjectRepository>();
            _dataProcessingRepository = new Mock<IDataProcessingRegistrationRepository>();
            _operationClock = new Mock<IOperationClock>();
            _sut = new UserNotificationService(
                _userNotificationRepository.Object,
                _transactionManager.Object,
                _systemUsageRepository.Object,
                _contractRepository.Object,
                _projectRepository.Object,
                _dataProcessingRepository.Object,
                _operationClock.Object);
        }

        [Fact]
        public void Can_Create()
        {
            //Arrange
            var relatedEntityType = A<RelatedEntityType>();
            var relatedEntityId = A<int>();
            var now = DateTime.Now;
            var notification = new UserNotification(A<string>(), A<string>(), NotificationType.Advice, A<int>(), A<int>(), now);
            var notificationWithRelatedEntity = ExpectRelatedEntity(relatedEntityType, relatedEntityId, notification);

            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin(IsolationLevel.ReadCommitted)).Returns(transaction.Object);
            _userNotificationRepository.Setup(x => x.Add(It.IsAny<UserNotification>())).Returns(notificationWithRelatedEntity);
            _operationClock.Setup(x => x.Now).Returns(now);

            //Act
            var result = _sut.AddUserNotification(notificationWithRelatedEntity.OrganizationId, notificationWithRelatedEntity.NotificationRecipientId, notificationWithRelatedEntity.Name, notificationWithRelatedEntity.NotificationMessage, relatedEntityId, relatedEntityType, notification.NotificationType);

            //Assert
            Assert.True(result.Ok);
            transaction.Verify(x => x.Commit());
        }

        [Fact]
        public void Can_Delete()
        {
            //Arrange
            var now = DateTime.Now;
            var notification = new UserNotification(A<string>(), A<string>(), NotificationType.Advice, A<int>(), A<int>(), now);

            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin(IsolationLevel.ReadCommitted)).Returns(transaction.Object); 
            _userNotificationRepository.Setup(x => x.DeleteById(notification.Id)).Returns(Maybe<OperationError>.None);

            //Act
            var result = _sut.Delete(notification.Id);

            //Assert
            Assert.True(result.IsNone);
            transaction.Verify(x => x.Commit());
        }

        [Fact]
        public void Can_Get()
        {
            //Arrange
            var now = DateTime.Now;
            var notification = new UserNotification(A<string>(), A<string>(), NotificationType.Advice, A<int>(), A<int>(), now);

            _userNotificationRepository.Setup(x => x.GetById(notification.Id)).Returns(notification);

            //Act
            var result = _sut.GetUserNotification(notification.Id);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(notification, result.Value);
        }

        [Fact]
        public void Can_Not_Get_If_Not_Exists()
        {
            //Arrange
            var now = DateTime.Now;
            var notification = new UserNotification(A<string>(), A<string>(), NotificationType.Advice, A<int>(), A<int>(), now);

            _userNotificationRepository.Setup(x => x.GetById(notification.Id)).Returns(Maybe<UserNotification>.None);

            //Act
            var result = _sut.GetUserNotification(notification.Id);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error);
        }


        private UserNotification ExpectRelatedEntity(RelatedEntityType relatedEntityType, int relatedEntityId, UserNotification notification)
        {
            switch (relatedEntityType)
            {
                case RelatedEntityType.itContract:
                    notification.Itcontract_Id = relatedEntityId;
                    _contractRepository.Setup(x => x.GetById(relatedEntityId)).Returns(new ItContract());
                    break;
                case RelatedEntityType.itProject:
                    notification.ItProject_Id = relatedEntityId;
                    _projectRepository.Setup(x => x.GetById(relatedEntityId)).Returns(new ItProject());
                    break;
                case RelatedEntityType.itSystemUsage:
                    notification.ItSystemUsage_Id = relatedEntityId;
                    _systemUsageRepository.Setup(x => x.GetSystemUsage(relatedEntityId)).Returns(new ItSystemUsage());
                    break;
                case RelatedEntityType.dataProcessingRegistration:
                    notification.DataProcessingRegistration_Id = relatedEntityId;
                    _dataProcessingRepository.Setup(x => x.GetById(relatedEntityId)).Returns(new DataProcessingRegistration());
                    break;
            }
            return notification;
        }
    }
}
