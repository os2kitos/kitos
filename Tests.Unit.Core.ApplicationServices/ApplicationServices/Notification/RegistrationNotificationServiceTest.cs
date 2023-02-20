using System.Collections.Generic;
using System.Linq;
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
        private readonly Mock<IGenericRepository<AdviceUserRelation>> _adviceUserRelationRepository;
        private readonly Mock<IDomainEvents> _domainEventHandler;
        private readonly Mock<IAdviceRootResolution> _adviceRootResolution;

        private readonly RegistrationNotificationService _sut;

        public RegistrationNotificationServiceTest()
        {
            _adviceService = new Mock<IAdviceService>();
            _authorizationContext = new Mock<IAuthorizationContext>();
            _transactionManager = new Mock<ITransactionManager>();
            _adviceRepository = new Mock<IGenericRepository<Advice>>();
            _adviceUserRelationRepository = new Mock<IGenericRepository<AdviceUserRelation>>();
            _domainEventHandler = new Mock<IDomainEvents>();
            _adviceRootResolution = new Mock<IAdviceRootResolution>();

            _sut = new RegistrationNotificationService(
                _adviceService.Object,
                _authorizationContext.Object,
                _transactionManager.Object,
                _adviceRepository.Object,
                _domainEventHandler.Object,
                _adviceRootResolution.Object,
                _adviceUserRelationRepository.Object);
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

        [Fact]
        public void Can_Create()
        {
            var model = A<NotificationModel>();
            var entity = A<IEntityWithAdvices>();

            ExpectResolveRootReturns(It.Is<Advice>(x => x.RelationId == model.RelationId), Maybe<IEntityWithAdvices>.Some(entity));
            ExpectAllowModifyReturns(entity, true);
            var transaction = ExpectDatabaseTransaction();

            _adviceRepository.Verify(x => x.Insert(It.Is<Advice>(x => x.RelationId == model.RelationId)), Times.Once);
            transaction.Verify(x => x.Commit(),Times.Once);

        }


        private Mock<IDatabaseTransaction> ExpectDatabaseTransaction()
        {
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            return transaction;
        }
        private void ExpectResolveRootReturns(Advice notification, Maybe<IEntityWithAdvices> result)
        {
            _adviceRootResolution.Setup(x => x.Resolve(notification)).Returns(result);
        }

        private void ExpectAllowModifyReturns(IEntity entity, bool result)
        {
            _authorizationContext.Setup(x => x.AllowModify(entity)).Returns(result);
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

    }
}
