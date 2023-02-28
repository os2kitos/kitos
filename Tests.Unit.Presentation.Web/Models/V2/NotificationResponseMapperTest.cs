using System;
using Core.Abstractions.Extensions;
using Core.ApplicationServices.Model.Notification;
using Core.ApplicationServices.Model.Notification.Read;
using Core.DomainModel;
using Core.DomainModel.Advice;
using Core.DomainModel.Shared;
using Presentation.Web.Controllers.API.V2.Internal.Notifications.Mapping;
using Presentation.Web.Models.API.V2.Internal.Response.Notifications;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    public class NotificationResponseMapperTest : WithAutoFixture
    {
        private readonly NotificationResponseMapper _sut;

        public NotificationResponseMapperTest()
        {
            _sut = new NotificationResponseMapper();
        }

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract)]
        public void MapNotificationResponseDTO_Maps_Properties(RelatedEntityType relatedEntityType)
        {
            //Arrange
            var notification = CreateNotification(relatedEntityType);
            var relationUuid = A<Guid>();

            //Act
            var dto = _sut.MapNotificationResponseDTO(notification);

            //Assert
            Assert.Equal(relationUuid, dto.OwnerResource.Uuid);
            AssertNotificationResponse(notification, dto);
        }

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract)]
        public void MapNotificationResponseDTO_Maps_No_Properties(RelatedEntityType relatedEntityType)
        {
            //Arrange
            var notification = new NotificationResultModel(A<int>(), A<Guid>(), A<bool>(), null, null, null, null, null, null, null, null, relatedEntityType, A<AdviceType>(), null, null);

            //Act
            var dto = _sut.MapNotificationResponseDTO(notification);

            //Assert
            Assert.Null(dto.Name);
            Assert.Null(dto.LastSent);
            Assert.Null(dto.FromDate);
            Assert.Null(dto.ToDate);
            Assert.Null(dto.Subject);
            Assert.Null(dto.Body);
            Assert.Null(dto.CCs);
            Assert.Null(dto.Receivers);
            Assert.Null(dto.RepetitionFrequency);
        }


        [Fact]
        public void MapNotificationAccessRightsResponseDTO_Maps_AccessRights()
        {
            //Arrange
            var accessRights = A<NotificationPermissions>();

            //Act
            var dto = _sut.MapNotificationAccessRightsResponseDTO(accessRights);

            //Assert
            Assert.Equal(accessRights.Read, dto.Read);
            Assert.Equal(accessRights.Deactivate, dto.Deactivate);
            Assert.Equal(accessRights.Delete, dto.Delete);
            Assert.Equal(accessRights.Modify, dto.Modify);

        }

        [Fact]
        public void MapNotificationSentResponseDTO_Maps_AccessRights()
        {
            //Arrange
            var sent = new AdviceSent{AdviceSentDate = A<DateTime>()};

            //Act
            var dto = _sut.MapNotificationSentResponseDTO(sent);

            //Assert
            Assert.Equal(sent.AdviceSentDate, dto.SentDate);

        }

        private static void AssertNotificationResponse(NotificationResultModel notification, NotificationResponseDTO dto)
        {
            Assert.Equal(notification.Uuid, dto.Uuid);
            Assert.Equal(notification.IsActive, dto.Active);
            Assert.Equal(notification.Name, dto.Name);
            Assert.Equal(notification.SentDate, dto.LastSent);
            Assert.Equal(notification.FromDate, dto.FromDate);
            Assert.Equal(notification.ToDate, dto.ToDate);
            Assert.Equal(notification.Subject, dto.Subject);
            Assert.Equal(notification.Body, dto.Body);
            Assert.Equal(notification.RelatedEntityType, dto.OwnerResourceType.ToRelatedEntityType());
            Assert.Equal(notification.NotificationType.ToNotificationType(), dto.NotificationType);

            AssertRecipients(notification, dto);

            if (notification.RepetitionFrequency == Scheduling.Immediate)
            {
                Assert.Null(dto.RepetitionFrequency);
            }
            else
            {
                Assert.Equal(notification.RepetitionFrequency, dto.RepetitionFrequency.GetValueOrDefault().ToScheduling());
            }
        }

        private static void AssertRecipients(NotificationResultModel notification, NotificationResponseDTO dto)
        {
            AssertRecipientsByRoot(notification.Ccs, dto.CCs);
            AssertRecipientsByRoot(notification.Receivers, dto.Receivers);
        }

        private static void AssertRecipientsByRoot(RecipientResultModel relations, RecipientResponseDTO dto)
        {
            foreach (var roleRecipient in dto.RoleRecipients)
            {
                Assert.Single(relations.RoleRecipients,
                    x => x.Role.Uuid == roleRecipient.Role.Uuid);
            }
            foreach (var emailRecipient in dto.EmailRecipients)
            {
                Assert.Single(relations.EmailRecipients,
                    x => x.Email == emailRecipient.Email);
            }
        }

        private NotificationResultModel CreateNotification(RelatedEntityType relatedEntityType)
        {
            return new NotificationResultModel
            (
                A<int>(),
                A<Guid>(),
                A<bool>(),
                A<string>(),
                A<DateTime>(),
                A<DateTime>(),
                A<DateTime>(),
                A<string>(),
                A<string>(),
                A<Scheduling>(),
                A<IEntityWithAdvices>(),
                relatedEntityType,
                A<AdviceType>(),
                CreateRecipients(),
                CreateRecipients()
            );
        }

        private RecipientResultModel CreateRecipients()
        {
            return new RecipientResultModel(
                CreateEmailRecipient().WrapAsEnumerable(),
                CreateRoleRecipient().WrapAsEnumerable()
            );
        }

        private EmailRecipientResultModel CreateEmailRecipient()
        {
            return new EmailRecipientResultModel(CreateEmail());
        }

        private RoleRecipientResultModel CreateRoleRecipient()
        {
            return new RoleRecipientResultModel(A<IRoleEntity>());
        }

        private string CreateEmail()
        {
            return $"{nameof(NotificationResponseMapperTest)}_{A<Guid>()}@kitos.dk";
        }
    }
}
