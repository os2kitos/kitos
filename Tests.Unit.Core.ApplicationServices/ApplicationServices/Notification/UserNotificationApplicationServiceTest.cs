using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Notification;
using Core.DomainModel.Notification;
using Core.DomainModel.Result;
using Core.DomainModel.Shared;
using Core.DomainServices.Notifications;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.Notification
{
    public class UserNotificationApplicationServiceTest : WithAutoFixture
    {
        private readonly UserNotificationApplicationService _sut;
        private readonly Mock<IUserNotificationService> _userNotificationService;
        private readonly Mock<IOrganizationalUserContext> _activeUserContext;

        public UserNotificationApplicationServiceTest()
        {
            _userNotificationService = new Mock<IUserNotificationService>();
            _activeUserContext = new Mock<IOrganizationalUserContext>();
            _sut = new UserNotificationApplicationService(_userNotificationService.Object, _activeUserContext.Object);
        }

        [Fact]
        public void Can_Delete()
        {
            //Arrange
            var notificationId = A<int>();
            var notification = new UserNotification {
                Id = notificationId,
                NotificationRecipientId = A<int>()
            };

            _userNotificationService.Setup(x => x.GetUserNotification(notificationId)).Returns(notification);
            _userNotificationService.Setup(x => x.Delete(notificationId)).Returns(true);
            _activeUserContext.Setup(x => x.UserId).Returns(notification.NotificationRecipientId);

            //Act
            
            var result = _sut.Delete(notificationId);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(notification, result.Value);
        }

        [Fact]
        public void Can_Not_Delete_If_Notification_Not_Found()
        {
            //Arrange
            var notificationId = A<int>();
            var notification = new UserNotification
            {
                Id = notificationId,
                NotificationRecipientId = A<int>()
            };

            _userNotificationService.Setup(x => x.GetUserNotification(notificationId)).Returns(Result<UserNotification, OperationError>.Failure(OperationFailure.NotFound));
            _userNotificationService.Setup(x => x.Delete(notificationId)).Returns(true);
            _activeUserContext.Setup(x => x.UserId).Returns(notification.NotificationRecipientId);

            //Act
            var result = _sut.Delete(notification.Id);

            //Assert
            Assert.True(result.Failed);
        }

        [Fact]
        public void Can_Not_Delete_If_Not_Recipient()
        {
            //Arrange
            var notificationId = A<int>();
            var notification = new UserNotification
            {
                Id = notificationId,
                NotificationRecipientId = A<int>()
            };

            _userNotificationService.Setup(x => x.GetUserNotification(notificationId)).Returns(notification);
            _userNotificationService.Setup(x => x.Delete(notificationId)).Returns(true);
            _activeUserContext.Setup(x => x.UserId).Returns(A<int>());

            //Act
            var result = _sut.Delete(notification.Id);

            //Assert
            Assert.True(result.Failed);
        }

        [Fact]
        public void Can_GetNotificationsForUser()
        {
            //Arrange
            var userId = A<int>();
            var orgId = A<int>();
            var relatedEntityType = A<RelatedEntityType>();

            var notification1 = new UserNotification
            {
                Id = A<int>()
            };
            var notification2 = new UserNotification
            {
                Id = A<int>()
            };

            var notificationsList = new List<UserNotification>()
            {
                notification1,
                notification2
            };

            _userNotificationService.Setup(x => x.GetNotificationsForUser(orgId, userId, relatedEntityType)).Returns(notificationsList);
            _activeUserContext.Setup(x => x.UserId).Returns(userId);

            //Act
            var result = _sut.GetNotificationsForUser(orgId, userId, relatedEntityType);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(2, result.Value.ToList().Count());
        }

        [Fact]
        public void Can_Not_GetNotificationsForUser_When_Using_Other_UserId()
        {
            //Arrange
            var userId = A<int>();
            var orgId = A<int>();
            var relatedEntityType = A<RelatedEntityType>();

            var notification1 = new UserNotification
            {
                Id = A<int>()
            };
            var notification2 = new UserNotification
            {
                Id = A<int>()
            };

            var notificationsList = new List<UserNotification>()
            {
                notification1,
                notification2
            };

            _userNotificationService.Setup(x => x.GetNotificationsForUser(orgId, userId, relatedEntityType)).Returns(notificationsList);
            _activeUserContext.Setup(x => x.UserId).Returns(A<int>());

            //Act
            var result = _sut.GetNotificationsForUser(orgId, userId, relatedEntityType);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
        }

        [Fact]
        public void Can_GetNumberOfUnresolvedNotificationsForUser()
        {
            //Arrange
            var userId = A<int>();
            var orgId = A<int>();
            var relatedEntityType = A<RelatedEntityType>();

            var listLength = A<int>();

            _userNotificationService.Setup(x => x.GetNumberOfUnresolvedNotificationsForUser(orgId, userId, relatedEntityType)).Returns(listLength);
            _activeUserContext.Setup(x => x.UserId).Returns(userId);

            //Act
            var result = _sut.GetNumberOfUnresolvedNotificationsForUser(orgId, userId, relatedEntityType);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(listLength, result.Value);
        }

        [Fact]
        public void Can_Not_GetNumberOfUnresolvedNotificationsForUser_When_Using_Other_UserId()
        {
            //Arrange
            var userId = A<int>();
            var orgId = A<int>();
            var relatedEntityType = A<RelatedEntityType>();

            var listLength = A<int>();

            _userNotificationService.Setup(x => x.GetNumberOfUnresolvedNotificationsForUser(orgId, userId, relatedEntityType)).Returns(listLength);
            _activeUserContext.Setup(x => x.UserId).Returns(A<int>());

            //Act
            var result = _sut.GetNumberOfUnresolvedNotificationsForUser(orgId, userId, relatedEntityType);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
        }
    }
}
