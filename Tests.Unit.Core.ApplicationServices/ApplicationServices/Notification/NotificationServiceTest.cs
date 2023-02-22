using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Notification;
using Core.ApplicationServices.Model.Notification.Write;
using Core.ApplicationServices.Notification;
using Core.DomainModel;
using Core.DomainModel.Advice;
using Core.DomainModel.Events;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Generic;
using Core.DomainServices;
using Infrastructure.Services.DataAccess;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainModel.Shared;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;
using Core.DomainServices.Queries.Notifications;

namespace Tests.Unit.Core.ApplicationServices.Notification
{
    public class NotificationServiceTest : WithAutoFixture
    {
        private readonly Mock<IEntityIdentityResolver> _entityIdentityResolver;
        private readonly Mock<IRegistrationNotificationService> _registrationNotificationService;
        private readonly Mock<IRegistrationNotificationUserRelationsService> _notificationUserRelationsService;
        private readonly Mock<IAuthorizationContext> _authorizationContext;
        private readonly Mock<ITransactionManager> _transactionManager;
        private readonly Mock<IGenericRepository<ItSystemUsage>> _usageRepository;
        private readonly Mock<IGenericRepository<ItContract>> _contractRepository;
        private readonly Mock<IGenericRepository<DataProcessingRegistration>> _dprRepository;
        private readonly Mock<IDomainEvents> _domainEvents;

        private readonly NotificationService _sut;

        public NotificationServiceTest()
        {
            _entityIdentityResolver = new Mock<IEntityIdentityResolver>();
            _registrationNotificationService = new Mock<IRegistrationNotificationService>();
            _notificationUserRelationsService = new Mock<IRegistrationNotificationUserRelationsService>();
            _authorizationContext = new Mock<IAuthorizationContext>();
            _transactionManager = new Mock<ITransactionManager>();
            _usageRepository = new Mock<IGenericRepository<ItSystemUsage>>();
            _contractRepository = new Mock<IGenericRepository<ItContract>>();
            _dprRepository = new Mock<IGenericRepository<DataProcessingRegistration>>();
            _domainEvents = new Mock<IDomainEvents>();

            _sut = new NotificationService(_entityIdentityResolver.Object,
                _registrationNotificationService.Object,
                _authorizationContext.Object,
                _notificationUserRelationsService.Object,
                _transactionManager.Object,
                _usageRepository.Object,
                _contractRepository.Object,
                _dprRepository.Object,
                _domainEvents.Object);
        }

        [Fact]
        public void Can_GetNotifications()
        {
            //Arrange
            var orgUuid = A<Guid>();
            var orgId = A<int>();
            var notifications = new List<Advice> {new(), new()}.AsQueryable();

            ExpectResolveIdReturns<Organization>(orgUuid, orgId);
            ExpectGetNotificationsByOrganizationIdReturns(orgId, Result<IQueryable<Advice>, OperationError>.Success(notifications));

            //Act
            var result = _sut.GetNotifications(orgUuid);

            //Arrange
            Assert.True(result.Ok);
            var resultList = result.Value.ToList();
            Assert.Equal(notifications.Count(), resultList.Count);
        }

        [Fact]
        public void Can_GetNotifications_With_Filter()
        {
            //Arrange
            var orgUuid = A<Guid>();
            var orgId = A<int>();
            var fromDate = A<DateTime>();
            var notification = new Advice() {AlarmDate = fromDate};
            var notifications = new List<Advice> { notification, new()}.AsQueryable();
            var condition = new QueryBySinceNotificationFromDate(fromDate);

            ExpectResolveIdReturns<Organization>(orgUuid, orgId);
            ExpectGetNotificationsByOrganizationIdReturns(orgId, Result<IQueryable<Advice>, OperationError>.Success(notifications));

            //Act
            var result = _sut.GetNotifications(orgUuid, condition);

            //Arrange
            Assert.True(result.Ok);
            var resultList = result.Value.ToList();
            var resultNotification = Assert.Single(resultList);
            Assert.Equal(notification.AlarmDate, resultNotification.AlarmDate);
        }

        [Fact]
        public void GetNotifications_Returns_OperationError_When_Failed_To_GetNotifications()
        {
            //Arrange
            var orgUuid = A<Guid>();
            var orgId = A<int>();
            var error = A<OperationError>();

            ExpectResolveIdReturns<Organization>(orgUuid, orgId);
            ExpectGetNotificationsByOrganizationIdReturns(orgId, error);

            //Act
            var result = _sut.GetNotifications(orgUuid);

            //Arrange
            Assert.True(result.Failed);
            Assert.Equal(error.FailureType, result.Error.FailureType);
        }

