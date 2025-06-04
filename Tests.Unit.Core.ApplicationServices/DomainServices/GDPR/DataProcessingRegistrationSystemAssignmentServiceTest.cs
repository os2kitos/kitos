using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.GDPR;
using Core.DomainServices.Repositories.SystemUsage;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.DomainServices.GDPR
{
    public class DataProcessingRegistrationSystemAssignmentServiceTest : WithAutoFixture
    {
        private readonly Mock<IItSystemUsageRepository> _systemRepository;
        private readonly DataProcessingRegistrationSystemAssignmentService _sut;

        public DataProcessingRegistrationSystemAssignmentServiceTest()
        {
            _systemRepository = new Mock<IItSystemUsageRepository>();
            _sut = new DataProcessingRegistrationSystemAssignmentService(_systemRepository.Object);
        }

        [Fact]
        public void Can_GetApplicableSystems()
        {
            //Arrange
            var alreadyAssignedSystem = new ItSystemUsage { Id = A<int>() };
            var availableSystem = new ItSystemUsage { Id = A<int>() };

            var organizationId = A<int>();

            var registration = new DataProcessingRegistration
            {
                OrganizationId = organizationId,
                SystemUsages = { alreadyAssignedSystem }
            };
            ExpectSystemsInUse(organizationId, availableSystem, alreadyAssignedSystem);

            //Act
            var applicableSystems = _sut.GetApplicableSystems(registration);

            //Assert
            var itSystem = Assert.Single(applicableSystems);
            Assert.Same(availableSystem, itSystem);
        }

        [Fact]
        public void Can_AssignSystem()
        {
            //Arrange
            var organizationId = A<int>();
            var systemUsageId = A<int>();
            var registration = new DataProcessingRegistration { OrganizationId = organizationId };
            var itSystemUsage = new ItSystemUsage { OrganizationId = registration.OrganizationId };
            _systemRepository.Setup(x => x.GetSystemUsage(systemUsageId)).Returns(itSystemUsage);

            //Act
            var result = _sut.AssignSystem(registration, systemUsageId);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(itSystemUsage, result.Value);
            Assert.True(registration.SystemUsages.Contains(itSystemUsage));
        }

        [Fact]
        public void Cannot_AssignSystem_If_Organization_Id_Is_Different_From_Dpr_Org()
        {
            //Arrange
            var orgId1 = A<int>();
            var orgId2 = orgId1 + 1;
            var systemUsageId = A<int>();
            var registration = new DataProcessingRegistration { OrganizationId = orgId1 };
            var itSystemUsage = new ItSystemUsage { OrganizationId = orgId2 };
            _systemRepository.Setup(x => x.GetSystemUsage(systemUsageId)).Returns(itSystemUsage);

            //Act
            var result = _sut.AssignSystem(registration, systemUsageId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_AssignSystem_If_System_Is_Not_Found()
        {
            //Arrange
            var organizationId = A<int>();
            var systemUsageId = A<int>();
            var registration = new DataProcessingRegistration { OrganizationId = organizationId };

            _systemRepository.Setup(x => x.GetSystemUsage(systemUsageId)).Returns(default(ItSystemUsage));

            //Act
            var result = _sut.AssignSystem(registration, systemUsageId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            Assert.Empty(registration.SystemUsages);
        }

        [Fact]
        public void Can_RemoveSystem()
        {
            //Arrange
            var organizationId = A<int>();
            var systemId = A<int>();

            var itSystemUsage = new ItSystemUsage { OrganizationId = organizationId, Id = A<int>() };

            var registration = new DataProcessingRegistration { OrganizationId = organizationId, SystemUsages = { itSystemUsage } };

            _systemRepository.Setup(x => x.GetSystemUsage(systemId)).Returns(itSystemUsage);

            //Act
            var result = _sut.RemoveSystem(registration, systemId);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(itSystemUsage, result.Value);
            Assert.Empty(registration.SystemUsages);
        }

        [Fact]
        public void Cannot_RemoveSystem_If_System_Is_Not_Found()
        {
            //Arrange
            var organizationId = A<int>();
            var systemId = A<int>();
            var registration = new DataProcessingRegistration { OrganizationId = organizationId };

            _systemRepository.Setup(x => x.GetSystemUsage(systemId)).Returns(default(ItSystemUsage));

            //Act
            var result = _sut.RemoveSystem(registration, systemId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_RemoveSystem_If_System_Is_Not_AssignedAlready()
        {
            //Arrange
            var organizationId = A<int>();
            var systemId = A<int>();

            var itSystemUsage = new ItSystemUsage { OrganizationId = A<int>(), Id = A<int>() };

            var registration = new DataProcessingRegistration { OrganizationId = organizationId, SystemUsages = { new ItSystemUsage() { Id = A<int>() } } };

            _systemRepository.Setup(x => x.GetSystemUsage(systemId)).Returns(itSystemUsage);

            //Act
            var result = _sut.RemoveSystem(registration, systemId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            Assert.NotEmpty(registration.SystemUsages);
        }

        private void ExpectSystemsInUse(int organizationId, params ItSystemUsage[] systemUsages)
        {
            _systemRepository.Setup(x => x.GetSystemUsagesFromOrganization(organizationId))
                .Returns(systemUsages.AsQueryable());
        }
    }
}
