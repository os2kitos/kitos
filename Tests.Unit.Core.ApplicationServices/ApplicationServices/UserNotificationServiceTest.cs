using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Notification;
using Core.DomainModel.Notification;
using Core.DomainModel.Result;
using Core.DomainModel.Shared;
using Core.DomainServices.Repositories.Notification;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.Types;
using Moq;
using System.Data;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices
{
    public class UserNotificationServiceTest : WithAutoFixture
    {
        private readonly UserNotificationService _sut;
        private readonly Mock<IUserNotificationRepository> _userNotificationRepository;
        private readonly Mock<ITransactionManager> _transactionManager;
        private readonly Mock<IAuthorizationContext> _authorizationContext;

        public UserNotificationServiceTest()
        {
            _userNotificationRepository = new Mock<IUserNotificationRepository>();
            _transactionManager = new Mock<ITransactionManager>();
            _authorizationContext = new Mock<IAuthorizationContext>();
            _sut = new UserNotificationService(_userNotificationRepository.Object, _transactionManager.Object, _authorizationContext.Object);
        }

        [Fact]
        public void Can_Create()
        {
            //Arrange
            var notification = new UserNotification
            {
                Name = A<string>(),
                OrganizationId = A<int>(),
                NotificationMessage = A<string>(),
                RelatedEntityId = A<int>(),
                RelatedEntityType = A<RelatedEntityType>(),
                ObjectOwnerId = A<int>(),
                NotificationType = A<NotificationType>()
            };

            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin(IsolationLevel.Serializable)).Returns(transaction.Object);
            _userNotificationRepository.Setup(x => x.Add(notification)).Returns(notification);

            //Act
            var result = _sut.AddUserNotification(notification.ObjectOwnerId.Value, notification.Name, notification.NotificationMessage, notification.RelatedEntityId, notification.RelatedEntityType, notification.NotificationType);

            //Assert
            Assert.True(result.Ok);
            transaction.Verify(x => x.Commit());
        }

        [Fact]
        public void Can_Delete()
        {
            //Arrange
            var notification = new UserNotification
            {
                Id = A<int>()
            };

            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin(IsolationLevel.Serializable)).Returns(transaction.Object);
            _userNotificationRepository.Setup(x => x.GetById(notification.Id)).Returns(notification);
            _authorizationContext.Setup(x => x.AllowDelete(notification)).Returns(true);

            //Act
            var result = _sut.Delete(notification.Id);

            //Assert
            Assert.True(result.Ok);
            transaction.Verify(x => x.Commit());
        }

        [Fact]
        public void Can_Not_Delete_If_No_Rights()
        {
            //Arrange
            var notification = new UserNotification
            {
                Id = A<int>()
            };

            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin(IsolationLevel.Serializable)).Returns(transaction.Object);
            _userNotificationRepository.Setup(x => x.GetById(notification.Id)).Returns(notification);
            _authorizationContext.Setup(x => x.AllowDelete(notification)).Returns(false);

            //Act
            var result = _sut.Delete(notification.Id);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
            transaction.Verify(x => x.Rollback());
        }

        [Fact]
        public void Can_Not_Delete_If_Notification_Not_Found()
        {
            //Arrange
            var notification = new UserNotification
            {
                Id = A<int>()
            };

            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin(IsolationLevel.Serializable)).Returns(transaction.Object);
            _userNotificationRepository.Setup(x => x.GetById(notification.Id)).Returns(Maybe<UserNotification>.None);
            _authorizationContext.Setup(x => x.AllowDelete(notification)).Returns(true);

            //Act
            var result = _sut.Delete(notification.Id);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error);
            transaction.Verify(x => x.Rollback());
        }
    }
}
