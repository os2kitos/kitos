using System.Data;
using Core.ApplicationServices;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Result;
using Core.ApplicationServices.System;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainServices.Repositories.System;
using Infrastructure.Services.DataAccess;
using Moq;
using Serilog;
using Tests.Unit.Presentation.Web.Helpers;
using Xunit;

namespace Tests.Unit.Presentation.Web.Services
{
    public class ItSystemServiceTest : WithAutoFixture
    {
        private readonly ItSystemService _sut;
        private readonly Mock<IAuthorizationContext> _authorizationContext;
        private readonly Mock<IItSystemRepository> _systemRepository;
        private readonly Mock<ITransactionManager> _transactionManager;
        private readonly Mock<ReferenceService> _referenceService;

        public ItSystemServiceTest()
        {
            _authorizationContext = new Mock<IAuthorizationContext>();
            _systemRepository = new Mock<IItSystemRepository>();
            _transactionManager = new Mock<ITransactionManager>();
            _referenceService = new Mock<ReferenceService>();
            _sut = new ItSystemService(
                null, 
                null, 
                _systemRepository.Object, 
                _authorizationContext.Object,
                _transactionManager.Object,
                _referenceService.Object,
                null
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
            Assert.Equal(OperationResult.Forbidden, result.Status);
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
            Assert.Equal(OperationResult.NotFound, result.Status);
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
            Assert.Equal(OperationResult.Ok, result.Status);
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
            Assert.Equal(OperationResult.Ok, result.Status);
            Assert.Collection(result.Value, 
                item => Assert.Equal(firstSystemUsage.Id, item.ItSystemUsageId),
                item => Assert.Equal(secondSystemUsage.Id, item.ItSystemUsageId)
                );
        }

        [Fact]
        public void Delete_Returns_Forbidden()
        {
            //Arrange
            var system = CreateSystem();
            ExpectAllowDeleteReturns(system, false);

            //Act
            var result = _sut.Delete(system.Id);

            //Assert
            Assert.Equal(SystemDeleteResult.Forbidden, result);
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
        }

        [Fact]
        public void Delete_Returns_Ok_If_System_Can_Be_Deleted()
        {
            //Arrange
            var system = CreateSystem();
            ExpectAllowDeleteReturns(system, true);
            ExpectGetSystemReturns(system.Id, system);
            ExpectTransactionToBeSet();

            //Act
            var result = _sut.Delete(system.Id);

            //Assert
            Assert.Equal(SystemDeleteResult.Ok, result);
        }

        [Fact]
        public void Delete_Returns_Ok_And_Deletes_ExternalReferences()
        {
            //Arrange
            var system = CreateSystem();
            AddExternalReference(system, CreateExternalReference());
            ExpectAllowDeleteReturns(system, true);
            ExpectGetSystemReturns(system.Id, system);
            ExpectTransactionToBeSet();

            //Act
            var result = _sut.Delete(system.Id);

            //Assert
            Assert.Equal(SystemDeleteResult.Ok, result);
        }

        private Organization CreateOrganization()
        {
            return new Organization { Id = A<int>(), Name = A<string>()};
        }

        private ItSystem CreateSystem()
        {
            return new ItSystem { Id = A<int>() };
        }

        private ItSystemUsage CreateSystemUsage(Organization organization)
        {
            return new ItSystemUsage { Id = A<int>(), Organization = organization };
        }

        private ItInterfaceExhibit CreateInterfaceExhibit()
        {
            return new ItInterfaceExhibit() { Id = A<int>()};
        }

        private ExternalReference CreateExternalReference()
        {
            return new ExternalReference() {Id = A<int>()};
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

        private void ExpectTransactionToBeSet()
        {
            var dbTransAction = new Mock<IDatabaseTransaction>().Object;
            _transactionManager.Setup(x => x.Begin(IsolationLevel.Serializable)).Returns(dbTransAction);
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
    }
}