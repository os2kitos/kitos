using System;
using System.Data;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.References;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.References;
using Core.DomainModel.Result;
using Core.DomainServices.Repositories.Contract;
using Core.DomainServices.Repositories.GDPR;
using Core.DomainServices.Repositories.Project;
using Core.DomainServices.Repositories.Reference;
using Core.DomainServices.Repositories.System;
using Core.DomainServices.Repositories.SystemUsage;
using Core.DomainServices.Time;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.DomainEvents;
using Infrastructure.Services.Types;
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
        private readonly Mock<IItSystemUsageRepository> _systemUsageRepository;
        private readonly Mock<IItContractRepository> _contractRepository;
        private readonly Mock<IItProjectRepository> _projectRepository;

        public ReferenceServiceTest()
        {
            _referenceRepository = new Mock<IReferenceRepository>();
            _systemRepository = new Mock<IItSystemRepository>();
            _authorizationContext = new Mock<IAuthorizationContext>();
            _transactionManager = new Mock<ITransactionManager>();
            _dbTransaction = new Mock<IDatabaseTransaction>();

            _systemUsageRepository = new Mock<IItSystemUsageRepository>();
            _contractRepository = new Mock<IItContractRepository>();
            _projectRepository = new Mock<IItProjectRepository>();
            _sut = new ReferenceService(
                _referenceRepository.Object,
                _systemRepository.Object,
                _systemUsageRepository.Object,
                _contractRepository.Object, 
                _projectRepository.Object, 
                Mock.Of<IDataProcessingRegistrationRepository>(),
                _authorizationContext.Object,
                _transactionManager.Object,
                Mock.Of<IOperationClock>(x => x.Now == DateTime.Now),
                Mock.Of<IDomainEvents>()
            );
        }

        [Fact]
        public void DeleteBySystemId_Returns_NotFound()
        {
            //Arrange
            var id = A<int>();
            ExpectGetSystemReturns(id, null);

            //Act
            var result = _sut.DeleteBySystemId(id);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.NotFound, result.Error);
            _dbTransaction.Verify(x => x.Rollback(), Times.Never);
            _dbTransaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void DeleteBySystemId_Returns_Forbidden_If_Not_Allowed_To_Modify_System()
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
        public void DeleteBySystemId_Returns_Ok_If_No_References()
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
        public void DeleteBySystemId_Returns_Ok_If_References()
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
        public void DeleteBySystemUsageId_Returns_NotFound()
        {
            //Arrange
            var id = A<int>();
            ExpectGetSystemUsageReturns(id, null);

            //Act
            var result = _sut.DeleteBySystemUsageId(id);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.NotFound, result.Error);
            _dbTransaction.Verify(x => x.Rollback(), Times.Never);
            _dbTransaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void DeleteBySystemUsageId_Returns_Forbidden_If_Not_Allowed_To_Modify_System()
        {
            //Arrange
            var systemUsage = CreateSystemUsage();
            ExpectGetSystemUsageReturns(systemUsage.Id, systemUsage);
            ExpectAllowModifyReturns(systemUsage, false);

            //Act
            var result = _sut.DeleteBySystemUsageId(systemUsage.Id);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
            _dbTransaction.Verify(x => x.Rollback(), Times.Never);
            _dbTransaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void DeleteBySystemUsageId_Returns_Ok_If_No_References()
        {
            var systemUsage = CreateSystemUsage();
            ExpectGetSystemUsageReturns(systemUsage.Id, systemUsage);
            ExpectAllowModifyReturns(systemUsage, true);

            //Act
            var result = _sut.DeleteBySystemUsageId(systemUsage.Id);

            //Assert
            Assert.True(result.Ok);
            _dbTransaction.Verify(x => x.Rollback(), Times.Never);
            _dbTransaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void DeleteBySystemUsageId_Returns_Ok_If_References()
        {
            //Arrange
            var systemUsage = CreateSystemUsage();
            var reference = CreateReference();
            systemUsage = AddExternalReference(systemUsage, reference);
            ExpectGetSystemUsageReturns(systemUsage.Id, systemUsage);
            ExpectAllowModifyReturns(systemUsage, true);
            ExpectTransactionToBeSet();

            //Act
            var result = _sut.DeleteBySystemUsageId(systemUsage.Id);

            //Assert
            Assert.True(result.Ok);
            _dbTransaction.Verify(x => x.Rollback(), Times.Never);
            _dbTransaction.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public void DeleteByContractId_Returns_NotFound()
        {
            //Arrange
            var id = A<int>();
            ExpectGetContractReturns(id, null);

            //Act
            var result = _sut.DeleteByContractId(id);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.NotFound, result.Error);
            _dbTransaction.Verify(x => x.Rollback(), Times.Never);
            _dbTransaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void DeleteByContractId_Returns_Forbidden_If_Not_Allowed_To_Modify_System()
        {
            //Arrange
            var contract = CreateContract();
            ExpectGetContractReturns(contract.Id, contract);
            ExpectAllowModifyReturns(contract, false);

            //Act
            var result = _sut.DeleteByContractId(contract.Id);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
            _dbTransaction.Verify(x => x.Rollback(), Times.Never);
            _dbTransaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void DeleteByContractId_Returns_Ok_If_No_References()
        {
            var contract = CreateContract();
            ExpectGetContractReturns(contract.Id, contract);
            ExpectAllowModifyReturns(contract, true);

            //Act
            var result = _sut.DeleteByContractId(contract.Id);

            //Assert
            Assert.True(result.Ok);
            _dbTransaction.Verify(x => x.Rollback(), Times.Never);
            _dbTransaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void DeleteByContractId_Returns_Ok_If_References()
        {
            //Arrange
            var contract = CreateContract();
            var reference = CreateReference();
            contract = AddExternalReference(contract, reference);
            ExpectGetContractReturns(contract.Id, contract);
            ExpectAllowModifyReturns(contract, true);
            ExpectTransactionToBeSet();

            //Act
            var result = _sut.DeleteByContractId(contract.Id);

            //Assert
            Assert.True(result.Ok);
            _dbTransaction.Verify(x => x.Rollback(), Times.Never);
            _dbTransaction.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public void DeleteByProjectId_Returns_NotFound()
        {
            //Arrange
            var id = A<int>();
            ExpectGetProjectReturns(id, null);

            //Act
            var result = _sut.DeleteByProjectId(id);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.NotFound, result.Error);
            _dbTransaction.Verify(x => x.Rollback(), Times.Never);
            _dbTransaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void DeleteByProjectId_Returns_Forbidden_If_Not_Allowed_To_Modify_System()
        {
            //Arrange
            var project = CreateProject();
            ExpectGetProjectReturns(project.Id, project);
            ExpectAllowModifyReturns(project, false);

            //Act
            var result = _sut.DeleteByProjectId(project.Id);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
            _dbTransaction.Verify(x => x.Rollback(), Times.Never);
            _dbTransaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void DeleteByProjectId_Returns_Ok_If_No_References()
        {
            var project = CreateProject();
            ExpectGetProjectReturns(project.Id, project);
            ExpectAllowModifyReturns(project, true);

            //Act
            var result = _sut.DeleteByProjectId(project.Id);

            //Assert
            Assert.True(result.Ok);
            _dbTransaction.Verify(x => x.Rollback(), Times.Never);
            _dbTransaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void DeleteByProjectId_Returns_Ok_If_References()
        {
            //Arrange
            var project = CreateProject();
            var reference = CreateReference();
            project = AddExternalReference(project, reference);
            ExpectGetProjectReturns(project.Id, project);
            ExpectAllowModifyReturns(project, true);
            ExpectTransactionToBeSet();

            //Act
            var result = _sut.DeleteByProjectId(project.Id);

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
            var result = _sut.AddReference(id, rootType, A<string>(), A<string>(), A<string>());

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
            var result = _sut.AddReference(id, rootType, A<string>(), A<string>(), A<string>());

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
            var entity = new Mock<IEntityWithExternalReferences>();
            ExpectGetRootEntityReturns(id, rootType, Maybe<IEntityWithExternalReferences>.Some(entity.Object));
            ExpectAllowModifyReturns(entity.Object, true);
            ExpectAddReferenceReturns(entity, title, externalReferenceId, url, new ExternalReference());

            //Act
            var result = _sut.AddReference(id, rootType, title, externalReferenceId, url);

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
            var entity = new Mock<IEntityWithExternalReferences>();
            ExpectGetRootEntityReturns(id, rootType, Maybe<IEntityWithExternalReferences>.Some(entity.Object));
            ExpectAllowModifyReturns(entity.Object, true);
            var operationError = A<OperationError>();
            ExpectAddReferenceReturns(entity, title, externalReferenceId, url, operationError);

            //Act
            var result = _sut.AddReference(id, rootType, title, externalReferenceId, url);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            _referenceRepository.Verify(x => x.SaveRootEntity(entity.Object), Times.Never);
        }

        private static void ExpectAddReferenceReturns(Mock<IEntityWithExternalReferences> entity, string title, string externalReferenceId, string url, Result<ExternalReference, OperationError> result)
        {
            entity.Setup(x => x.AddExternalReference(It.Is<ExternalReference>(er =>
                    er.Title == title &&
                    er.ExternalReferenceId == externalReferenceId &&
                    er.URL == url)))
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

        private void ExpectGetSystemUsageReturns(int id, ItSystemUsage systemUsage)
        {
            _systemUsageRepository.Setup(x => x.GetSystemUsage(id)).Returns(systemUsage);
        }

        private void ExpectGetContractReturns(int id, ItContract contract)
        {
            _contractRepository.Setup(x => x.GetById(id)).Returns(contract);
        }

        private void ExpectGetProjectReturns(int id, ItProject project)
        {
            _projectRepository.Setup(x => x.GetById(id)).Returns(project);
        }

        private void ExpectTransactionToBeSet()
        {
            _transactionManager.Setup(x => x.Begin(IsolationLevel.ReadCommitted)).Returns(_dbTransaction.Object);
        }

        private ItSystem CreateSystem()
        {
            return new ItSystem { Id = A<int>() };
        }

        private ItContract CreateContract()
        {
            return new ItContract { Id = A<int>() };
        }

        private ItProject CreateProject()
        {
            return new ItProject { Id = A<int>() };
        }

        private ItSystemUsage CreateSystemUsage()
        {
            return new ItSystemUsage { Id = A<int>(), ItSystem = CreateSystem() };
        }
        private ExternalReference CreateReference()
        {
            return new ExternalReference { Id = A<int>() };
        }

        private static T AddExternalReference<T>(T system, ExternalReference reference) where T: IEntityWithExternalReferences
        {
            system.ExternalReferences.Add(reference);
            return system;
        }
    }
}