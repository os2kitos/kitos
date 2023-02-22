using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices;
using Core.ApplicationServices.Model.Notification;
using Core.DomainModel.Advice;
using Core.DomainModel.Events;
using Core.DomainServices.Advice;
using Core.DomainServices;
using Infrastructure.Services.DataAccess;
using Core.ApplicationServices.Notification;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Shared;
using Core.DomainServices.Authorization;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.Notification
{
    public class RegistrationNotificationServiceTest: WithAutoFixture
    {
        private readonly Mock<IAdviceService> _adviceService;
        private readonly Mock<IAuthorizationContext> _authorizationContext;
        private readonly Mock<ITransactionManager> _transactionManager;
        private readonly Mock<IGenericRepository<Advice>> _adviceRepository;
        private readonly Mock<IDomainEvents> _domainEventHandler;
        private readonly Mock<IAdviceRootResolution> _adviceRootResolution;

        private readonly RegistrationNotificationService _sut;

        public RegistrationNotificationServiceTest()
        {
            _adviceService = new Mock<IAdviceService>();
            _authorizationContext = new Mock<IAuthorizationContext>();
            _transactionManager = new Mock<ITransactionManager>();
            _adviceRepository = new Mock<IGenericRepository<Advice>>();
            _domainEventHandler = new Mock<IDomainEvents>();
            _adviceRootResolution = new Mock<IAdviceRootResolution>();

            _sut = new RegistrationNotificationService(
                _adviceService.Object,
                _authorizationContext.Object,
                _transactionManager.Object,
                _adviceRepository.Object,
                _domainEventHandler.Object,
                _adviceRootResolution.Object);
        }

        [Fact]
        public void Can_GetCurrentUserNotifications()
        {
            //Arrange
            var notifications = new List<Advice> {new(), new()};

            ExpectGetAdvicesAccessibleToCurrentUserReturns(notifications.AsQueryable());
            //Act
            var result = _sut.GetCurrentUserNotifications();

            //Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Equal(notifications.Count, resultList.Count);
        }

        [Fact]
        public void Can_GetNotificationsByOrganizationId()
        {
            //Arrange
            var orgId = A<int>();
            var notifications = new List<Advice> { new(), new() };

            ExpectGetOrganizationReadAccessLevelReturns(orgId, OrganizationDataReadAccessLevel.All);
            ExpectGetAdvicesForOrgReturns(orgId, notifications.AsQueryable());

            //Act
            var result = _sut.GetNotificationsByOrganizationId(orgId);

            //Assert
            Assert.True(result.Ok);
            var resultList = result.Value.ToList();
            Assert.Equal(notifications.Count, resultList.Count);
        }

        [Theory]
        [InlineData(OrganizationDataReadAccessLevel.None)]
        [InlineData(OrganizationDataReadAccessLevel.Public)]
        [InlineData(OrganizationDataReadAccessLevel.RightsHolder)]
        public void GetNotificationsByOrganizationId_Returns_Forbidden_When_AccessLevel_Is_Not_Sufficient(OrganizationDataReadAccessLevel accessLevel)
        {
            //Arrange
            var orgId = A<int>();

            ExpectGetOrganizationReadAccessLevelReturns(orgId, accessLevel);

            //Act
            var result = _sut.GetNotificationsByOrganizationId(orgId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Can_GetNotificationById()
        {
            //Arrange
            var id = A<int>();
            var notification = new Advice {Id = id};

            ExpectGetByIdReturns(id, notification);

            //Act
            var result = _sut.GetNotificationById(id);

            //Assert
            Assert.True(result.HasValue);
            Assert.Equal(notification.Id, result.Value.Id);
        }

        [Fact]
        public void GetNotificationById_Returns_None_If_NotFound()
        {
            //Arrange
            var id = A<int>();

            ExpectGetByIdReturns(id, Maybe<Advice>.None);

            //Act
            var result = _sut.GetNotificationById(id);

            //Assert
            Assert.True(result.IsNone);
        }

        [Fact]
        public void Can_GetSent()
        {
            //Arrange
            var notifications = new List<Advice> { new(){AdviceSent = new List<AdviceSent> {new ()}}, new(){ AdviceSent = new List<AdviceSent>{new ()} } };

            ExpectGetAdvicesAccessibleToCurrentUserReturns(notifications.AsQueryable());
            //Act
            var result = _sut.GetSent();

            //Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
        }

        [Theory]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.itContract)]
        public void Can_Create(RelatedEntityType relatedEntityType)
        {
            //Arrange
            var model = A<NotificationModel>();
            var entity = CreateEntityWithAdvices(relatedEntityType);

            ExpectResolveRootReturns(model, Maybe<IEntityWithAdvices>.Some(entity));
            ExpectAllowModifyReturns(entity, true);
            var transaction = ExpectDatabaseTransaction();

            //Act
            var result = _sut.Create(model);

            //Assert
            Assert.True(result.Ok);
            _adviceRepository.Verify(x => x.Insert(It.Is<Advice>(advice => advice.RelationId == model.RelationId)), Times.Once);
            _domainEventHandler.Verify(x => x.Raise(It.Is<EntityCreatedEvent<Advice>>(createdEvent => createdEvent.Entity.RelationId == model.RelationId)));
            _adviceRepository.Verify(x => x.Save());

            transaction.Verify(x => x.Commit(),Times.Once);
        }

        [Theory]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.itContract)]
        public void Create_Returns_Forbidden_When_User_Not_Allowed_To_Modify_RootEntity(RelatedEntityType relatedEntityType)
        {
            //Arrange
            var model = A<NotificationModel>();
            var entity = CreateEntityWithAdvices(relatedEntityType);

            ExpectResolveRootReturns(model, Maybe<IEntityWithAdvices>.Some(entity));
            ExpectAllowModifyReturns(entity, false);

            //Act
            var result = _sut.Create(model);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Theory]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.itContract)]
        public void Can_Update(RelatedEntityType relatedEntityType)
        {
            //Arrange
            var id = A<int>();
            var model = A<BaseNotificationModel>();
            var notification = new Advice{Id = id};
            var root = CreateEntityWithAdvices(relatedEntityType);

            var transaction = ExpectDatabaseTransaction();
            ExpectGetByIdReturns(id, notification);
            ExpectAllowModifyReturns(notification, true);
            ExpectResolveRootReturns(model, Maybe<IEntityWithAdvices>.Some(root));

            //Act
            var result = _sut.Update(id, model);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(id, result.Value.Id);

            _adviceRepository.Verify(x => x.Update(It.Is<Advice>(notificationParam => notificationParam.RelationId == model.RelationId)), Times.Once);
            ValidateRootModificationWasCalled(relatedEntityType, root);
            _adviceRepository.Verify(x => x.Save());

            _adviceService.Verify(x => x.UpdateSchedule(notification));

            transaction.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public void Update_Returns_Forbidden_When_Not_Allowed_To_Modify_Root()
        {
            //Arrange
            var id = A<int>();
            var model = A<BaseNotificationModel>();
            var notification = new Advice();

            ExpectDatabaseTransaction();
            ExpectGetByIdReturns(id, notification);
            ExpectAllowModifyReturns(notification, false);

            //Act
            var result = _sut.Update(id, model);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Update_Returns_NotFound_When_NotificationId_NotFound()
        {
            //Arrange
            var id = A<int>();
            var model = A<BaseNotificationModel>();

            ExpectDatabaseTransaction();
            ExpectGetByIdReturns(id, Maybe<Advice>.None);

            //Act
            var result = _sut.Update(id, model);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Theory]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.itContract)]
        public void Can_Delete(RelatedEntityType relatedEntityType)
        {
            //Arrange
            var id = A<int>();
            var notification = new Advice{Id = id, IsActive = false};
            var root = CreateEntityWithAdvices(relatedEntityType);

            var transaction = ExpectDatabaseTransaction();
            ExpectGetByIdReturns(id, notification);
            ExpectAllowDeleteReturns(notification, true);
            ExpectResolveRootReturns(id, Maybe<IEntityWithAdvices>.Some(root));

            //Act
            var error = _sut.Delete(id);

            //Assert
            Assert.False(error.HasValue);

            ValidateRootModificationWasCalled(relatedEntityType, root);
            _adviceService.Verify(x => x.Delete(notification), Times.Once);
            transaction.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public void Delete_Returns_BadState_If_Entity_Cannot_Be_Deleted()
        {
            //Arrange
            var id = A<int>();
            var notification = new Advice{Id = id, IsActive = true};

            ExpectDatabaseTransaction();
            ExpectGetByIdReturns(id, notification);
            ExpectAllowDeleteReturns(notification, true);

            //Act
            var error = _sut.Delete(id);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.BadState, error.Value.FailureType);
        }

        [Fact]
        public void Delete_Returns_Forbidden_If_Entity_Is_Not_Allowed_To_Be_Deleted()
        {
            //Arrange
            var id = A<int>();
            var notification = new Advice();

            ExpectDatabaseTransaction();
            ExpectGetByIdReturns(id, notification);
            ExpectAllowDeleteReturns(notification, false);

            //Act
            var error = _sut.Delete(id);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.Forbidden, error.Value.FailureType);
        }

        [Fact]
        public void Delete_Returns_NotFound_If_Entity_Is_Not_Found()
        {
            //Arrange
            var id = A<int>();

            ExpectDatabaseTransaction();
            ExpectGetByIdReturns(id, Maybe<Advice>.None);

            //Act
            var error = _sut.Delete(id);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.NotFound, error.Value.FailureType);
        }

        [Theory]
        [InlineData(RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.itContract)]
        public void Can_Deactivate(RelatedEntityType relatedEntityType)
        {
            //Arrange
            var id = A<int>();
            var notification = new Advice{Id = id, IsActive = false};
            var root = CreateEntityWithAdvices(relatedEntityType);

            var transaction = ExpectDatabaseTransaction();
            ExpectGetByIdReturns(id, notification);
            ExpectAllowModifyReturns(notification, true);
            ExpectResolveRootReturns(id, Maybe<IEntityWithAdvices>.Some(root));

            //Act
            var result = _sut.DeactivateNotification(id);

            //Assert
            Assert.True(result.Ok);
            Assert.False(result.Value.IsActive);

            _adviceService.Verify(x => x.Deactivate(notification), Times.Once);
            ValidateRootModificationWasCalled(relatedEntityType, root);
            transaction.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public void Deactivate_Returns_Forbidden_If_Entity_Is_Not_Allowed_To_Be_Deleted()
        {
            //Arrange
            var id = A<int>();
            var notification = new Advice();

            ExpectDatabaseTransaction();
            ExpectGetByIdReturns(id, notification);
            ExpectAllowModifyReturns(notification, false);

            //Act
            var result = _sut.DeactivateNotification(id);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Deactivate_Returns_NotFound_If_Entity_Is_Not_Found()
        {
            //Arrange
            var id = A<int>();

            ExpectDatabaseTransaction();
            ExpectGetByIdReturns(id, Maybe<Advice>.None);

            //Act
            var result = _sut.DeactivateNotification(id);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        private Mock<IDatabaseTransaction> ExpectDatabaseTransaction()
        {
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            return transaction;
        }

        private void ExpectResolveRootReturns(BaseNotificationModel notification, Maybe<IEntityWithAdvices> result)
        {
            _adviceRootResolution.Setup(x => x.Resolve(It.Is<Advice>(advice => advice.RelationId == notification.RelationId))).Returns(result);
        }

        private void ExpectResolveRootReturns(int id, Maybe<IEntityWithAdvices> result)
        {
            _adviceRootResolution.Setup(x => x.Resolve(It.Is<Advice>(advice => advice.Id == id))).Returns(result);
        }

        private void ExpectAllowModifyReturns(IEntity entity, bool result)
        {
            _authorizationContext.Setup(x => x.AllowModify(entity)).Returns(result);
        }

        private void ExpectAllowDeleteReturns(IEntity entity, bool result)
        {
            _authorizationContext.Setup(x => x.AllowDelete(entity)).Returns(result);
        }

        private void ExpectGetByIdReturns(int id, Maybe<Advice> result)
        {
            _adviceService.Setup(x => x.GetAdviceById(id)).Returns(result);
        }

        private void ExpectGetAdvicesForOrgReturns(int orgId, IQueryable<Advice> result)
        {
            _adviceService.Setup(x => x.GetAdvicesForOrg(orgId)).Returns(result);
        }

        private void ExpectGetOrganizationReadAccessLevelReturns(int orgId, OrganizationDataReadAccessLevel result)
        {
            _authorizationContext.Setup(x => x.GetOrganizationReadAccessLevel(orgId)).Returns(result);
        }

        private void ExpectGetAdvicesAccessibleToCurrentUserReturns(IQueryable<Advice> result)
        {
            _adviceService.Setup(x => x.GetAdvicesAccessibleToCurrentUser()).Returns(result);
        }

        private static IEntityWithAdvices CreateEntityWithAdvices(RelatedEntityType relatedEntityType)
        {
            return relatedEntityType switch
            {
                RelatedEntityType.dataProcessingRegistration => new DataProcessingRegistration(),
                RelatedEntityType.itSystemUsage => new ItSystemUsage(),
                RelatedEntityType.itContract => new ItContract(),
                _ => throw new ArgumentOutOfRangeException(nameof(relatedEntityType), relatedEntityType, null)
            };
        }

        private void ValidateRootModificationWasCalled(RelatedEntityType relatedEntityType, IEntityWithAdvices entity)
        {
            switch (relatedEntityType)
            {
                case RelatedEntityType.dataProcessingRegistration:
                    _domainEventHandler.Verify(x => x.Raise(It.Is<EntityUpdatedEvent<DataProcessingRegistration>>(createdEvent => createdEvent.Entity == entity)));
                    break;
                case RelatedEntityType.itSystemUsage:
                    _domainEventHandler.Verify(x => x.Raise(It.Is<EntityUpdatedEvent<ItSystemUsage>>(createdEvent => createdEvent.Entity == entity)));
                    break;
                case RelatedEntityType.itContract:
                    _domainEventHandler.Verify(x => x.Raise(It.Is<EntityUpdatedEvent<ItContract>>(createdEvent => createdEvent.Entity == entity)));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(relatedEntityType), relatedEntityType, null);
            }
        }
    }
}
