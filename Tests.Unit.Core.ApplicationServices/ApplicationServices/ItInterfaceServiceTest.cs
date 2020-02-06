using System.Data;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Interface;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Repositories.System;
using Core.DomainServices.Time;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.DomainEvents;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;
using DataRow = Core.DomainModel.ItSystem.DataRow;

namespace Tests.Unit.Core.ApplicationServices
{
    public class ItInterfaceServiceTest : WithAutoFixture
    {
        private readonly ItInterfaceService _sut;
        private readonly Mock<IGenericRepository<ItInterface>> _interfaceRepository;
        private readonly Mock<IAuthorizationContext> _authorizationContext;
        private readonly Mock<IItSystemRepository> _systemRepository;
        private readonly Mock<IDomainEvents> _domainEvents;
        private readonly Mock<ITransactionManager> _transactionManager;
        private readonly Mock<IOrganizationalUserContext> _UserContext;

        public ItInterfaceServiceTest()
        {
            _interfaceRepository = new Mock<IGenericRepository<ItInterface>>();
            _authorizationContext = new Mock<IAuthorizationContext>();
            _systemRepository = new Mock<IItSystemRepository>();
            _domainEvents = new Mock<IDomainEvents>();
            _transactionManager = new Mock<ITransactionManager>();
            _UserContext = new Mock<IOrganizationalUserContext>();
            _sut = new ItInterfaceService(
                _interfaceRepository.Object,
                Mock.Of<IGenericRepository<DataRow>>(),
                _systemRepository.Object,
                _authorizationContext.Object,
                _UserContext.Object,
                _transactionManager.Object,
                _domainEvents.Object,
                new Mock<IOperationClock>().Object);
        }

        [Fact]
        public void ChangeExposingSystem_Returns_NotFound()
        {
            //Arrange
            var interfaceId = A<int>();
            ExpectGetInterfaceReturns(interfaceId, default(ItInterface));

            //Act
            var result = _sut.ChangeExposingSystem(interfaceId, A<int>());

            //Assert
            Assert.Equal(OperationFailure.NotFound, result.Error);
        }

        [Fact]
        public void ChangeExposingSystem_Returns_Forbidden_If_Modification_Access_To_Interface_Is_Denied()
        {
            //Arrange
            var interfaceId = A<int>();
            var itInterface = new ItInterface();
            ExpectGetInterfaceReturns(interfaceId, itInterface);
            ExpectAllowModifyReturns(itInterface, false);

            //Act
            var result = _sut.ChangeExposingSystem(interfaceId, A<int>());

            //Assert
            Assert.Equal(OperationFailure.Forbidden, result.Error);
        }

        [Fact]
        public void ChangeExposingSystem_Returns_BadInput_If_System_Id_Is_Invalid()
        {
            //Arrange
            var interfaceId = A<int>();
            var itInterface = new ItInterface();
            var newSystemId = A<int>();

            ExpectGetInterfaceReturns(interfaceId, itInterface);
            ExpectAllowModifyReturns(itInterface, true);
            ExpectGetSystemReturns(newSystemId, default(ItSystem));

            //Act
            var result = _sut.ChangeExposingSystem(interfaceId, newSystemId);

            //Assert
            Assert.Equal(OperationFailure.BadInput, result.Error);
        }

        [Fact]
        public void ChangeExposingSystem_Returns_Forbidden_If_Read_Access_To_System_Is_Denied()
        {
            //Arrange
            var interfaceId = A<int>();
            var itInterface = new ItInterface();
            var newSystemId = A<int>();
            var itSystem = new ItSystem();

            ExpectGetInterfaceReturns(interfaceId, itInterface);
            ExpectAllowModifyReturns(itInterface, true);
            ExpectGetSystemReturns(newSystemId, itSystem);
            ExpectAllowReadReturns(itSystem, false);

            //Act
            var result = _sut.ChangeExposingSystem(interfaceId, newSystemId);

            //Assert
            Assert.Equal(OperationFailure.Forbidden, result.Error);
        }

        [Fact]
        public void ChangeExposingSystem_Returns_Ok_When_Existing_And_New_Is_None()
        {
            //Arrange
            var interfaceId = A<int>();
            var itInterface = new ItInterface();

            ExpectGetInterfaceReturns(interfaceId, itInterface);
            ExpectAllowModifyReturns(itInterface, true);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin(It.IsAny<IsolationLevel>())).Returns(transaction.Object);

            //Act
            var result = _sut.ChangeExposingSystem(interfaceId, null);

            //Assert that no changes were saved
            Assert.True(result.Ok);
            transaction.Verify(x => x.Commit(), Times.Never);
            _interfaceRepository.Verify(x => x.Save(), Times.Never);
        }

        [Fact]
        public void ChangeExposingSystem_Returns_Ok_When_Existing_And_New_Are_Equal()
        {
            //Arrange
            var interfaceId = A<int>();
            var existingSystem = new ItSystem() { Id = A<int>() };
            var itInterface = new ItInterface() { ExhibitedBy = new ItInterfaceExhibit() { ItSystem = existingSystem } };
            ExpectGetInterfaceReturns(interfaceId, itInterface);
            ExpectAllowModifyReturns(itInterface, true);
            ExpectGetSystemReturns(existingSystem.Id, existingSystem);
            ExpectAllowReadReturns(existingSystem, true);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin(It.IsAny<IsolationLevel>())).Returns(transaction.Object);

            //Act
            var result = _sut.ChangeExposingSystem(interfaceId, existingSystem.Id);

            //Assert that no changes were saved
            Assert.True(result.Ok);
            transaction.Verify(x => x.Commit(), Times.Never);
            _interfaceRepository.Verify(x => x.Save(), Times.Never);
        }

        [Fact]
        public void ChangeExposingSystem_Returns_Ok_With_Changes()
        {
            //Arrange
            var interfaceId = A<int>();
            var existingSystem = new ItSystem() { Id = A<int>() };
            var newSystem = new ItSystem() { Id = A<int>() };
            var itInterface = new ItInterface() { ExhibitedBy = new ItInterfaceExhibit() { ItSystem = existingSystem } };
            ExpectGetInterfaceReturns(interfaceId, itInterface);
            ExpectAllowModifyReturns(itInterface, true);
            ExpectGetSystemReturns(newSystem.Id, newSystem);
            ExpectAllowReadReturns(newSystem, true);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin(It.IsAny<IsolationLevel>())).Returns(transaction.Object);

            //Act
            var result = _sut.ChangeExposingSystem(interfaceId, newSystem.Id);

            //Assert that no changes were saved
            Assert.True(result.Ok);
            Assert.Same(newSystem, result.Value.ExhibitedBy.ItSystem);
            transaction.Verify(x => x.Commit(), Times.Once);
            _interfaceRepository.Verify(x => x.Save(), Times.Once);
        }

        private void ExpectGetInterfaceReturns(int interfaceId, ItInterface value)
        {
            _interfaceRepository.Setup(x => x.GetByKey(interfaceId)).Returns(value);
        }

        private void ExpectAllowModifyReturns(ItInterface itInterface, bool value)
        {
            _authorizationContext.Setup(x => x.AllowModify(itInterface)).Returns(value);
        }

        private void ExpectAllowReadReturns(ItSystem itSystem, bool value)
        {
            _authorizationContext.Setup(x => x.AllowReads(itSystem)).Returns(value);
        }

        private void ExpectGetSystemReturns(int newSystemId, ItSystem value)
        {
            _systemRepository.Setup(x => x.GetSystem(newSystemId)).Returns(value);
        }
    }
}
