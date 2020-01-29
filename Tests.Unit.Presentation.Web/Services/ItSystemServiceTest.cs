using System.Collections.Generic;
using System.Data;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.System;
using Core.ApplicationServices.System;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainServices.Repositories.System;
using Infrastructure.Services.DataAccess;
using Moq;
using Serilog;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Services
{
    public class ItSystemServiceTest : WithAutoFixture
    {
        private readonly ItSystemService _sut;
        private readonly Mock<IAuthorizationContext> _authorizationContext;
        private readonly Mock<IItSystemRepository> _systemRepository;
        private readonly Mock<ITransactionManager> _transactionManager;
        private readonly Mock<IDatabaseTransaction> _dbTransaction;
        private readonly Mock<IReferenceService> _referenceService;
        private readonly Mock<ILogger> _logger;

        public ItSystemServiceTest()
        {
            _authorizationContext = new Mock<IAuthorizationContext>();
            _systemRepository = new Mock<IItSystemRepository>();
            _transactionManager = new Mock<ITransactionManager>();
            _dbTransaction = new Mock<IDatabaseTransaction>();
            _referenceService = new Mock<IReferenceService>();
            _logger = new Mock<ILogger>();
            _sut = new ItSystemService(
                null,
                _systemRepository.Object,
                _authorizationContext.Object,
                _transactionManager.Object,
                _referenceService.Object,
                _logger.Object
                );
        }

        [Fact]
        public void GetUsingOrganizations_Returns_Forbidden_If_Read_Access_To_System_Is_Unauthorized()
        {
            //Arrange
            var system = CreateSystem();
            ExpectGetSystemReturns(system.Id, system);
            ExpectAllowReadsReturns(system, false);

            //Act
            var result = _sut.GetUsingOrganizations(system.Id);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
        }

        [Fact]
        public void GetUsingOrganizations_Returns_NotFound_If_System_Does_Not_Exist()
        {
            //Arrange
            var system = CreateSystem();
            ExpectGetSystemReturns(system.Id, null);
            //Act
            var result = _sut.GetUsingOrganizations(system.Id);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.NotFound, result.Error);
        }

        [Fact]
        public void GetUsingOrganizations_Returns_UsingOrganization_List_With_Single_Item()
        {
            //Arrange
            var system = CreateSystem();
            var organization = CreateOrganization();
            var systemUsage = CreateSystemUsage(organization);
            AddUsage(system, systemUsage);
            ExpectGetSystemReturns(system.Id, system);
            ExpectAllowReadsReturns(system, true);

            //Act
            var result = _sut.GetUsingOrganizations(system.Id);

            //Assert
            Assert.True(result.Ok);
            var usage = Assert.Single(result.Value);
            Assert.Equal(systemUsage.Id, usage.ItSystemUsageId);
            Assert.Equal(organization.Id, usage.Organization.Id);
            Assert.Equal(organization.Name, usage.Organization.Name);
        }

        [Fact]
        public void GetUsingOrganizations_Returns_UsingOrganization_List_With_Multiple_Items()
        {
            //Arrange
            var system = CreateSystem();
            var firstOrganization = CreateOrganization();
            var firstSystemUsage = CreateSystemUsage(firstOrganization);
            AddUsage(system, firstSystemUsage);
            var secondOrganization = CreateOrganization();
            var secondSystemUsage = CreateSystemUsage(secondOrganization);
            AddUsage(system, secondSystemUsage);
            ExpectGetSystemReturns(system.Id, system);
            ExpectAllowReadsReturns(system, true);

            //Act
            var result = _sut.GetUsingOrganizations(system.Id);

            //Assert
            Assert.True(result.Ok);
            Assert.Collection(result.Value,
                item => Assert.Equal(firstSystemUsage.Id, item.ItSystemUsageId),
                item => Assert.Equal(secondSystemUsage.Id, item.ItSystemUsageId)
                );
        }

        [Fact]
        public void Delete_Returns_NotFound()
        {
            //Arrange
            var system = CreateSystem();
            ExpectGetSystemReturns(system.Id, null);

            //Act
            var result = _sut.Delete(system.Id);

            //Assert
            Assert.Equal(SystemDeleteResult.NotFound, result);
            _dbTransaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Delete_Returns_Forbidden()
        {
            //Arrange
            var system = CreateSystem();
            ExpectAllowDeleteReturns(system, false);
            ExpectGetSystemReturns(system.Id, system);

            //Act
            var result = _sut.Delete(system.Id);

            //Assert
            Assert.Equal(SystemDeleteResult.Forbidden, result);
            _dbTransaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Delete_Returns_InUse_If_System_Is_In_Use()
        {
            //Arrange
            var system = CreateSystem();
            var organization = CreateOrganization();
            var systemUsage = CreateSystemUsage(organization);
            AddUsage(system, systemUsage);
            ExpectAllowDeleteReturns(system, true);
            ExpectGetSystemReturns(system.Id, system);

            //Act
            var result = _sut.Delete(system.Id);

            //Assert
            Assert.Equal(SystemDeleteResult.InUse, result);
            _dbTransaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Delete_Returns_HasChildren_If_System_Has_Children()
        {
            //Arrange
            var system = CreateSystem();
            AddChild(system, CreateSystem());
            ExpectAllowDeleteReturns(system, true);
            ExpectGetSystemReturns(system.Id, system);

            //Act
            var result = _sut.Delete(system.Id);

            //Assert
            Assert.Equal(SystemDeleteResult.HasChildren, result);
            _dbTransaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Delete_Returns_HasExhibitInterfaces_If_System_Has_Exhibit_Interfaces()
        {
            //Arrange
            var system = CreateSystem();
            AddInterfaceExhibit(system, CreateInterfaceExhibit());
            ExpectAllowDeleteReturns(system, true);
            ExpectGetSystemReturns(system.Id, system);

            //Act
            var result = _sut.Delete(system.Id);

            //Assert
            Assert.Equal(SystemDeleteResult.HasInterfaceExhibits, result);
            _dbTransaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Delete_Returns_Not_Found_If_System_Does_Not_Exist()
        {
            //Arrange
            var system = CreateSystem();
            ExpectAllowDeleteReturns(system, true);
            ExpectGetSystemReturns(system.Id, null);

            //Act
            var result = _sut.Delete(system.Id);

            //Assert
            Assert.Equal(SystemDeleteResult.NotFound, result);
            _dbTransaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Delete_Returns_Ok_If_System_Can_Be_Deleted()
        {
            //Arrange
            var system = CreateSystem();
            ExpectAllowDeleteReturns(system, true);
            ExpectGetSystemReturns(system.Id, system);
            ExpectTransactionToBeSet();
            _referenceService.Setup(x => x.DeleteBySystemId(system.Id)).Returns(Result<IEnumerable<ExternalReference>, OperationFailure>.Success(new ExternalReference[0]));

            //Act
            var result = _sut.Delete(system.Id);

            //Assert
            Assert.Equal(SystemDeleteResult.Ok, result);
            _dbTransaction.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public void Delete_Returns_Ok_And_Deletes_ExternalReferences()
        {
            //Arrange
            var system = CreateSystem();
            var externalReference = CreateExternalReference();
            AddExternalReference(system, externalReference);
            ExpectAllowDeleteReturns(system, true);
            ExpectGetSystemReturns(system.Id, system);
            ExpectDeleteReferenceReturns(system.Id, Result<IEnumerable<ExternalReference>, OperationFailure>.Success(new ExternalReference[0]));
            ExpectTransactionToBeSet();

            //Act
            var result = _sut.Delete(system.Id);

            //Assert
            Assert.Equal(SystemDeleteResult.Ok, result);
            _dbTransaction.Verify(x => x.Commit(), Times.Once);
            _referenceService.Verify(x => x.DeleteBySystemId(system.Id), Times.Once);
        }

        [Theory]
        [InlineData(OperationFailure.Forbidden)]
        [InlineData(OperationFailure.UnknownError)]
        [InlineData(OperationFailure.NotFound)]
        public void Delete_Returns_UnknownError_And_Does_Not_Delete_ExternalReferences(OperationFailure referenceDeleteResult)
        {
            //Arrange
            var system = CreateSystem();
            var externalReference = CreateExternalReference();
            AddExternalReference(system, externalReference);
            ExpectAllowDeleteReturns(system, true);
            ExpectGetSystemReturns(system.Id, system);
            ExpectDeleteReferenceReturns(system.Id, Result<IEnumerable<ExternalReference>, OperationFailure>.Failure(referenceDeleteResult));
            ExpectTransactionToBeSet();

            //Act
            var result = _sut.Delete(system.Id);

            //Assert
            Assert.Equal(SystemDeleteResult.UnknownError, result);
            _dbTransaction.Verify(x => x.Commit(), Times.Never);
            _dbTransaction.Verify(x => x.Rollback(), Times.Once);
            _referenceService.Verify(x => x.DeleteBySystemId(system.Id), Times.Once);
        }

        [Fact]
        public void Delete_Returns_Ok_With_Task_Refs_Added()
        {
            //Arrange
            var system = CreateSystem();
            var taskRef = createTaskRef();
            AddTaskRef(system, taskRef);
            ExpectAllowDeleteReturns(system, true);
            ExpectGetSystemReturns(system.Id, system);
            ExpectTransactionToBeSet();
            _referenceService.Setup(x => x.DeleteBySystemId(system.Id)).Returns(Result<IEnumerable<ExternalReference>, OperationFailure>.Success(new ExternalReference[0]));

            //Act
            var result = _sut.Delete(system.Id);

            //Assert
            Assert.Equal(SystemDeleteResult.Ok, result);
            _dbTransaction.Verify(x => x.Commit(), Times.Once);
            _dbTransaction.Verify(x => x.Rollback(), Times.Never);
            _referenceService.Verify(x => x.DeleteBySystemId(system.Id), Times.Once);
        }

        private Organization CreateOrganization()
        {
            return new Organization { Id = A<int>(), Name = A<string>() };
        }

        private ItSystem CreateSystem()
        {
            return new ItSystem { Id = A<int>() };
        }

        private TaskRef createTaskRef()
        {
            return new TaskRef { Id = A<int>() };
        }

        private ItSystemUsage CreateSystemUsage(Organization organization)
        {
            return new ItSystemUsage { Id = A<int>(), Organization = organization };
        }

        private ItInterfaceExhibit CreateInterfaceExhibit()
        {
            return new ItInterfaceExhibit() { Id = A<int>() };
        }

        private ExternalReference CreateExternalReference()
        {
            return new ExternalReference() { Id = A<int>() };
        }

        private void ExpectAllowReadsReturns(ItSystem system, bool value)
        {
            _authorizationContext.Setup(x => x.AllowReads(system)).Returns(value);
        }

        private void ExpectAllowDeleteReturns(ItSystem system, bool value)
        {
            _authorizationContext.Setup(x => x.AllowDelete(system)).Returns(value);
        }

        private void ExpectGetSystemReturns(int id, ItSystem system)
        {
            _systemRepository.Setup(x => x.GetSystem(id)).Returns(system);
        }

        private void ExpectDeleteReferenceReturns(int id, Result<IEnumerable<ExternalReference>, OperationFailure> result)
        {
            _referenceService.Setup(x => x.DeleteBySystemId(id)).Returns(result);
        }

        private void ExpectTransactionToBeSet()
        {
            _transactionManager.Setup(x => x.Begin(IsolationLevel.Serializable)).Returns(_dbTransaction.Object);
        }

        private static void AddUsage(ItSystem system, ItSystemUsage usage)
        {
            system.Usages.Add(usage);
        }

        private static void AddChild(ItSystem mainSystem, ItSystem childSystem)
        {
            mainSystem.Children.Add(childSystem);
        }

        private static void AddInterfaceExhibit(ItSystem system, ItInterfaceExhibit interfaceExhibit)
        {
            system.ItInterfaceExhibits.Add(interfaceExhibit);
        }

        private static void AddExternalReference(ItSystem system, ExternalReference externalReference)
        {
            system.ExternalReferences.Add(externalReference);
        }

        private static void AddTaskRef(ItSystem system, TaskRef taskRef)
        {
            system.TaskRefs.Add(taskRef);
        }
    }
}