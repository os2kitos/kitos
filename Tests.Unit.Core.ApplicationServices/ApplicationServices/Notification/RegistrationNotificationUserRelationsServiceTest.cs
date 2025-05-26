using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Notification;
using Core.ApplicationServices.Notification;
using Core.DomainModel;
using Core.DomainModel.Advice;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Shared;
using Core.DomainServices;
using Core.DomainServices.Advice;
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
        private readonly Mock<IAdviceRootResolution> _adviceRootResolution;

        private readonly RegistrationNotificationUserRelationsService _sut;

        public RegistrationNotificationUserRelationsServiceTest()
        {
            _adviceUserRelationRepository = new Mock<IGenericRepository<AdviceUserRelation>>();
            _registrationNotificationService = new Mock<IRegistrationNotificationService>();
            _transactionManager = new Mock<ITransactionManager>();
            _authorizationContext = new Mock<IAuthorizationContext>();
            _adviceRootResolution = new Mock<IAdviceRootResolution>();

            _sut = new RegistrationNotificationUserRelationsService(
                _adviceUserRelationRepository.Object,
                _registrationNotificationService.Object,
                _transactionManager.Object,
                _authorizationContext.Object,
                _adviceRootResolution.Object);
        }

        [Fact]
        public void Can_UpdateNotificationUserRelations()
        {
            //Arrange
            var notificationId = A<int>();
            var ccs = A<RecipientModel>();
            var receivers = A<RecipientModel>();
            var notification = new Advice{RelationId = A<int>()};
            var userRelations = new List<AdviceUserRelation>{ new() {AdviceId = notificationId}, new() { AdviceId = notificationId } };
            var relatedEntityType = A<RelatedEntityType>();
            var root = CreateEntityWithAdvices(relatedEntityType);

            var transaction = ExpectDatabaseTransaction();
            ExpectGetNotificationByIdReturns(notificationId, notification);
            ExpectResolveRootReturns(notification.RelationId.Value, Maybe<IEntityWithAdvices>.Some(root));
            ExpectAllowModifyReturns(root, true);
            ExpectUserRelationsAsQueryableReturns(userRelations.AsQueryable());
            ExpectAllowDeleteUserRelationsReturns(userRelations, true);

            //Act
            var result = _sut.UpdateNotificationUserRelations(notificationId, ccs, receivers, relatedEntityType);

            //Assert
            Assert.True(result.Ok);
            _adviceUserRelationRepository.Verify(x => x.AddRange(It.Is<IEnumerable<AdviceUserRelation>>(entities =>
                    VerifyUserRelationsAreCorrect(ccs, receivers, entities))), Times.Once);
            _adviceUserRelationRepository.Verify(x => x.Save(), Times.Once);
            transaction.Verify(x => x.Commit(), Times.Once);
        }
        
        [Fact]
        public void UpdateNotificationUserRelations_Returns_Forbidden_When_NotAllowed_To_Modify_Relation()
        {
            //Arrange
            var notificationId = A<int>();
            var ccs = A<RecipientModel>();
            var receivers = A<RecipientModel>();
            var notification = new Advice{RelationId = A<int>()};
            var relatedEntityType = A<RelatedEntityType>();
            var root = CreateEntityWithAdvices(relatedEntityType);

            ExpectDatabaseTransaction();
            ExpectGetNotificationByIdReturns(notificationId, notification);
            ExpectResolveRootReturns(notification.RelationId.Value, Maybe<IEntityWithAdvices>.Some(root));
            ExpectAllowModifyReturns(root, false);

            //Act
            var result = _sut.UpdateNotificationUserRelations(notificationId, ccs, receivers, relatedEntityType);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }
        
        [Fact]
        public void UpdateNotificationUserRelations_Returns_NotFound_When_Root_Entity_Was_Not_Found()
        {
            //Arrange
            var notificationId = A<int>();
            var ccs = A<RecipientModel>();
            var receivers = A<RecipientModel>();
            var notification = new Advice{RelationId = A<int>()};
            var relatedEntityType = A<RelatedEntityType>();

            ExpectDatabaseTransaction();
            ExpectGetNotificationByIdReturns(notificationId, notification);
            ExpectResolveRootReturns(notification.RelationId.Value, Maybe<IEntityWithAdvices>.None);

            //Act
            var result = _sut.UpdateNotificationUserRelations(notificationId, ccs, receivers, relatedEntityType);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Fact]
        public void UpdateNotificationUserRelations_Returns_Error_When_GetNotification_Fails()
        {
            //Arrange
            var notificationId = A<int>();
            var ccs = A<RecipientModel>();
            var receivers = A<RecipientModel>();
            var relatedEntityType = A<RelatedEntityType>();
            var expectedError = A<OperationError>();

            ExpectDatabaseTransaction();
            ExpectGetNotificationByIdReturns(notificationId, expectedError);

            //Act
            var result = _sut.UpdateNotificationUserRelations(notificationId, ccs, receivers, relatedEntityType);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(expectedError.FailureType, result.Error.FailureType);
        }

        private static bool VerifyUserRelationsAreCorrect(RecipientModel ccs, RecipientModel receivers, IEnumerable<AdviceUserRelation> relations)
        {
            var relationsList = relations.ToList();

            var emailResult = VerifyEmailsAreCorrect(ccs.EmailRecipients, relationsList) &&
                              VerifyEmailsAreCorrect(receivers.EmailRecipients, relationsList);
            var roleResult = VerifyRolesAreCorrect(ccs.RoleRecipients, relationsList) &&
                              VerifyRolesAreCorrect(receivers.RoleRecipients, relationsList);

            return emailResult && roleResult;
        }

        private static bool VerifyEmailsAreCorrect(IEnumerable<EmailRecipientModel> recipients,
            IEnumerable<AdviceUserRelation> relations)
        {
            var relationsList = relations.ToList();
            foreach (var model in recipients)
            {
                if (relationsList.Any(x => x.Email == model.Email))
                    continue;

                return false;
            }

            return true;
        }

        private static bool VerifyRolesAreCorrect(IEnumerable<RoleRecipientModel> recipients,
            IEnumerable<AdviceUserRelation> relations)
        {
            var relationsList = relations.ToList();
            foreach (var model in recipients)
            {
                if (relationsList.Any(x => x.DataProcessingRegistrationRoleId == model.RoleId))
                    continue;
                if (relationsList.Any(x => x.ItContractRoleId == model.RoleId))
                    continue;
                if (relationsList.Any(x => x.ItSystemRoleId == model.RoleId))
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

        private void ExpectGetNotificationByIdReturns(int id, Result<Advice, OperationError> result)
        {
            _registrationNotificationService.Setup(x => x.GetNotificationById(id)).Returns(result);
        }

        private Mock<IDatabaseTransaction> ExpectDatabaseTransaction()
        {
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            return transaction;
        }

        private void ExpectResolveRootReturns(int relationId, Maybe<IEntityWithAdvices> result)
        {
            _adviceRootResolution.Setup(x => x.Resolve(It.Is<Advice>(advice => advice.RelationId == relationId))).Returns(result);
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
    }
}
