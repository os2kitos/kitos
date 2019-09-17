using System;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Result;
using Core.ApplicationServices.Model.System;
using Core.ApplicationServices.System;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Repositories.System;
using Moq;
using Tests.Unit.Presentation.Web.Helpers;
using Xunit;

namespace Tests.Unit.Presentation.Web.Services
{
    public class ItSystemServiceTest : WithAutoFixture
    {
        private readonly ItSystemService _sut;
        private readonly Mock<IAuthorizationContext> _authorizationContext;
        private readonly Mock<IItSystemRepository> _systemRepository;

        public ItSystemServiceTest()
        {
            _authorizationContext = new Mock<IAuthorizationContext>();
            _systemRepository = new Mock<IItSystemRepository>();
            
            _sut = new ItSystemService(
                null, 
                null, 
                _systemRepository.Object, 
                _authorizationContext.Object);
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

        private void ExpectAllowReadsReturns(ItSystem system, bool value)
        {
            _authorizationContext.Setup(x => x.AllowReads(system)).Returns(value);
        }

        private void ExpectGetSystemReturns(int id, ItSystem system)
        {
            _systemRepository.Setup(x => x.GetSystem(id)).Returns(system);
        }

        private void AddUsage(ItSystem system, ItSystemUsage usage)
        {
            system.Usages.Add(usage);
        }
    }
}