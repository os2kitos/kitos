using System.Data;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.System;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Result;
using Core.DomainServices.Repositories.Reference;
using Core.DomainServices.Repositories.System;
using Infrastructure.Services.DataAccess;
using Moq;
using Tests.Unit.Presentation.Web.Helpers;
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
                _transactionManager.Object
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

        private void ExpectAllowModifyReturns(ItSystem system, bool value)
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
            return new ItSystem {Id = A<int>()};
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