        [Fact]
        public void GetNotifications_Returns_NotFound_When_Failed_To_Find_OrgId()
        {
            //Arrange
            var orgUuid = A<Guid>();

            ExpectResolveIdReturns<Organization>(orgUuid, Maybe<int>.None);

            //Act
            var result = _sut.GetNotifications(orgUuid);

            //Arrange
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract)]
        public void Can_GetNotificationByUuid(RelatedEntityType relatedEntityType)
        {
            //Arrange
            var uuid = A<Guid>();
            var id = A<int>();
            var notification = new Advice {Id = id, Type = relatedEntityType};

            ExpectResolveIdReturns<Advice>(uuid, id);
            ExpectGetNotificationByIdReturns(id, notification);

            //Act
            var result = _sut.GetNotificationByUuid(uuid, relatedEntityType);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(notification.Id, result.Value.Id);
        }

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage, RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.dataProcessingRegistration, RelatedEntityType.itContract)]
        [InlineData(RelatedEntityType.itContract, RelatedEntityType.itSystemUsage)]
        public void GetNotificationByUuid_Returns_BadInput_When_Type_Differs(RelatedEntityType relatedEntityType, RelatedEntityType actualType)
        {
            //Arrange
            var uuid = A<Guid>();
            var id = A<int>();
            var notification = new Advice {Id = id, Type = actualType };

            ExpectResolveIdReturns<Advice>(uuid, id);
            ExpectGetNotificationByIdReturns(id, notification);

            //Act
            var result = _sut.GetNotificationByUuid(uuid, relatedEntityType);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void GetNotificationByUuid_Returns_NotFound_When_Notification_Was_Not_Found()
        {
            //Arrange
            var uuid = A<Guid>();
            var id = A<int>();

            ExpectResolveIdReturns<Advice>(uuid, id);
            ExpectGetNotificationByIdReturns(id, Maybe<Advice>.None);

            //Act
            var result = _sut.GetNotificationByUuid(uuid, A<RelatedEntityType>());

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Fact]
        public void GetNotificationByUuid_Returns_NotFound_When_NotificationId_Was_Not_Found()
        {
            //Arrange
            var uuid = A<Guid>();

            ExpectResolveIdReturns<Advice>(uuid, Maybe<int>.None);

            //Act
            var result = _sut.GetNotificationByUuid(uuid, A<RelatedEntityType>());

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract)]
        public void Can_GetSentNotifications(RelatedEntityType relatedEntityType)
        {
            //Arrange
            var uuid = A<Guid>();
            var notification = new Advice { Uuid = uuid, Type = relatedEntityType };
            var notificationSentIQueryable = new List<AdviceSent> { new() { Advice = notification }, new() { Advice = new Advice() } }.AsQueryable();

            ExpectGetSentReturns(notificationSentIQueryable);

            //Act
            var result = _sut.GetNotificationSentByUuid(uuid, relatedEntityType);

            //Assert
            var notificationSent = Assert.Single(result);
            Assert.Equal(notification.Uuid, notificationSent.Advice.Uuid);
            Assert.Equal(notification.Type, notificationSent.Advice.Type);
        }

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract)]
        public void Can_CreateImmediateNotification(RelatedEntityType relatedEntityType)
        {
            //Arrange
            var parameters = CreateNewParameters(relatedEntityType);
            var notification = new Advice {Uuid = parameters.OwnerResourceUuid};

            var transaction = ExpectDatabaseTransactionReturns();
            var relatedEntity = ExpectGetRelatedEntityReturnsEntity(parameters.OwnerResourceUuid, parameters.Type);
            var roleIds = ExpectRoleRecipientIds(parameters);
            ExpectAllowModifyReturns(relatedEntity, true);
            ExpectCreateReturns(parameters, AdviceType.Immediate, roleIds, notification);

            //Act
            var result = _sut.CreateImmediateNotification(parameters);

            //Assert
            Assert.True(result.Ok);
            transaction.Verify(x => x.Commit(), Times.Once);
            VerifyDatabaseSave(parameters.Type);
        }

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract)]
        public void CreateImmediateNotification_Returns_Forbidden_When_Not_Allowed_To_Modify(RelatedEntityType relatedEntityType)
        {
            var parameters = CreateNewParameters(relatedEntityType);
            Test_Command_Which_Updates_Notification_Returns_Failure_Forbidden(parameters.OwnerResourceUuid, relatedEntityType, _ => _sut.CreateImmediateNotification(parameters));
        }

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract)]
        public void CreateImmediateNotification_Returns_Forbidden_When_RelatedEntity_Not_Found(RelatedEntityType relatedEntityType)
        {
            var parameters = CreateNewParameters(relatedEntityType);
            Test_Command_Which_Updates_Notification_Returns_Failure_NotFound(relatedEntityType, _ => _sut.CreateImmediateNotification(parameters));
        }

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract)]
        public void Can_CreateScheduledNotification(RelatedEntityType relatedEntityType)
        {
            //Arrange
            var parameters = CreateNewScheduledParameters(relatedEntityType);
            var notification = new Advice {Uuid = parameters.OwnerResourceUuid};

            var transaction = ExpectDatabaseTransactionReturns();
            var relatedEntity = ExpectGetRelatedEntityReturnsEntity(parameters.OwnerResourceUuid, parameters.Type);
            var roleIds = ExpectRoleRecipientIds(parameters);
            ExpectAllowModifyReturns(relatedEntity, true);
            ExpectCreateReturns(parameters, AdviceType.Repeat, roleIds, notification);

            //Act
            var result = _sut.CreateScheduledNotification(parameters);

            //Assert
            Assert.True(result.Ok);
            transaction.Verify(x => x.Commit(), Times.Once);
            VerifyDatabaseSave(parameters.Type);
        }

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract)]
        public void CreateScheduledNotification_Returns_Forbidden_When_Not_Allowed_To_Modify(RelatedEntityType relatedEntityType)
        {
            var parameters = CreateNewScheduledParameters(relatedEntityType);
            Test_Command_Which_Updates_Notification_Returns_Failure_Forbidden(parameters.OwnerResourceUuid, relatedEntityType, _ => _sut.CreateScheduledNotification(parameters));
        }

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract)]
        public void CreateScheduledNotification_Returns_Forbidden_When_RelatedEntity_Not_Found(RelatedEntityType relatedEntityType)
        {
            var parameters = CreateNewScheduledParameters(relatedEntityType);
            Test_Command_Which_Updates_Notification_Returns_Failure_NotFound(relatedEntityType, _ => _sut.CreateScheduledNotification(parameters));
        }

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract)]
        public void Can_UpdateScheduledNotification(RelatedEntityType relatedEntityType)
        {
            //Arrange
            var uuid = A<Guid>();
            var id = A<int>();
            var parameters = CreateNewUpdateScheduledParameters(relatedEntityType);
            var notification = new Advice { Uuid = parameters.OwnerResourceUuid };

            var transaction = ExpectDatabaseTransactionReturns();
            var relatedEntity = ExpectGetRelatedEntityReturnsEntity(parameters.OwnerResourceUuid, parameters.Type);
            var resolvedRoleIds = ExpectRoleRecipientIds(parameters);
            ExpectResolveIdReturns<Advice>(uuid, id);
            ExpectAllowModifyReturns(relatedEntity, true);
            ExpectUpdateNotificationUserRelations(id, parameters, resolvedRoleIds, Maybe<OperationError>.None);
            ExpectUpdateReturns(id, parameters, notification);

            //Act
            var result = _sut.UpdateScheduledNotification(uuid, parameters);

            //Assert
            Assert.True(result.Ok);
            transaction.Verify(x => x.Commit(), Times.Once);
            VerifyDatabaseSave(parameters.Type);
        }

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract)]
        public void UpdateScheduledNotification_Returns_NotFound_If_NotificationId_Was_Not_Found(RelatedEntityType relatedEntityType)
        {
            //Arrange
            var uuid = A<Guid>();
            var parameters = CreateNewUpdateScheduledParameters(relatedEntityType);

            ExpectDatabaseTransactionReturns();
            var relatedEntity = ExpectGetRelatedEntityReturnsEntity(parameters.OwnerResourceUuid, parameters.Type);
            ExpectRoleRecipientIds(parameters);
            ExpectAllowModifyReturns(relatedEntity, true);
            ExpectResolveIdReturns<Advice>(uuid, Maybe<int>.None);

            //Act
            var result = _sut.UpdateScheduledNotification(uuid, parameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, OperationFailure.NotFound);
        }

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract)]
        public void UpdateScheduledNotification_Returns_Error_If_Update_Relations_Fails(RelatedEntityType relatedEntityType)
        {
            //Arrange
            var uuid = A<Guid>();
            var id = A<int>();
            var parameters = CreateNewUpdateScheduledParameters(relatedEntityType);
            var error = A<OperationError>();

            ExpectDatabaseTransactionReturns();
            var relatedEntity = ExpectGetRelatedEntityReturnsEntity(parameters.OwnerResourceUuid, parameters.Type);
            var resolvedRoleIds = ExpectRoleRecipientIds(parameters);
            ExpectResolveIdReturns<Advice>(uuid, id);
            ExpectAllowModifyReturns(relatedEntity, true);
            ExpectUpdateNotificationUserRelations(id, parameters, resolvedRoleIds, error);

            //Act
            var result = _sut.UpdateScheduledNotification(uuid, parameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(error.FailureType, result.Error.FailureType);
        }

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract)]
        public void UpdateScheduledNotification_Returns_Forbidden_When_Not_Allowed_To_Modify(RelatedEntityType relatedEntityType)
        {
            var parameters = CreateNewScheduledParameters(relatedEntityType);
            Test_Command_Which_Updates_Notification_Returns_Failure_Forbidden(parameters.OwnerResourceUuid, relatedEntityType, _ => _sut.UpdateScheduledNotification(A<Guid>(), parameters));
        }

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract)]
        public void UpdateScheduledNotification_Returns_Forbidden_When_RelatedEntity_Not_Found(RelatedEntityType relatedEntityType)
        {
            var parameters = CreateNewScheduledParameters(relatedEntityType);
            Test_Command_Which_Updates_Notification_Returns_Failure_NotFound(relatedEntityType, _ => _sut.UpdateScheduledNotification(A<Guid>(), parameters));
        }

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract)]
        public void Can_DeactivateNotification(RelatedEntityType relatedEntityType)
        {
            //Arrange
            var uuid = A<Guid>();
            var id = A<int>();
            var notification = new Advice{Id = id, Type = relatedEntityType};

            ExpectResolveIdReturns<Advice>(uuid, id);
            ExpectGetNotificationByIdReturns(id, notification);
            ExpectAllowModifyReturns(notification, true);
            ExpectDeactivateReturns(id, notification);

            //Act
            var result = _sut.DeactivateNotification(uuid, relatedEntityType);

            //Assert
            Assert.True(result.Ok);
        }

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract)]
        public void DeactivateNotification_Returns_Error_When_Deactivate_Fails(RelatedEntityType relatedEntityType)
        {
            //Arrange
            var uuid = A<Guid>();
            var id = A<int>();
            var notification = new Advice{Id = id, Type = relatedEntityType};
            var error = A<OperationError>();

            ExpectResolveIdReturns<Advice>(uuid, id);
            ExpectGetNotificationByIdReturns(id, notification);
            ExpectAllowModifyReturns(notification, true);
            ExpectDeactivateReturns(id, error);

            //Act
            var result = _sut.DeactivateNotification(uuid, relatedEntityType);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(error.FailureType, result.Error.FailureType);
        }

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract)]
        public void DeactivateNotification_Returns_Forbidden_If_Not_Allowed_To_Modify(RelatedEntityType relatedEntityType)
        {
            //Arrange
            var uuid = A<Guid>();
            var id = A<int>();
            var notification = new Advice{Id = id, Type = relatedEntityType};

            ExpectResolveIdReturns<Advice>(uuid, id);
            ExpectGetNotificationByIdReturns(id, notification);
            ExpectAllowModifyReturns(notification, false);

            //Act
            var result = _sut.DeactivateNotification(uuid, relatedEntityType);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract)]
        public void Can_DeleteNotification(RelatedEntityType relatedEntityType)
        {
            //Arrange
            var uuid = A<Guid>();
            var id = A<int>();
            var notification = new Advice{Id = id, Type = relatedEntityType};

            ExpectResolveIdReturns<Advice>(uuid, id);
            ExpectGetNotificationByIdReturns(id, notification);
            ExpectAllowDeleteReturns(notification, true);
            ExpectDeleteReturns(id, Maybe<OperationError>.None);

            //Act
            var error = _sut.DeleteNotification(uuid, relatedEntityType);

            //Assert
            Assert.True(error.IsNone);
        }

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract)]
        public void DeleteNotification_Returns_Error_When_Delete_Fails(RelatedEntityType relatedEntityType)
        {
            //Arrange
            var uuid = A<Guid>();
            var id = A<int>();
            var notification = new Advice{Id = id, Type = relatedEntityType};
            var expectedError = A<OperationError>();

            ExpectResolveIdReturns<Advice>(uuid, id);
            ExpectGetNotificationByIdReturns(id, notification);
            ExpectAllowDeleteReturns(notification, true);
            ExpectDeleteReturns(id, expectedError);

            //Act
            var error = _sut.DeleteNotification(uuid, relatedEntityType);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(expectedError.FailureType, error.Value.FailureType);
        }

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract)]
        public void DeleteNotification_Returns_Forbidden_If_Not_Allowed_To_Modify(RelatedEntityType relatedEntityType)
        {
            //Arrange
            var uuid = A<Guid>();
            var id = A<int>();
            var notification = new Advice{Id = id, Type = relatedEntityType};

            ExpectResolveIdReturns<Advice>(uuid, id);
            ExpectGetNotificationByIdReturns(id, notification);
            ExpectAllowDeleteReturns(notification, false);

            //Act
            var error = _sut.DeleteNotification(uuid, relatedEntityType);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.Forbidden, error.Value.FailureType);
        }

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract)]
        public void Can_GetAccessRights(RelatedEntityType relatedEntityType)
        {
            //Arrange
            var notificationUuid = A<Guid>();
            var notificationId = A<int>();
            var relatedEntityUuid = A<Guid>();
            var notification = CreateNewNotification(relatedEntityType);

            ExpectResolveIdReturns<Advice>(notificationUuid, notificationId);
            ExpectGetNotificationByIdReturns(notificationId, notification);
            var relatedEntity = ExpectGetRelatedEntityReturnsEntity(relatedEntityUuid, relatedEntityType);
            ExpectAllowModifyReturns(relatedEntity, true);
            ExpectAllowDeleteReturns(relatedEntity, true);

            //Act
            var result = _sut.GetAccessRights(notificationUuid, relatedEntityUuid, relatedEntityType);

            //Assert
            Assert.True(result.Ok);
            var accessRights = result.Value;
            Assert.True(accessRights.CanBeModified);
            Assert.True(accessRights.CanBeDeactivated);
            Assert.False(accessRights.CanBeDeleted);
        }

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract)]
        public void GetAccessRights_CanBeDeleted_Is_True(RelatedEntityType relatedEntityType)
        {
            //Arrange
            var notificationUuid = A<Guid>();
            var notificationId = A<int>();
            var relatedEntityUuid = A<Guid>();
            var notification = CreateNewNotification(relatedEntityType, false);

            ExpectResolveIdReturns<Advice>(notificationUuid, notificationId);
            ExpectGetNotificationByIdReturns(notificationId, notification);
            var relatedEntity = ExpectGetRelatedEntityReturnsEntity(relatedEntityUuid, relatedEntityType);
            ExpectAllowModifyReturns(relatedEntity, true);
            ExpectAllowDeleteReturns(relatedEntity, true);

            //Act
            var result = _sut.GetAccessRights(notificationUuid, relatedEntityUuid, relatedEntityType);

            //Assert
            Assert.True(result.Ok);
            var accessRights = result.Value;
            Assert.False(accessRights.CanBeModified);
            Assert.False(accessRights.CanBeDeactivated);
            Assert.True(accessRights.CanBeDeleted);
        }

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract)]
        public void GetAccessRights_CanBeDeleted_Is_False_When_Not_Allowed_To_Delete(RelatedEntityType relatedEntityType)
        {
            //Arrange
            var notificationUuid = A<Guid>();
            var notificationId = A<int>();
            var relatedEntityUuid = A<Guid>();
            var notification = CreateNewNotification(relatedEntityType, false);

            ExpectResolveIdReturns<Advice>(notificationUuid, notificationId);
            ExpectGetNotificationByIdReturns(notificationId, notification);
            var relatedEntity = ExpectGetRelatedEntityReturnsEntity(relatedEntityUuid, relatedEntityType);
            ExpectAllowModifyReturns(relatedEntity, true);
            ExpectAllowDeleteReturns(relatedEntity, false);

            //Act
            var result = _sut.GetAccessRights(notificationUuid, relatedEntityUuid, relatedEntityType);

            //Assert
            Assert.True(result.Ok);
            var accessRights = result.Value;
            Assert.False(accessRights.CanBeModified);
            Assert.False(accessRights.CanBeDeactivated);
            Assert.False(accessRights.CanBeDeleted);
        }

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract)]
        public void GetAccessRights_CanBeDeactivated_Is_False_When_Any_AdviceSent(RelatedEntityType relatedEntityType)
        {
            //Arrange
            var notificationUuid = A<Guid>();
            var notificationId = A<int>();
            var relatedEntityUuid = A<Guid>();
            var notification = CreateNewNotification(relatedEntityType);
            notification.AdviceSent = new List<AdviceSent> {new()};

            ExpectResolveIdReturns<Advice>(notificationUuid, notificationId);
            ExpectGetNotificationByIdReturns(notificationId, notification);
            var relatedEntity = ExpectGetRelatedEntityReturnsEntity(relatedEntityUuid, relatedEntityType);
            ExpectAllowModifyReturns(relatedEntity, true);
            ExpectAllowDeleteReturns(relatedEntity, true);

            //Act
            var result = _sut.GetAccessRights(notificationUuid, relatedEntityUuid, relatedEntityType);

            //Assert
            Assert.True(result.Ok);
            var accessRights = result.Value;
            Assert.True(accessRights.CanBeModified);
            Assert.False(accessRights.CanBeDeactivated);
            Assert.False(accessRights.CanBeDeleted);
        }

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract)]
        public void GetAccessRights_ReadOnly_When_Notification_Is_Immediate(RelatedEntityType relatedEntityType)
        {
            //Arrange
            var notificationUuid = A<Guid>();
            var notificationId = A<int>();
            var relatedEntityUuid = A<Guid>();
            var notification = CreateNewNotification(relatedEntityType, notificationType: AdviceType.Immediate);

            ExpectResolveIdReturns<Advice>(notificationUuid, notificationId);
            ExpectGetNotificationByIdReturns(notificationId, notification);
            var relatedEntity = ExpectGetRelatedEntityReturnsEntity(relatedEntityUuid, relatedEntityType);
            ExpectAllowModifyReturns(relatedEntity, true);
            ExpectAllowDeleteReturns(relatedEntity, true);

            //Act
            var result = _sut.GetAccessRights(notificationUuid, relatedEntityUuid, relatedEntityType);

            //Assert
            Assert.True(result.Ok);
            var accessRights = result.Value;
            Assert.False(accessRights.CanBeModified);
            Assert.False(accessRights.CanBeDeactivated);
            Assert.False(accessRights.CanBeDeleted);
        }

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract)]
        public void GetAccessRights_ReadOnly_When_Not_Allowed_To_Modify(RelatedEntityType relatedEntityType)
        {
            //Arrange
            var notificationUuid = A<Guid>();
            var notificationId = A<int>();
            var relatedEntityUuid = A<Guid>();
            var notification = CreateNewNotification(relatedEntityType);

            ExpectResolveIdReturns<Advice>(notificationUuid, notificationId);
            ExpectGetNotificationByIdReturns(notificationId, notification);
            var relatedEntity = ExpectGetRelatedEntityReturnsEntity(relatedEntityUuid, relatedEntityType);
            ExpectAllowModifyReturns(relatedEntity, false);

            //Act
            var result = _sut.GetAccessRights(notificationUuid, relatedEntityUuid, relatedEntityType);

            //Assert
            Assert.True(result.Ok);
            var accessRights = result.Value;
            Assert.False(accessRights.CanBeModified);
            Assert.False(accessRights.CanBeDeactivated);
            Assert.False(accessRights.CanBeDeleted);
        }

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract)]
        public void GetAccessRights_Returns_NotFound_When_Related_Entity_Is_Not_Found(RelatedEntityType relatedEntityType)
        {
            //Arrange
            var notificationUuid = A<Guid>();
            var notificationId = A<int>();
            var relatedEntityUuid = A<Guid>();
            var notification = CreateNewNotification(relatedEntityType);

            ExpectResolveIdReturns<Advice>(notificationUuid, notificationId);
            ExpectGetNotificationByIdReturns(notificationId, notification);
            ExpectGetRelatedEntityReturnsNone(relatedEntityType);

            //Act
            var result = _sut.GetAccessRights(notificationUuid, relatedEntityUuid, relatedEntityType);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract)]
        public void GetAccessRights_Returns_NotFound_When_Notification_Is_Not_Found(RelatedEntityType relatedEntityType)
        {
            //Arrange
            var notificationUuid = A<Guid>();
            var notificationId = A<int>();
            var relatedEntityUuid = A<Guid>();

            ExpectResolveIdReturns<Advice>(notificationUuid, notificationId);
            ExpectGetNotificationByIdReturns(notificationId, Maybe<Advice>.None);

            //Act
            var result = _sut.GetAccessRights(notificationUuid, relatedEntityUuid, relatedEntityType);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract)]
        public void GetAccessRights_Returns_NotFound_When_NotificationId_Is_Not_Found(RelatedEntityType relatedEntityType)
        {
            //Arrange
            var notificationUuid = A<Guid>();
            var relatedEntityUuid = A<Guid>();

            ExpectResolveIdReturns<Advice>(notificationUuid, Maybe<int>.None);

            //Act
            var result = _sut.GetAccessRights(notificationUuid, relatedEntityUuid, relatedEntityType);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        private void Test_Command_Which_Updates_Notification_Returns_Failure_Forbidden<TSuccess>(Guid uuid, RelatedEntityType relatedEntityType, Func<IEntityWithAdvices, Result<TSuccess, OperationError>> command)
        {
            //Arrange
            var transaction = ExpectDatabaseTransactionReturns();
            var entity = ExpectGetRelatedEntityReturnsEntity(uuid, relatedEntityType);
            ExpectAllowModifyReturns(entity, false);

            //Act

            var result = command(entity);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
            transaction.Verify(x => x.Commit(), Times.Never);
        }

        private void Test_Command_Which_Updates_Notification_Returns_Failure_NotFound<TSuccess>(RelatedEntityType relatedEntityType, Func<IEntityWithAdvices, Result<TSuccess, OperationError>> command)
        {
            //Arrange
            var transaction = ExpectDatabaseTransactionReturns();
            var entity = ExpectGetRelatedEntityReturnsEntity(A<Guid>(), relatedEntityType);

            //Act
            var result = command(entity);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
            transaction.Verify(x => x.Commit(), Times.Never);
        }

        private static Advice CreateNewNotification(RelatedEntityType relatedEntityType, bool isActive = true,
            AdviceType notificationType = AdviceType.Repeat)
        {
            return new Advice
            {
                IsActive = isActive,
                Type = relatedEntityType,
                AdviceType = notificationType,
            };
        }
        
        private ImmediateNotificationModificationParameters CreateNewParameters(RelatedEntityType relatedEntityType)
        {
            return new ImmediateNotificationModificationParameters(A<string>(), A<string>(), relatedEntityType, A<Guid>(), A<RootRecipientModificationParameters>(), A<RootRecipientModificationParameters>());
        }

        private UpdateScheduledNotificationModificationParameters CreateNewUpdateScheduledParameters(RelatedEntityType relatedEntityType)
        {
            return new UpdateScheduledNotificationModificationParameters(A<string>(),
                A<string>(),
                relatedEntityType,
                A<Guid>(),
                A<RootRecipientModificationParameters>(),
                A<RootRecipientModificationParameters>(),
                A<string>(),
                A<DateTime>());
        }

        private ScheduledNotificationModificationParameters CreateNewScheduledParameters(RelatedEntityType relatedEntityType)
        {
            return new ScheduledNotificationModificationParameters(A<string>(),
                A<string>(),
                relatedEntityType,
                A<Guid>(),
                A<RootRecipientModificationParameters>(),
                A<RootRecipientModificationParameters>(),
                A<string>(),
                A<DateTime>(),
                A<Scheduling>(),
                A<DateTime>());
        }

        private IReadOnlyList<int> ExpectRoleRecipientIds(ImmediateNotificationModificationParameters parameters)
        {
            var roleIds = new List<int>();

            roleIds.AddRange(ExpectRoleRecipientIdsForType(parameters.Ccs.RoleRecipients, parameters.Type));
            roleIds.AddRange(ExpectRoleRecipientIdsForType(parameters.Receivers.RoleRecipients, parameters.Type));

            return roleIds;
        }

        private IEnumerable<int> ExpectRoleRecipientIdsForType(IEnumerable<RoleRecipientModificationParameters> roles, RelatedEntityType type)
        {
            var roleIds = new List<int>();
            foreach (var ccsRoleRecipient in roles)
            {
                var newId = A<int>();
                roleIds.Add(newId);
                ExpectResolveRoleIdReturns(ccsRoleRecipient.RoleUuid, type, newId);
            }

            return roleIds;
        }

        private void ExpectResolveIdReturns<T>(Guid uuid, Maybe<int> result) where T: class, IHasUuid, IHasId
        {
            _entityIdentityResolver.Setup(x => x.ResolveDbId<T>(uuid)).Returns(result);
        }

        private void ExpectGetNotificationsByOrganizationIdReturns(int orgId, Result<IQueryable<Advice>, OperationError> result)
        {
            _registrationNotificationService.Setup(x => x.GetNotificationsByOrganizationId(orgId)).Returns(result);
        }

        private void ExpectGetNotificationByIdReturns(int notificationId, Maybe<Advice> result)
        {
            _registrationNotificationService.Setup(x => x.GetNotificationById(notificationId)).Returns(result);
        }

        private void ExpectGetSentReturns(IQueryable<AdviceSent> result)
        {
            _registrationNotificationService.Setup(x => x.GetSent()).Returns(result);
        }

        private void ExpectCreateReturns(ImmediateNotificationModificationParameters parameters, AdviceType adviceType, IReadOnlyList<int> roleIds, Result<Advice, OperationError> result)
        {
            _registrationNotificationService.Setup(x => x.Create(It.Is<NotificationModel>(notificationModel => 
                notificationModel.Subject == parameters.Subject && 
                notificationModel.Body == parameters.Body && 
                notificationModel.AdviceType == adviceType && 
                CheckRecipientIsMappedCorrectly(parameters, roleIds, notificationModel.Recipients)))).Returns(result);
        }

        private void ExpectCreateReturns(ScheduledNotificationModificationParameters parameters, AdviceType adviceType, IReadOnlyList<int> roleIds, Result<Advice, OperationError> result)
        {
            _registrationNotificationService.Setup(x => x.Create(It.Is<NotificationModel>(notificationModel => 
                notificationModel.Subject == parameters.Subject && 
                notificationModel.Body == parameters.Body && 
                notificationModel.AdviceType == adviceType &&
                notificationModel.FromDate == parameters.FromDate &&
                notificationModel.ToDate == parameters.ToDate &&
                notificationModel.RepetitionFrequency == parameters.RepetitionFrequency && 
                notificationModel.Name == parameters.Name &&
                CheckRecipientIsMappedCorrectly(parameters, roleIds, notificationModel.Recipients)))).Returns(result);
        }

        private void ExpectUpdateReturns(int id, UpdateScheduledNotificationModificationParameters parameters, Result<Advice, OperationError> result)
        {
            _registrationNotificationService.Setup(x => x.Update(id, 
                It.Is<NotificationModel>(notificationModel => 
                notificationModel.Subject == parameters.Subject && 
                notificationModel.Body == parameters.Body && 
                notificationModel.AdviceType == AdviceType.Repeat &&
                notificationModel.ToDate == parameters.ToDate &&
                notificationModel.Name == parameters.Name))).Returns(result);
        }

        private void ExpectUpdateNotificationUserRelations(int notificationId, UpdateScheduledNotificationModificationParameters parameters, IReadOnlyList<int> roleIds,
            Maybe<OperationError> result)
        {
            _notificationUserRelationsService.Setup(x => x.UpdateNotificationUserRelations(notificationId,
                    It.Is<IEnumerable<RecipientModel>>(recipientModels => CheckRecipientIsMappedCorrectly(parameters, roleIds, recipientModels))))
                .Returns(result);
        }

        private bool CheckRecipientIsMappedCorrectly(ImmediateNotificationModificationParameters parameters,
            IReadOnlyList<int> roleIds,
            IEnumerable<RecipientModel> models)
        {
            foreach (var recipientModel in models)
            {
                if (parameters.Ccs.EmailRecipients.Select(x => x.Email).Contains(recipientModel.Email))
                {
                    continue;
                }

                if (parameters.Receivers.EmailRecipients.Select(x => x.Email).Contains(recipientModel.Email))
                {
                    continue;
                }

                var relationIdResult = GetRelationIdFromRecipientModel(recipientModel);
                if (relationIdResult.IsNone)
                    return false;
                var relationId = relationIdResult.Value;

                if (roleIds.Contains(relationId) == false)
                {
                    return false;
                }
            }

            return true;
        }

        private Maybe<int> GetRelationIdFromRecipientModel(RecipientModel recipientModel)
        {
            return recipientModel.DataProcessingRegistrationRoleId ??
                   recipientModel.ItContractRoleId ?? recipientModel.ItSystemRoleId;
        }

        private void ExpectDeactivateReturns(int id, Result<Advice, OperationError> result)
        {
            _registrationNotificationService.Setup(x => x.DeactivateNotification(id)).Returns(result);
        }

        private void ExpectDeleteReturns(int id, Maybe<OperationError> result)
        {
            _registrationNotificationService.Setup(x => x.Delete(id)).Returns(result);
        }

        private Mock<IDatabaseTransaction> ExpectDatabaseTransactionReturns()
        {
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            return transaction;
        }

        private void ExpectResolveRoleIdReturns(Guid uuid, RelatedEntityType relatedEntityType, Maybe<int> result)
        {
            switch (relatedEntityType)
            {
                case RelatedEntityType.dataProcessingRegistration:
                    ExpectResolveIdReturns<DataProcessingRegistrationRole>(uuid, result);
                    break;
                case RelatedEntityType.itContract:
                    ExpectResolveIdReturns<ItContractRole>(uuid, result);
                    break;
                case RelatedEntityType.itSystemUsage:
                    ExpectResolveIdReturns<ItSystemRole>(uuid, result);
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(relatedEntityType), relatedEntityType, null);
            }
        }

        private IEntityWithAdvices ExpectGetRelatedEntityReturnsEntity(Guid uuid, RelatedEntityType relatedEntityType)
        {
            switch (relatedEntityType)
            {
                case RelatedEntityType.dataProcessingRegistration:
                    var dpr = new DataProcessingRegistration(){Uuid = uuid};
                    _dprRepository.Setup(x => x.AsQueryable()).Returns(new List<DataProcessingRegistration>{dpr}.AsQueryable);
                    return dpr;
                case RelatedEntityType.itContract:
                    var contract = new ItContract() { Uuid = uuid };
                    _contractRepository.Setup(x => x.AsQueryable()).Returns(new List<ItContract> { contract }.AsQueryable);
                    return contract;
                case RelatedEntityType.itSystemUsage:
                    var usage = new ItSystemUsage() { Uuid = uuid };
                    _usageRepository.Setup(x => x.AsQueryable()).Returns(new List<ItSystemUsage> { usage }.AsQueryable);
                    return usage;
                default: throw new ArgumentOutOfRangeException(nameof(relatedEntityType), relatedEntityType, null);
            }
        }

        private void ExpectGetRelatedEntityReturnsNone(RelatedEntityType relatedEntityType)
        {
            switch (relatedEntityType)
            {
                case RelatedEntityType.dataProcessingRegistration:
                    _dprRepository.Setup(x => x.AsQueryable()).Returns(new List<DataProcessingRegistration>().AsQueryable);
                    break;
                case RelatedEntityType.itContract:
                    _contractRepository.Setup(x => x.AsQueryable()).Returns(new List<ItContract>().AsQueryable);
                    break;
                case RelatedEntityType.itSystemUsage:
                    _usageRepository.Setup(x => x.AsQueryable()).Returns(new List<ItSystemUsage>().AsQueryable);
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(relatedEntityType), relatedEntityType, null);
            }
        }

        private void ExpectUpdateRelatedEntityReturns(IEntityWithAdvices entity, RelatedEntityType relatedEntityType)
        {
            switch (relatedEntityType)
            {
                case RelatedEntityType.dataProcessingRegistration:
                    var dpr = entity as DataProcessingRegistration;
                    _dprRepository.Setup(x => x.Update(dpr));
                    ExpectRaiseUpdateEvent<DataProcessingRegistration>(dpr);
                    break;
                case RelatedEntityType.itContract:
                    var contract = entity as ItContract;
                    _contractRepository.Setup(x => x.Update(contract));
                    ExpectRaiseUpdateEvent<ItContract>(contract);
                    break;
                case RelatedEntityType.itSystemUsage:
                    var usage = entity as ItSystemUsage;
                    _usageRepository.Setup(x => x.Update(usage));
                    ExpectRaiseUpdateEvent<ItSystemUsage>(usage);
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(relatedEntityType), relatedEntityType, null);
            }
        }

        private void ExpectRaiseUpdateEvent<T>(IEntity entity)
        {
            _domainEvents.Setup(x => x.Raise(It.Is<EntityUpdatedEvent<T>>(eventParameter => eventParameter == entity)));
        }

        private void ExpectAllowModifyReturns(IEntity entity, bool result)
        {
            _authorizationContext.Setup(x => x.AllowModify(entity)).Returns(result);
        }

        private void ExpectAllowDeleteReturns(IEntity entity, bool result)
        {
            _authorizationContext.Setup(x => x.AllowDelete(entity)).Returns(result);
        }

        private void VerifyDatabaseSave(RelatedEntityType relatedEntityType)
        {
            switch (relatedEntityType)
            {
                case RelatedEntityType.dataProcessingRegistration:
                    _dprRepository.Verify(x => x.Save(), Times.Once);
                    break;
                case RelatedEntityType.itContract:
                    _contractRepository.Verify(x => x.Save(), Times.Once);
                    break;
                case RelatedEntityType.itSystemUsage:
                    _usageRepository.Verify(x => x.Save(), Times.Once);
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(relatedEntityType), relatedEntityType, null);
            }
        }
    }
}
