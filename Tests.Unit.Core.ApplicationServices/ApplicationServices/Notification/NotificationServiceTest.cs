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
            ExpectRoleRecipientIds(parameters);
            ExpectAllowModifyReturns(relatedEntity, true);
            ExpectCreateReturns(parameters, AdviceType.Immediate, notification);

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
            ExpectRoleRecipientIds(parameters);
            ExpectAllowModifyReturns(relatedEntity, true);
            ExpectCreateReturns(parameters, AdviceType.Immediate, notification);

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

        private ImmediateNotificationModificationParameters CreateNewParameters(RelatedEntityType relatedEntityType)
        {
            return new ImmediateNotificationModificationParameters(A<string>(), A<string>(), relatedEntityType, A<Guid>(), A<RootRecipientModificationParameters>(), A<RootRecipientModificationParameters>());
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

        private void ExpectRoleRecipientIds(ImmediateNotificationModificationParameters parameters)
        {
            foreach (var ccsRoleRecipient in parameters.Ccs.RoleRecipients)
            {
                ExpectResolveRoleIdReturns(ccsRoleRecipient.RoleUuid, parameters.Type, A<int>());
            }
            foreach (var receiversRoleRecipient in parameters.Receivers.RoleRecipients)
            {
                ExpectResolveRoleIdReturns(receiversRoleRecipient.RoleUuid, parameters.Type, A<int>());
            }
        }

        private void ExpectResolveIdReturns<T>(Guid uuid, Maybe<int> result) where T: class, IHasUuid, IHasId
        {
            _entityIdentityResolver.Setup(x => x.ResolveDbId<T>(uuid)).Returns(result);
        }

        private void ExpectResolveUuidReturns<T>(int id, Maybe<Guid> result) where T: class, IHasUuid, IHasId
        {
            _entityIdentityResolver.Setup(x => x.ResolveUuid<T>(id)).Returns(result);
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

        private void ExpectCreateReturns(ImmediateNotificationModificationParameters parameters, AdviceType adviceType, Result<Advice, OperationError> result)
        {
            _registrationNotificationService.Setup(x => x.Create(It.Is<NotificationModel>(notificationModel => 
                notificationModel.Subject == parameters.Subject && 
                notificationModel.Body == parameters.Body && 
                notificationModel.AdviceType == adviceType))).Returns(result);
        }

        private void ExpectCreateReturns(ScheduledNotificationModificationParameters parameters, AdviceType adviceType, Result<Advice, OperationError> result)
        {
            _registrationNotificationService.Setup(x => x.Create(It.Is<NotificationModel>(notificationModel => 
                notificationModel.Subject == parameters.Subject && 
                notificationModel.Body == parameters.Body && 
                notificationModel.AdviceType == adviceType &&
                notificationModel.FromDate == parameters.FromDate &&
                notificationModel.ToDate == parameters.ToDate &&
                notificationModel.RepetitionFrequency == parameters.RepetitionFrequency && 
                notificationModel.Name == parameters.Name))).Returns(result);
        }

        private void ExpectUpdateReturns(int notificationId, UpdateNotificationModel model, Result<Advice, OperationError> result)
        {
            _registrationNotificationService.Setup(x => x.Update(notificationId, model)).Returns(result);
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
