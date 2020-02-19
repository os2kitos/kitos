using System;
using System.Data;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.References;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.References;
using Core.DomainModel.Result;
using Core.DomainServices.Repositories.Reference;
using Core.DomainServices.Repositories.System;
using Core.DomainServices.Time;
using Infrastructure.Services.DataAccess;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Services
{
    public class ReferenceServiceTest : WithAutoFixture
    {
        private readonly ReferenceService _sut;
        private readonly Mock<IReferenceRepository> _referenceRepository;
        private readonly Mock<IItSystemRepository> _systemRepository;
        private readonly Mock<IAuthorizationContext> _authorizationContext;
        private readonly Mock<ITransactionManager> _transactionManager;
        private readonly Mock<IDatabaseTransaction> _dbTransaction;

        public ReferenceServiceTest()
        {
            _referenceRepository = new Mock<IReferenceRepository>();
            _systemRepository = new Mock<IItSystemRepository>();
            _authorizationContext = new Mock<IAuthorizationContext>();
            _transactionManager = new Mock<ITransactionManager>();
            _dbTransaction = new Mock<IDatabaseTransaction>();

            _sut = new ReferenceService(
                _referenceRepository.Object,
                _systemRepository.Object,
                _authorizationContext.Object,
                _transactionManager.Object,
                new Mock<IOrganizationalUserContext>().Object,
                Mock.Of<IOperationClock>(x => x.Now == DateTime.Now)
            );
        }

        [Fact]
        public void Delete_Returns_NotFound()
        {
            //Arrange
            var system = CreateSystem();
            ExpectGetSystemReturns(system.Id, null);

            //Act
            var result = _sut.DeleteBySystemId(system.Id);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.NotFound, result.Error);
            _dbTransaction.Verify(x => x.Rollback(), Times.Never);
            _dbTransaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Delete_Returns_Forbidden_If_Not_Allowed_To_Modify_System()
        {
            //Arrange
            var system = CreateSystem();
            ExpectGetSystemReturns(system.Id, system);
            ExpectAllowModifyReturns(system, false);

            //Act
            var result = _sut.DeleteBySystemId(system.Id);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
            _dbTransaction.Verify(x => x.Rollback(), Times.Never);
            _dbTransaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Delete_Returns_Ok_If_No_References()
        {
            //Arrange
            var system = CreateSystem();
            ExpectGetSystemReturns(system.Id, system);
            ExpectAllowModifyReturns(system, true);

            //Act
            var result = _sut.DeleteBySystemId(system.Id);

            //Assert
            Assert.True(result.Ok);
            _dbTransaction.Verify(x => x.Rollback(), Times.Never);
            _dbTransaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Delete_Returns_Ok_If_References()
        {
            //Arrange
            var system = CreateSystem();
            var reference = CreateReference();
            system = AddExternalReference(system, reference);
            ExpectGetSystemReturns(system.Id, system);
            ExpectAllowModifyReturns(system, true);
            ExpectTransactionToBeSet();

            //Act
            var result = _sut.DeleteBySystemId(system.Id);

            //Assert
            Assert.True(result.Ok);
            _dbTransaction.Verify(x => x.Rollback(), Times.Never);
            _dbTransaction.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public void AddReference_Returns_NotFound_If_Root_Is_NotFound()
        {
            //Arrange
            var id = A<int>();
            var rootType = A<ReferenceRootType>();
            ExpectGetRootEntityReturns(id, rootType, Maybe<IEntityWithExternalReferences>.None);

            //Act
            var result = _sut.AddReference(id, rootType, A<string>(), A<string>(), A<string>(), A<Display>());

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(new OperationError("Root entity could not be found", OperationFailure.NotFound), result.Error);
        }

        [Fact]
        public void AddReference_Returns_Forbidden_If_Modification_Of_Root_Is_Denied()
        {
            //Arrange
            var id = A<int>();
            var rootType = A<ReferenceRootType>();
            var entity = new Mock<IEntityWithExternalReferences>();
            ExpectGetRootEntityReturns(id, rootType, Maybe<IEntityWithExternalReferences>.Some(entity.Object));
            ExpectAllowModifyReturns(entity.Object, false);

            //Act
            var result = _sut.AddReference(id, rootType, A<string>(), A<string>(), A<string>(), A<Display>());

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(new OperationError("Not allowed to modify root entity", OperationFailure.Forbidden), result.Error);
        }

        [Fact]
        public void AddReference_Saves_Root_If_Add_Succeeds()
        {
            //Arrange
            var id = A<int>();
            var rootType = A<ReferenceRootType>();
            var title = A<string>();
            var externalReferenceId = A<string>();
            var url = A<string>();
            var display = A<Display>();
            var entity = new Mock<IEntityWithExternalReferences>();
            ExpectGetRootEntityReturns(id, rootType, Maybe<IEntityWithExternalReferences>.Some(entity.Object));
            ExpectAllowModifyReturns(entity.Object, true);
            ExpectAddReferenceReturns(entity, title, externalReferenceId, url, display, new ExternalReference());

            //Act
            var result = _sut.AddReference(id, rootType, title, externalReferenceId, url, display);

            //Assert
            Assert.True(result.Ok);
            _referenceRepository.Verify(x => x.SaveRootEntity(entity.Object), Times.Once);
        }

        [Fact]
        public void AddReference_Does_Not_Save_Root_If_Add_Fails()
        {
            //Arrange
            var id = A<int>();
            var rootType = A<ReferenceRootType>();
            var title = A<string>();
            var externalReferenceId = A<string>();
            var url = A<string>();
            var display = A<Display>();
            var entity = new Mock<IEntityWithExternalReferences>();
            ExpectGetRootEntityReturns(id, rootType, Maybe<IEntityWithExternalReferences>.Some(entity.Object));
            ExpectAllowModifyReturns(entity.Object, true);
            var operationError = A<OperationError>();
            ExpectAddReferenceReturns(entity, title, externalReferenceId, url, display, operationError);

            //Act
            var result = _sut.AddReference(id, rootType, title, externalReferenceId, url, display);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            _referenceRepository.Verify(x => x.SaveRootEntity(entity.Object), Times.Never);
        }

        private static void ExpectAddReferenceReturns(Mock<IEntityWithExternalReferences> entity, string title, string externalReferenceId, string url,
            Display display, Result<ExternalReference, OperationError> result)
        {
            entity.Setup(x => x.AddExternalReference(It.Is<ExternalReference>(er =>
                    er.Title == title && er.ExternalReferenceId == externalReferenceId && er.URL == url &&
                    er.Display == display)))
                .Returns(result);
        }

        private void ExpectGetRootEntityReturns(int id, ReferenceRootType rootType, Maybe<IEntityWithExternalReferences> value)
        {
            _referenceRepository.Setup(x => x.GetRootEntity(id, rootType)).Returns(value);
        }

        private void ExpectAllowModifyReturns(IEntity system, bool value)
        {
            _authorizationContext.Setup(x => x.AllowModify(system)).Returns(value);
        }

        private void ExpectGetSystemReturns(int id, ItSystem system)
        {
            _systemRepository.Setup(x => x.GetSystem(id)).Returns(system);
        }
        private void ExpectTransactionToBeSet()
        {
            _transactionManager.Setup(x => x.Begin(IsolationLevel.Serializable)).Returns(_dbTransaction.Object);
        }

        private ItSystem CreateSystem()
        {
            return new ItSystem { Id = A<int>() };
        }
        private ExternalReference CreateReference()
        {
            return new ExternalReference { Id = A<int>() };
        }

        private static ItSystem AddExternalReference(ItSystem system, ExternalReference reference)
        {
            system.ExternalReferences.Add(reference);
            return system;
        }
    }
}