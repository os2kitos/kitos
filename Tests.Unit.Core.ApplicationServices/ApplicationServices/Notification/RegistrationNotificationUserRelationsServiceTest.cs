using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Notification;
using Core.ApplicationServices.Notification;
using Core.DomainModel;
using Core.DomainModel.Advice;
using Core.DomainServices;
using Infrastructure.Services.DataAccess;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.Notification
{
    public class RegistrationNotificationUserRelationsServiceTest : WithAutoFixture
    {
        private readonly Mock<IGenericRepository<AdviceUserRelation>> _adviceUserRelationRepository;
        private readonly Mock<IRegistrationNotificationService> _registrationNotificationService;
        private readonly Mock<ITransactionManager> _transactionManager;
        private readonly Mock<IAuthorizationContext> _authorizationContext;

        private readonly RegistrationNotificationUserRelationsService _sut;

        public RegistrationNotificationUserRelationsServiceTest()
        {
            _adviceUserRelationRepository = new Mock<IGenericRepository<AdviceUserRelation>>();
            _registrationNotificationService = new Mock<IRegistrationNotificationService>();
            _transactionManager = new Mock<ITransactionManager>();
            _authorizationContext = new Mock<IAuthorizationContext>();

            _sut = new RegistrationNotificationUserRelationsService(
                _adviceUserRelationRepository.Object,
                _registrationNotificationService.Object,
                _transactionManager.Object,
                _authorizationContext.Object);
        }

        [Fact]
        public void Can_UpdateNotificationUserRelations()
        {
            //Arrange
            var notificationId = A<int>();
            var models = A<IReadOnlyList<RecipientModel>>();
            var notification = new Advice();
            var userRelations = new List<AdviceUserRelation>{ new() {AdviceId = notificationId}, new() { AdviceId = notificationId } };

            var transaction = ExpectDatabaseTransaction();
            ExpectGetNotificationByIdReturns(notificationId, notification);
            ExpectAllowModifyReturns(notification, true);
            ExpectUserRelationsAsQueryableReturns(userRelations.AsQueryable());
            ExpectAllowDeleteUserRelationsReturns(userRelations, true);

            //Act
            var error = _sut.UpdateNotificationUserRelations(notificationId, models);

            //Assert
            Assert.False(error.HasValue);
            _adviceUserRelationRepository.Verify(x => x.AddRange(It.Is<IEnumerable<AdviceUserRelation>>(entities =>
                    VerifyUserRelationsAreCorrect(models, entities))), Times.Once);
            _adviceUserRelationRepository.Verify(x => x.Save(), Times.Exactly(userRelations.Count + 1));
            transaction.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public void UpdateNotificationUserRelations_Returns_Forbidden_When_NotAllowed_To_Delete_Relation()
        {
            //Arrange
            var notificationId = A<int>();
            var models = A<IReadOnlyList<RecipientModel>>();
            var notification = new Advice();
            var userRelations = new List<AdviceUserRelation>{ new() {AdviceId = notificationId}, new() { AdviceId = notificationId } };

            ExpectDatabaseTransaction();
            ExpectGetNotificationByIdReturns(notificationId, notification);
            ExpectAllowModifyReturns(notification, true);
            ExpectUserRelationsAsQueryableReturns(userRelations.AsQueryable());
            ExpectAllowDeleteUserRelationsReturns(userRelations, false);

            //Act
            var error = _sut.UpdateNotificationUserRelations(notificationId, models);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.Forbidden, error.Value.FailureType);
        }

        [Fact]
        public void UpdateNotificationUserRelations_Returns_Forbidden_When_NotAllowed_To_Modify_Relation()
        {
            //Arrange
            var notificationId = A<int>();
            var models = A<IReadOnlyList<RecipientModel>>();
            var notification = new Advice();

            ExpectDatabaseTransaction();
            ExpectGetNotificationByIdReturns(notificationId, notification);
            ExpectAllowModifyReturns(notification, false);

            //Act
            var error = _sut.UpdateNotificationUserRelations(notificationId, models);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.Forbidden, error.Value.FailureType);
        }

        [Fact]
        public void UpdateNotificationUserRelations_Returns_NotFound_When_Notification_NotFound()
        {
            //Arrange
            var notificationId = A<int>();
            var models = A<IReadOnlyList<RecipientModel>>();

            ExpectDatabaseTransaction();
            ExpectGetNotificationByIdReturns(notificationId, Maybe<Advice>.None);

            //Act
            var error = _sut.UpdateNotificationUserRelations(notificationId, models);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.NotFound, error.Value.FailureType);
        }

        private static bool VerifyUserRelationsAreCorrect(IEnumerable<RecipientModel> models, IEnumerable<AdviceUserRelation> relations)
        {
            var relationsList = relations.ToList();
            foreach (var model in models)
            {
                if(relationsList.Any(x => x.Email == model.Email))
                    continue;
                if(relationsList.Any(x => x.DataProcessingRegistrationRoleId == model.DataProcessingRegistrationRoleId))
                    continue;
                if(relationsList.Any(x => x.ItContractRoleId == model.ItContractRoleId))
                    continue;
                if(relationsList.Any(x => x.ItSystemRoleId == model.ItSystemRoleId))
                    continue;

                return false;
            }

            return true;
        }

        private void ExpectAllowDeleteUserRelationsReturns(IEnumerable<AdviceUserRelation> list, bool result)
        {
            foreach (var adviceUserRelation in list)
            {
                ExpectAllowDeleteReturns(adviceUserRelation, result);
            }
        }
        
        private void ExpectUserRelationsAsQueryableReturns(IQueryable<AdviceUserRelation> result)
        {
            _adviceUserRelationRepository.Setup(x => x.AsQueryable()).Returns(result);
        }

        private void ExpectAllowDeleteReturns(IEntity entity, bool result)
        {
            _authorizationContext.Setup(x => x.AllowDelete(entity)).Returns(result);
        }

        private void ExpectAllowModifyReturns(IEntity entity, bool result)
        {
            _authorizationContext.Setup(x => x.AllowModify(entity)).Returns(result);
        }

        private void ExpectGetNotificationByIdReturns(int id, Maybe<Advice> result)
        {
            _registrationNotificationService.Setup(x => x.GetNotificationById(id)).Returns(result);
        }

        private Mock<IDatabaseTransaction> ExpectDatabaseTransaction()
        {
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            return transaction;
        }
    }
}
