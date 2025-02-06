using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Notification;
using Core.DomainModel.Notification;
using Core.DomainModel.Shared;
using Core.DomainServices.Notifications;

using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.DomainServices.Generic;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.Notification
{
    public class UserNotificationApplicationServiceTest : WithAutoFixture
    {
        private readonly UserNotificationApplicationService _sut;
        private readonly Mock<IUserNotificationService> _userNotificationService;
        private readonly Mock<IOrganizationalUserContext> _activeUserContext;
        private readonly Mock<IEntityIdentityResolver> _entityIdentityResolver;

        public UserNotificationApplicationServiceTest()
        {
            _userNotificationService = new Mock<IUserNotificationService>();
            _activeUserContext = new Mock<IOrganizationalUserContext>();
            _entityIdentityResolver = new Mock<IEntityIdentityResolver>();
            _sut = new UserNotificationApplicationService(_userNotificationService.Object, _activeUserContext.Object, _entityIdentityResolver.Object);
        }

        [Fact]
        public void Can_Delete()
        {
            //Arrange
            var notificationId = A<int>();
            var notification = new UserNotification
            {
                Id = notificationId,
                NotificationRecipientId = A<int>()
            };

            _userNotificationService.Setup(x => x.GetUserNotification(notificationId)).Returns(notification);
            _userNotificationService.Setup(x => x.Delete(notificationId)).Returns(Maybe<OperationError>.None);
            _activeUserContext.Setup(x => x.UserId).Returns(notification.NotificationRecipientId);

            //Act

            var result = _sut.Delete(notificationId);

            //Assert
            Assert.True(result.IsNone);
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
            _userNotificationService.Setup(x => x.Delete(notificationId)).Returns(Maybe<OperationError>.Some(new OperationError(OperationFailure.NotFound)));
            _activeUserContext.Setup(x => x.UserId).Returns(notification.NotificationRecipientId);

            //Act
            var result = _sut.Delete(notification.Id);

            //Assert
            Assert.True(result.HasValue);
            Assert.Equal(OperationFailure.NotFound, result.Value.FailureType);
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
            _userNotificationService.Setup(x => x.Delete(notificationId)).Returns(Maybe<OperationError>.None);
            _activeUserContext.Setup(x => x.UserId).Returns(A<int>());

            //Act
            var result = _sut.Delete(notification.Id);

            //Assert
            Assert.True(result.HasValue);
            Assert.Equal(OperationFailure.Forbidden, result.Value.FailureType);
        }

        [Fact]
        public void Can_GetNotificationsForUser()
        {
            //Arrange
            var numberOfNotifications = Math.Abs(A<int>());
            var userId = A<int>();
            var orgId = A<int>();
            var relatedEntityType = A<RelatedEntityType>();

            SetupUserNotificationService(numberOfNotifications, orgId, userId, relatedEntityType);

            _activeUserContext.Setup(x => x.UserId).Returns(userId);

            //Act
            var result = _sut.GetNotificationsForUser(orgId, userId, relatedEntityType);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(numberOfNotifications, result.Value.ToList().Count());
        }

        [Fact]
        public void Can_Not_GetNotificationsForUser_When_Using_Other_UserId()
        {
            //Arrange
            var numberOfNotifications = Math.Abs(A<int>());
            var userId = A<int>();
            var orgId = A<int>();
            var relatedEntityType = A<RelatedEntityType>();

            SetupUserNotificationService(numberOfNotifications, orgId, userId, relatedEntityType);
            _activeUserContext.Setup(x => x.UserId).Returns(A<int>());

            //Act
            var result = _sut.GetNotificationsForUser(orgId, userId, relatedEntityType);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
        }

        private void SetupUserNotificationService(int numberOfNotifications, int orgId, int userId, RelatedEntityType relatedEntityType)
        {
            var notificationsList = new List<UserNotification>();
            for (int i = 0; i < numberOfNotifications; i++)
            {
                notificationsList.Add(new UserNotification() { Id = A<int>() });
            }
            _userNotificationService.Setup(x => x.GetNotificationsForUser(orgId, userId, relatedEntityType)).Returns(Result<IQueryable<UserNotification>, OperationError>.Success(notificationsList.AsQueryable()));
        }
    }
}
