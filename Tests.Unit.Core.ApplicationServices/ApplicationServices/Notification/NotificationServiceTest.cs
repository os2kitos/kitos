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
            var relatedEntityUuid = A<Guid>();
            var relatedEntityType = A<RelatedEntityType>();

            var root = ExpectGetRelatedEntityReturnsEntity(relatedEntityUuid, relatedEntityType);
            var notifications = new List<Advice> {CreateNewNotification(relatedEntityType, relationId: root.Id), CreateNewNotification(relatedEntityType, relationId: root.Id) }.AsQueryable();

            ExpectResolveIdReturns<Organization>(orgUuid, orgId);
            ExpectGetNotificationsByOrganizationIdReturns(orgId, Result<IQueryable<Advice>, OperationError>.Success(notifications));

            //Act
            var result = _sut.GetNotifications(orgUuid, 0, int.MaxValue);

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
            var relatedEntityUuid = A<Guid>();
            var relatedEntityType = A<RelatedEntityType>();
            
            var root = ExpectGetRelatedEntityReturnsEntity(relatedEntityUuid, relatedEntityType);
            var notification = CreateNewNotification(relatedEntityType, relationId: root.Id);
            var condition1 = new QueryByOwnerResourceType(relatedEntityType);
            var condition2 = new QueryByOwnerResourceId(root.Id);

            var notifications = new List<Advice> { notification, CreateNewNotification(relatedEntityType) }.AsQueryable();

            ExpectResolveIdReturns<Organization>(orgUuid, orgId);
            ExpectGetNotificationsByOrganizationIdReturns(orgId, Result<IQueryable<Advice>, OperationError>.Success(notifications));

            //Act
            var result = _sut.GetNotifications(orgUuid, 0, int.MaxValue, condition1, condition2);

            //Arrange
            Assert.True(result.Ok);
            var resultList = result.Value.ToList();
            var resultNotification = Assert.Single(resultList);
            Assert.Equal(notification.RelationId, resultNotification.OwnerResource.Id);
        }

        [Fact]
        public void Can_GetNotifications_With_Pagination()
        {
            //Arrange
            var orgUuid = A<Guid>();
            var orgId = A<int>();
            var relatedEntityUuid = A<Guid>();
            var relatedEntityType = A<RelatedEntityType>();
            

            var root = ExpectGetRelatedEntityReturnsEntity(relatedEntityUuid, relatedEntityType);
            var notificationId = A<int>();
            var notification2Id = notificationId + A<int>();
            var notification = CreateNewNotification(relatedEntityType, id: notification2Id, relationId: root.Id);

            var notifications = new List<Advice> { CreateNewNotification(relatedEntityType, id: notificationId), notification, CreateNewNotification(relatedEntityType, id: notification2Id + A<int>()) }.AsQueryable();

            ExpectResolveIdReturns<Organization>(orgUuid, orgId);
            ExpectGetNotificationsByOrganizationIdReturns(orgId, Result<IQueryable<Advice>, OperationError>.Success(notifications));

            //Act
            var result = _sut.GetNotifications(orgUuid, 1, 1);

            //Arrange
            Assert.True(result.Ok);
            var resultNotification = Assert.Single(result.Value);
            Assert.Equal(notification.Uuid, resultNotification.Uuid);
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
            var result = _sut.GetNotifications(orgUuid, 0, int.MaxValue);

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
            var result = _sut.GetNotifications(orgUuid, 0, int.MaxValue);

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
            var relatedUuid = A<Guid>();
            var id = A<int>();
            var notification = new Advice {Id = id, Type = relatedEntityType};

            ExpectResolveIdReturns<Advice>(uuid, id);
            ExpectGetNotificationByIdReturns(id, notification);
            var root = ExpectGetRelatedEntityReturnsEntity(relatedUuid, relatedEntityType);
            ExpectAllowReadsReturns(root, true);
            notification.RelationId = root.Id;

            //Act
            var result = _sut.GetNotificationByUuid(uuid, relatedUuid, relatedEntityType);

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
            var relatedUuid = A<Guid>();
            var id = A<int>();
            var notification = new Advice {Id = id, Type = actualType };

            ExpectResolveIdReturns<Advice>(uuid, id);
            ExpectGetNotificationByIdReturns(id, notification);
            ExpectGetPermissionReturnsTrue(relatedUuid, relatedEntityType);

            //Act
            var result = _sut.GetNotificationByUuid(uuid, relatedUuid, relatedEntityType);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void GetNotificationByUuid_Returns_Error_When_GetNotification_Failed()
        {
            //Arrange
            var uuid = A<Guid>();
            var relatedUuid = A<Guid>();
            var id = A<int>();
            var error = A<OperationError>();

            ExpectResolveIdReturns<Advice>(uuid, id);
            ExpectGetNotificationByIdReturns(id, error);

            //Act
            var result = _sut.GetNotificationByUuid(uuid, relatedUuid, A<RelatedEntityType>());

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(error.FailureType, result.Error.FailureType);
        }

        [Fact]
        public void GetNotificationByUuid_Returns_NotFound_When_NotificationId_Was_Not_Found()
        {
            //Arrange
            var uuid = A<Guid>();
            var relatedUuid = A<Guid>();

            ExpectResolveIdReturns<Advice>(uuid, Maybe<int>.None);

            //Act
            var result = _sut.GetNotificationByUuid(uuid, relatedUuid, A<RelatedEntityType>());

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
            var relatedUuid = A<Guid>();
            var notification = CreateNewNotification(relatedEntityType, uuid: uuid);
            var notificationSentIQueryable = new List<AdviceSent> { new() { Advice = notification }, new() { Advice = new Advice() } }.AsQueryable();
            
            ExpectResolveIdReturns<Advice>(uuid, notification.Id);
            ExpectGetNotificationByIdReturns(notification.Id, notification);
            var root = ExpectGetRelatedEntityReturnsEntity(relatedUuid, relatedEntityType);
            notification.RelationId = root.Id;
            ExpectAllowReadsReturns(root, true);
            ExpectGetSentReturns(notificationSentIQueryable);

            //Act
            var result = _sut.GetNotificationSentByUuid(uuid, relatedUuid, relatedEntityType);

            //Assert
            Assert.True(result.Ok);
            var notificationSent = Assert.Single(result.Value);
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

            var transaction = ExpectDatabaseTransactionReturns();
            var relatedEntity = ExpectGetRelatedEntityReturnsEntity(parameters.BaseProperties.OwnerResourceUuid, parameters.BaseProperties.Type);
            var roleIds = ExpectRoleRecipientIds(parameters);
            ExpectAllowModifyReturns(relatedEntity, true);

            var notification = new Advice {RelationId = relatedEntity.Id, Type = relatedEntityType};
            ExpectImmediateCreateReturns(parameters, AdviceType.Immediate, roleIds, notification);

            //Act
            var result = _sut.CreateImmediateNotification(parameters);

            //Assert
            Assert.True(result.Ok);
            transaction.Verify(x => x.Commit(), Times.Once);
            VerifyDatabaseSave(parameters.BaseProperties.Type);
        }

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract)]
        public void CreateImmediateNotification_Returns_Forbidden_When_Not_Allowed_To_Modify(RelatedEntityType relatedEntityType)
        {
            var parameters = CreateNewParameters(relatedEntityType);
            Test_Command_Which_Updates_Notification_Returns_Failure_Forbidden(parameters.BaseProperties.OwnerResourceUuid, relatedEntityType, _ => _sut.CreateImmediateNotification(parameters));
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

            var transaction = ExpectDatabaseTransactionReturns();
            var relatedEntity = ExpectGetRelatedEntityReturnsEntity(parameters.BaseProperties.OwnerResourceUuid, relatedEntityType);
            var roleIds = ExpectRoleRecipientIds(parameters);
            ExpectAllowModifyReturns(relatedEntity, true);

            var notification = new Advice {RelationId = relatedEntity.Id, Type = relatedEntityType};
            ExpectScheduledCreateReturns(parameters, AdviceType.Repeat, roleIds, notification);

            //Act
            var result = _sut.CreateScheduledNotification(parameters);

            //Assert
            Assert.True(result.Ok);
            transaction.Verify(x => x.Commit(), Times.Once);
            VerifyDatabaseSave(parameters.BaseProperties.Type);
        }

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract)]
        public void CreateScheduledNotification_Returns_Forbidden_When_Not_Allowed_To_Modify(RelatedEntityType relatedEntityType)
        {
            var parameters = CreateNewScheduledParameters(relatedEntityType);
            Test_Command_Which_Updates_Notification_Returns_Failure_Forbidden(parameters.BaseProperties.OwnerResourceUuid, relatedEntityType, _ => _sut.CreateScheduledNotification(parameters));
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

            var transaction = ExpectDatabaseTransactionReturns();
            var relatedEntity = ExpectGetRelatedEntityReturnsEntity(parameters.BaseProperties.OwnerResourceUuid, parameters.BaseProperties.Type);

            var notification = CreateNewNotification(relatedEntityType, id: id, relationId: relatedEntity.Id); 
            var resolvedRoleIds = ExpectRoleRecipientIds(parameters);
            ExpectResolveIdReturns<Advice>(uuid, id);
            ExpectAllowModifyReturns(relatedEntity, true);
            ExpectUpdateNotificationUserRelations(id, parameters, resolvedRoleIds, parameters.BaseProperties.Type, notification);
            ExpectUpdateReturns(id, parameters, notification);

            //Act
            var result = _sut.UpdateScheduledNotification(uuid, parameters);

            //Assert
            Assert.True(result.Ok);
            transaction.Verify(x => x.Commit(), Times.Once);
            VerifyDatabaseSave(parameters.BaseProperties.Type);
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
            var relatedEntity = ExpectGetRelatedEntityReturnsEntity(parameters.BaseProperties.OwnerResourceUuid, parameters.BaseProperties.Type);
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
            var relatedEntity = ExpectGetRelatedEntityReturnsEntity(parameters.BaseProperties.OwnerResourceUuid, parameters.BaseProperties.Type);
            var resolvedRoleIds = ExpectRoleRecipientIds(parameters);
            ExpectResolveIdReturns<Advice>(uuid, id);
            ExpectAllowModifyReturns(relatedEntity, true);
            ExpectUpdateNotificationUserRelations(id, parameters, resolvedRoleIds, parameters.BaseProperties.Type, error);

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
            var parameters = CreateNewUpdateScheduledParameters(relatedEntityType);
            Test_Command_Which_Updates_Notification_Returns_Failure_Forbidden(parameters.BaseProperties.OwnerResourceUuid, relatedEntityType, _ => _sut.UpdateScheduledNotification(A<Guid>(), parameters));
        }

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract)]
        public void UpdateScheduledNotification_Returns_Forbidden_When_RelatedEntity_Not_Found(RelatedEntityType relatedEntityType)
        {
            var parameters = CreateNewUpdateScheduledParameters(relatedEntityType);
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
            var relationUuid = A<Guid>();
            var id = A<int>();

            ExpectResolveIdReturns<Advice>(uuid, id);
            var entity = ExpectGetRelatedEntityReturnsEntity(relationUuid, relatedEntityType);
            var notification = CreateNewNotification(relatedEntityType, id: id, relationId: entity.Id);

            ExpectGetNotificationByIdReturns(id, notification);
            ExpectGetPermissionReturnsTrue(entity);
            ExpectDeactivateReturns(id, notification);

            //Act
            var result = _sut.DeactivateNotification(uuid, relationUuid, relatedEntityType);

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
            var relationUuid = A<Guid>();
            var id = A<int>();
            var notification = CreateNewNotification(relatedEntityType, id: id);
            var error = A<OperationError>();

            ExpectResolveIdReturns<Advice>(uuid, id);
            ExpectGetNotificationByIdReturns(id, notification);
            ExpectDeactivateReturns(id, error);
            notification.RelationId = ExpectGetPermissionReturnsTrue(relationUuid, relatedEntityType);

            //Act
            var result = _sut.DeactivateNotification(uuid, relationUuid, relatedEntityType);

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
            var relationUuid = A<Guid>();
            var id = A<int>();
            var notification = new Advice{Id = id, Type = relatedEntityType};

            ExpectResolveIdReturns<Advice>(uuid, id);
            ExpectGetNotificationByIdReturns(id, notification);
            ExpectAllowModifyReturns(notification, false);
            notification.RelationId = ExpectGetPermissionReturnsTrue(relationUuid, relatedEntityType);

            //Act
            var result = _sut.DeactivateNotification(uuid, relationUuid, relatedEntityType);

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
            var relationUuid = A<Guid>();
            var id = A<int>();
            var notification = CreateNewNotification(relatedEntityType, isActive:false, id: id);

            ExpectResolveIdReturns<Advice>(uuid, id);
            ExpectGetNotificationByIdReturns(id, notification);
            ExpectDeleteReturns(id, Maybe<OperationError>.None);
            notification.RelationId = ExpectGetPermissionReturnsTrue(relationUuid, relatedEntityType);

            //Act
            var error = _sut.DeleteNotification(uuid, relationUuid, relatedEntityType);

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
            var relationUuid = A<Guid>();
            var id = A<int>();
            var notification = CreateNewNotification(relatedEntityType, isActive: false, id: id);
            var expectedError = A<OperationError>();

            ExpectResolveIdReturns<Advice>(uuid, id);
            ExpectGetNotificationByIdReturns(id, notification);
            ExpectAllowDeleteReturns(notification, true);
            ExpectDeleteReturns(id, expectedError);
            notification.RelationId = ExpectGetPermissionReturnsTrue(relationUuid, relatedEntityType);

            //Act
            var error = _sut.DeleteNotification(uuid, relationUuid, relatedEntityType);

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
            var relationUuid = A<Guid>();
            var id = A<int>();
            var notification = CreateNewNotification(relatedEntityType, isActive: false, id: id);

            ExpectResolveIdReturns<Advice>(uuid, id);
            ExpectGetNotificationByIdReturns(id, notification);
            ExpectAllowDeleteReturns(notification, false);
            notification.RelationId = ExpectGetPermissionReturnsTrue(relationUuid, relatedEntityType, false);

            //Act
            var error = _sut.DeleteNotification(uuid, relationUuid, relatedEntityType);

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
            ExpectGetPermissionReturnsTrue(relatedEntityUuid, relatedEntityType);

            //Act
            var result = _sut.GetPermissions(notificationUuid, relatedEntityUuid, relatedEntityType);

            //Assert
            Assert.True(result.Ok);
            var accessRights = result.Value;
            Assert.True(accessRights.Read);
            Assert.True(accessRights.Modify);
            Assert.True(accessRights.Deactivate);
            Assert.False(accessRights.Delete);
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
            ExpectGetPermissionReturnsTrue(relatedEntityUuid, relatedEntityType);

            //Act
            var result = _sut.GetPermissions(notificationUuid, relatedEntityUuid, relatedEntityType);

            //Assert
            Assert.True(result.Ok);
            var accessRights = result.Value;
            Assert.True(accessRights.Read);
            Assert.False(accessRights.Modify);
            Assert.False(accessRights.Deactivate);
            Assert.True(accessRights.Delete);
        }

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract)]
        public void GetAccessRights_Modify_Deactivate_Delete_Is_False_When_Not_Allowed_To_Modify(RelatedEntityType relatedEntityType)
        {
            //Arrange
            var notificationUuid = A<Guid>();
            var notificationId = A<int>();
            var relatedEntityUuid = A<Guid>();
            var notification = CreateNewNotification(relatedEntityType, id: notificationId);

            ExpectResolveIdReturns<Advice>(notificationUuid, notificationId);
            ExpectGetNotificationByIdReturns(notificationId, notification);
            ExpectGetPermissionReturnsTrue(relatedEntityUuid, relatedEntityType, false);

            //Act
            var result = _sut.GetPermissions(notificationUuid, relatedEntityUuid, relatedEntityType);

            //Assert
            Assert.True(result.Ok);
            var accessRights = result.Value;
            Assert.True(accessRights.Read);
            Assert.False(accessRights.Modify);
            Assert.False(accessRights.Deactivate);
            Assert.False(accessRights.Delete);
        }

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract)]
        public void GetAccessRights_CanBeDeactivated_Is_True_When_Any_AdviceSent(RelatedEntityType relatedEntityType)
        {
            //Arrange
            var notificationUuid = A<Guid>();
            var notificationId = A<int>();
            var relatedEntityUuid = A<Guid>();
            var notification = CreateNewNotification(relatedEntityType);
            notification.AdviceSent = new List<AdviceSent> {new()};

            ExpectResolveIdReturns<Advice>(notificationUuid, notificationId);
            ExpectGetNotificationByIdReturns(notificationId, notification);
            ExpectGetPermissionReturnsTrue(relatedEntityUuid, relatedEntityType);

            //Act
            var result = _sut.GetPermissions(notificationUuid, relatedEntityUuid, relatedEntityType);

            //Assert
            Assert.True(result.Ok);
            var accessRights = result.Value;
            Assert.True(accessRights.Read);
            Assert.True(accessRights.Modify);
            Assert.True(accessRights.Deactivate);
            Assert.False(accessRights.Delete);
        }

        [Theory]
        [MemberData(nameof(AllRelatedEntityTypeAndBoolFlagCombinations))]
        public void GetAccessRights_Can_Only_Delete_When_Notification_Is_Immediate_And_Active(RelatedEntityType relatedEntityType, bool isActive)
        {
            //Arrange
            var notificationUuid = A<Guid>();
            var notificationId = A<int>();
            var relatedEntityUuid = A<Guid>();
            var notification = CreateNewNotification(relatedEntityType, notificationType: AdviceType.Immediate, isActive: isActive);

            ExpectResolveIdReturns<Advice>(notificationUuid, notificationId);
            ExpectGetNotificationByIdReturns(notificationId, notification);
            ExpectGetPermissionReturnsTrue(relatedEntityUuid, relatedEntityType);

            //Act
            var result = _sut.GetPermissions(notificationUuid, relatedEntityUuid, relatedEntityType);

            //Assert
            Assert.True(result.Ok);
            var accessRights = result.Value;
            Assert.True(accessRights.Read);
            Assert.False(accessRights.Modify);
            Assert.False(accessRights.Deactivate);
            Assert.Equal(accessRights.Delete, isActive);
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
            var result = _sut.GetPermissions(notificationUuid, relatedEntityUuid, relatedEntityType);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract)]
        public void GetAccessRights_Returns_Error_When_GetNotification_Fails(RelatedEntityType relatedEntityType)
        {
            //Arrange
            var notificationUuid = A<Guid>();
            var notificationId = A<int>();
            var relatedEntityUuid = A<Guid>();
            var error = A<OperationError>();

            ExpectResolveIdReturns<Advice>(notificationUuid, notificationId);
            ExpectGetNotificationByIdReturns(notificationId, error);

            //Act
            var result = _sut.GetPermissions(notificationUuid, relatedEntityUuid, relatedEntityType);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(error.FailureType, result.Error.FailureType);
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
            var result = _sut.GetPermissions(notificationUuid, relatedEntityUuid, relatedEntityType);

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

        private Advice CreateNewNotification(RelatedEntityType relatedEntityType, bool isActive = true, AdviceType notificationType = AdviceType.Repeat, Guid? uuid =null, int? id = null, int? relationId = null)
        {
            return new Advice
            {
                Id = id ?? A<int>(),
                Uuid = uuid ?? A<Guid>(),
                RelationId = relationId ?? A<int>(),
                IsActive = isActive,
                Type = relatedEntityType,
                AdviceType = notificationType,
            };
        }
        
        private ImmediateNotificationModificationParameters CreateNewParameters(RelatedEntityType relatedEntityType)
        {
            return new ImmediateNotificationModificationParameters(new BaseNotificationPropertiesModificationParameters(A<string>(), A<string>(), relatedEntityType, A<Guid>(), A<RootRecipientModificationParameters>(), A<RootRecipientModificationParameters>()));
        }

        private UpdateScheduledNotificationModificationParameters CreateNewUpdateScheduledParameters(RelatedEntityType relatedEntityType)
        {
            return new UpdateScheduledNotificationModificationParameters(new BaseNotificationPropertiesModificationParameters(
                    A<string>(),
                    A<string>(),
                    relatedEntityType,
                    A<Guid>(),
                    A<RootRecipientModificationParameters>(),
                    A<RootRecipientModificationParameters>()
                ),
                A<string>(),
                A<DateTime>());
        }

        private CreateScheduledNotificationModificationParameters CreateNewScheduledParameters(RelatedEntityType relatedEntityType)
        {
            var random = new Random();
            return new CreateScheduledNotificationModificationParameters(
                new BaseNotificationPropertiesModificationParameters
                (
                    A<string>(),
                    A<string>(),
                    relatedEntityType,
                    A<Guid>(),
                    A<RootRecipientModificationParameters>(),
                    A<RootRecipientModificationParameters>()
                ),
                A<string>(),
                A<DateTime>(),
                (Scheduling)random.Next(1, 8),
                A<DateTime>());
        }

        private int ExpectGetPermissionReturnsTrue(Guid relationUuid, RelatedEntityType relatedEntityType, bool modifyResult = true, bool readResult = true)
        {
            var root = ExpectGetRelatedEntityReturnsEntity(relationUuid, relatedEntityType);
            ExpectAllowReadsReturns(root, readResult);
            ExpectAllowModifyReturns(root, modifyResult);

            return root.Id;
        }

        private void ExpectGetPermissionReturnsTrue(IEntityWithAdvices root, bool modifyResult = true, bool readResult = true)
        {
            ExpectAllowReadsReturns(root, readResult);
            ExpectAllowModifyReturns(root, modifyResult);
        }

        private IReadOnlyList<int> ExpectRoleRecipientIds<T>(T parameters) where T : class, IHasBaseNotificationPropertiesParameters
        {
            var roleIds = new List<int>();

            roleIds.AddRange(ExpectRoleRecipientIdsForType(parameters.BaseProperties.Ccs.RoleRecipients, parameters.BaseProperties.Type));
            roleIds.AddRange(ExpectRoleRecipientIdsForType(parameters.BaseProperties.Receivers.RoleRecipients, parameters.BaseProperties.Type));

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

        private void ExpectGetNotificationByIdReturns(int notificationId, Result<Advice, OperationError> result)
        {
            _registrationNotificationService.Setup(x => x.GetNotificationById(notificationId)).Returns(result);
        }

        private void ExpectGetSentReturns(IQueryable<AdviceSent> result)
        {
            _registrationNotificationService.Setup(x => x.GetSent()).Returns(result);
        }

        private void ExpectImmediateCreateReturns(ImmediateNotificationModificationParameters parameters, AdviceType adviceType, IReadOnlyList<int> roleIds, Result<Advice, OperationError> result)
        {
            _registrationNotificationService.Setup(x => x.CreateImmediateNotification(It.Is<ImmediateNotificationModel>(notificationModel => 
                notificationModel.BaseProperties.Subject == parameters.BaseProperties.Subject && 
                notificationModel.BaseProperties.Body == parameters.BaseProperties.Body && 
                notificationModel.BaseProperties.AdviceType == adviceType &&
                CheckRecipientIsMappedCorrectly(parameters, roleIds, notificationModel.Ccs) &&
                CheckRecipientIsMappedCorrectly(parameters, roleIds, notificationModel.Receivers)))).Returns(result);
        }

        private void ExpectScheduledCreateReturns(CreateScheduledNotificationModificationParameters parameters, AdviceType adviceType, IReadOnlyList<int> roleIds, Result<Advice, OperationError> result)
        {
            _registrationNotificationService.Setup(x => x.CreateScheduledNotification(It.Is<ScheduledNotificationModel>(notificationModel => 
                notificationModel.BaseProperties.Subject == parameters.BaseProperties.Subject && 
                notificationModel.BaseProperties.Body == parameters.BaseProperties.Body && 
                notificationModel.BaseProperties.AdviceType == adviceType &&
                notificationModel.FromDate == parameters.FromDate &&
                notificationModel.ToDate == parameters.ToDate &&
                notificationModel.RepetitionFrequency == parameters.RepetitionFrequency && 
                notificationModel.Name == parameters.Name &&
                CheckRecipientIsMappedCorrectly(parameters, roleIds, notificationModel.Ccs) &&
                CheckRecipientIsMappedCorrectly(parameters, roleIds, notificationModel.Receivers)
                ))).Returns(result);
        }

        private void ExpectUpdateReturns(int id, UpdateScheduledNotificationModificationParameters parameters, Result<Advice, OperationError> result)
        {
            _registrationNotificationService.Setup(x => x.Update(id, 
                It.Is<UpdateScheduledNotificationModel>(notificationModel => 
                notificationModel.BaseProperties.Subject == parameters.BaseProperties.Subject && 
                notificationModel.BaseProperties.Body == parameters.BaseProperties.Body && 
                notificationModel.BaseProperties.AdviceType == AdviceType.Repeat &&
                notificationModel.ToDate == parameters.ToDate &&
                notificationModel.Name == parameters.Name))).Returns(result);
        }

        private void ExpectUpdateNotificationUserRelations(int notificationId, UpdateScheduledNotificationModificationParameters parameters, IReadOnlyList<int> roleIds, RelatedEntityType relatedEntityType, Result<Advice, OperationError> result)
        {
            _notificationUserRelationsService.Setup(x => x.UpdateNotificationUserRelations
                (
                    notificationId,
                    It.Is<RecipientModel>(ccs => CheckRecipientIsMappedCorrectly(parameters, roleIds, ccs)),
                    It.Is<RecipientModel>(ccs => CheckRecipientIsMappedCorrectly(parameters, roleIds, ccs)),
                    relatedEntityType
                )
            )
                .Returns(result);
        }

        private static bool CheckRecipientIsMappedCorrectly<T>(T parameters,
            IReadOnlyList<int> roleIds,
            RecipientModel model) where T : class, IHasBaseNotificationPropertiesParameters
        {

            foreach (var recipientModel in model.EmailRecipients)
            {
                if (parameters.BaseProperties.Ccs.EmailRecipients.Select(x => x.Email).Contains(recipientModel.Email))
                {
                    continue;
                }

                if (parameters.BaseProperties.Receivers.EmailRecipients.Select(x => x.Email).Contains(recipientModel.Email))
                {
                    continue;
                }

                return false;
            }

            foreach (var recipientModel in model.RoleRecipients)
            {
                if (roleIds.Contains(recipientModel.RoleId) == false)
                {
                    return false;
                }
            }

            return true;
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
                    var dpr = new DataProcessingRegistration { Id = A<int>(), Uuid = uuid};
                    _dprRepository.Setup(x => x.AsQueryable()).Returns(new List<DataProcessingRegistration>{ dpr }.AsQueryable);
                    return dpr;
                case RelatedEntityType.itContract:
                    var contract = new ItContract { Id = A<int>(), Uuid = uuid };
                    _contractRepository.Setup(x => x.AsQueryable()).Returns(new List<ItContract> { contract }.AsQueryable);
                    return contract;
                case RelatedEntityType.itSystemUsage:
                    var usage = new ItSystemUsage { Id = A<int>(), Uuid = uuid };
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

        private void ExpectAllowReadsReturns(IEntity entity, bool result)
        {
            _authorizationContext.Setup(x => x.AllowReads(entity)).Returns(result);
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

        public static IEnumerable<object[]> AllRelatedEntityTypeAndBoolFlagCombinations()
        {
            var entityTypes = new[] {
                RelatedEntityType.itSystemUsage,
                RelatedEntityType.dataProcessingRegistration,
                RelatedEntityType.itContract
            };

            var flags = new[] { true, false };

            foreach (var entity in entityTypes)
            {
                foreach (var flag in flags)
                {
                    yield return new object[] { entity, flag };
                }
            }
        }
    }
}
