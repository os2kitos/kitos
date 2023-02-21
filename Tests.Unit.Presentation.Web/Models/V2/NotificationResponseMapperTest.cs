using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Notification;
using Core.DomainModel.Advice;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Shared;
using Core.DomainServices.Generic;
using Moq;
using Presentation.Web.Controllers.API.V2.Internal.Notifications.Mapping;
using Presentation.Web.Models.API.V2.Internal.Response.Notifications;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    public class NotificationResponseMapperTest : WithAutoFixture
    {
        private readonly Mock<IEntityIdentityResolver> _identityResolver;

        private readonly NotificationResponseMapper _sut;

        public NotificationResponseMapperTest()
        {
            _identityResolver = new Mock<IEntityIdentityResolver>();

            _sut = new NotificationResponseMapper(_identityResolver.Object);
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

            ExpectResolveRelationIdReturns(notification.RelationId.GetValueOrDefault(), relatedEntityType, relationUuid);

            //Act
            var result = _sut.MapNotificationResponseDTO(notification);

            //Assert
            Assert.True(result.Ok);
            var dto = result.Value;
            Assert.Equal(relationUuid, dto.OwnerResourceUuid);
            AssertNotificationResponse(notification, dto);
        }

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract)]
        public void MapNotificationResponseDTO_Maps_No_Properties(RelatedEntityType relatedEntityType)
        {
            //Arrange
            var notification = new Advice() {RelationId = A<int>(), Type = relatedEntityType};
            var relationUuid = A<Guid>();

            ExpectResolveRelationIdReturns(notification.RelationId.GetValueOrDefault(), relatedEntityType, relationUuid);

            //Act
            var result = _sut.MapNotificationResponseDTO(notification);

            //Assert
            Assert.True(result.Ok);
            var dto = result.Value;
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

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract)]
        public void MapNotificationResponseDTO_Returns_NotFound_When_RelatedUuid_NotFound(RelatedEntityType relatedEntityType)
        {
            //Arrange
            var notification = new Advice() {RelationId = A<int>(), Type = relatedEntityType};

            ExpectResolveRelationIdReturns(notification.RelationId.GetValueOrDefault(), relatedEntityType, Maybe<Guid>.None);

            //Act
            var result = _sut.MapNotificationResponseDTO(notification);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract)]
        public void MapNotificationResponseDTOs_Returns_NotFound_When_RelatedUuid_NotFound(RelatedEntityType relatedEntityType)
        {
            //Arrange
            var notifications = new List<Advice>{ CreateNotification(relatedEntityType), CreateNotification(relatedEntityType)};

            foreach (var notification in notifications)
            {
                ExpectResolveRelationIdReturns(notification.RelationId.GetValueOrDefault(), relatedEntityType, Maybe<Guid>.None);
            }

            //Act
            var response = _sut.MapNotificationResponseDTOs(notifications);

            //Assert
            Assert.True(response.Failed);
            Assert.Equal(OperationFailure.NotFound, response.Error.FailureType);
        }


        [Fact]
        public void MapNotificationAccessRightsResponseDTO_Maps_AccessRights()
        {
            //Arrange
            var accessRights = A<NotificationAccessRights>();

            //Act
            var dto = _sut.MapNotificationAccessRightsResponseDTO(accessRights);

            //Assert
            Assert.Equal(accessRights.CanBeDeactivated, dto.CanBeDeactivated);
            Assert.Equal(accessRights.CanBeDeleted, dto.CanBeDeleted);
            Assert.Equal(accessRights.CanBeModified, dto.CanBeModified);

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

        private static void AssertNotificationResponse(Advice notification, NotificationResponseDTO dto)
        {
            Assert.Equal(notification.Uuid, dto.Uuid);
            Assert.Equal(notification.IsActive, dto.Active);
            Assert.Equal(notification.Name, dto.Name);
            Assert.Equal(notification.SentDate, dto.LastSent);
            Assert.Equal(notification.AlarmDate, dto.FromDate);
            Assert.Equal(notification.StopDate, dto.ToDate);
            Assert.Equal(notification.Subject, dto.Subject);
            Assert.Equal(notification.Body, dto.Body);
            Assert.Equal(notification.Type, dto.Type.ToRelatedEntityType());
            Assert.Equal(notification.AdviceType.ToNotificationType(), dto.NotificationType);

            AssertRecipients(notification, dto);

            if (notification.Scheduling == Scheduling.Immediate)
            {
                Assert.Null(dto.RepetitionFrequency);
            }
            else
            {
                Assert.Equal(notification.Scheduling, dto.RepetitionFrequency.GetValueOrDefault().ToScheduling());
            }
        }

        private static void AssertRecipients(Advice notification, NotificationResponseDTO dto)
        {
            AssertRecipientsByRoot(notification.Reciepients, dto.CCs);
            AssertRecipientsByRoot(notification.Reciepients, dto.Receivers);
        }

        private static void AssertRecipientsByRoot(IEnumerable<AdviceUserRelation> relations,
            RecipientResponseDTO dto)
        {
            var adviceUserRelations = relations.ToList();
            foreach (var roleRecipient in dto.RoleRecipients)
            {
                Assert.Single(adviceUserRelations,
                    x => x.DataProcessingRegistrationRole?.Uuid == roleRecipient.Role.Uuid ||
                         x.ItContractRole?.Uuid == roleRecipient.Role.Uuid ||
                         x.ItSystemRole?.Uuid == roleRecipient.Role.Uuid);
            }
            foreach (var emailRecipient in dto.EmailRecipients)
            {
                Assert.Single(adviceUserRelations,
                    x => x.Email == emailRecipient.Email);
            }
        }

        private void ExpectResolveRelationIdReturns(int relationId, RelatedEntityType relatedEntityType, Maybe<Guid> result)
        {
            switch (relatedEntityType)
            {
                case RelatedEntityType.dataProcessingRegistration:
                    _identityResolver.Setup(x => x.ResolveUuid<DataProcessingRegistration>(relationId)).Returns(result);
                    break;
                case RelatedEntityType.itSystemUsage:
                    _identityResolver.Setup(x => x.ResolveUuid<ItSystemUsage>(relationId)).Returns(result);
                    break;
                case RelatedEntityType.itContract:
                    _identityResolver.Setup(x => x.ResolveUuid<ItContract>(relationId)).Returns(result);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(relatedEntityType), relatedEntityType, null);
            }
        }

        private Advice CreateNotification(RelatedEntityType relatedEntityType)
        {
            return new Advice
            {
                Uuid = A<Guid>(),
                RelationId = A<int>(),
                IsActive = A<bool>(),
                Name = A<string>(),
                SentDate = A<DateTime>(),
                AlarmDate = A<DateTime>(),
                StopDate = A<DateTime>(),
                Subject = A<string>(),
                Body = A<string>(),
                Scheduling = A<Scheduling>(),
                Type = relatedEntityType,
                AdviceType = A<AdviceType>(),
                Reciepients = CreateRecipients(relatedEntityType)
            };
        }

        private ICollection<AdviceUserRelation> CreateRecipients(RelatedEntityType relatedEntityType)
        {
            return new List<AdviceUserRelation>
            {
                CreateEmailRecipient(RecieverType.CC),
                CreateEmailRecipient(RecieverType.RECIEVER),
                CreateRoleRecipient(RecieverType.CC, relatedEntityType),
                CreateRoleRecipient(RecieverType.RECIEVER, relatedEntityType)
            };
        }

        private AdviceUserRelation CreateEmailRecipient(RecieverType receiverType)
        {
            return new AdviceUserRelation
            {
                RecieverType = receiverType,
                RecpientType = RecipientType.USER,
                Email = CreateEmail()
            };
        }

        private AdviceUserRelation CreateRoleRecipient(RecieverType receiverType, RelatedEntityType relatedEntityType)
        {
            return new AdviceUserRelation
            {
                RecieverType = receiverType,
                RecpientType = RecipientType.ROLE,
                ItContractRole = relatedEntityType == RelatedEntityType.itContract 
                    ? new ItContractRole {Uuid = A<Guid>(), Name = A<string>()}
                    : null,
                DataProcessingRegistrationRole = relatedEntityType == RelatedEntityType.dataProcessingRegistration
                    ? new DataProcessingRegistrationRole {Uuid = A<Guid>(), Name = A<string>()}
                    : null,
                ItSystemRole = relatedEntityType == RelatedEntityType.itSystemUsage
                    ? new ItSystemRole {Uuid = A<Guid>(), Name = A<string>()}
                    : null
            };
        }

        private string CreateEmail()
        {
            return $"{nameof(NotificationResponseMapperTest)}_{A<Guid>()}@kitos.dk";
        }
    }
}
