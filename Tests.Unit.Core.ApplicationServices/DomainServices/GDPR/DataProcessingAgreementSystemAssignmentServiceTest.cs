using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Result;
using Core.DomainServices.GDPR;
using Core.DomainServices.Repositories.System;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.DomainServices.GDPR
{
    public class DataProcessingAgreementSystemAssignmentServiceTest : WithAutoFixture
    {
        private readonly Mock<IItSystemRepository> _systemRepository;
        private readonly DataProcessingAgreementSystemAssignmentService _sut;

        public DataProcessingAgreementSystemAssignmentServiceTest()
        {
            _systemRepository = new Mock<IItSystemRepository>();
            _sut = new DataProcessingAgreementSystemAssignmentService(_systemRepository.Object);
        }

        [Fact]
        public void Can_GetApplicableSystems()
        {
            //Arrange
            var alreadyAssignedSystem = new ItSystem { Id = A<int>() };
            var availableSystem = new ItSystem { Id = A<int>() };

            var organizationId = A<int>();

            var dataProcessingAgreement = new DataProcessingAgreement
            {
                OrganizationId = organizationId,
                SystemUsages = new List<ItSystemUsage> { new ItSystemUsage
                {
                    OrganizationId = organizationId,
                    ItSystem = alreadyAssignedSystem
                } }
            };
            ExpectSystemsInUse(organizationId, availableSystem, alreadyAssignedSystem);

            //Act
            var applicableSystems = _sut.GetApplicableSystems(dataProcessingAgreement);

            //Assert
            var itSystem = Assert.Single(applicableSystems);
            Assert.Same(availableSystem, itSystem);
        }

        [Fact]
        public void Can_AssignSystem()
        {
            //Arrange
            var organizationId = A<int>();
            var systemId = A<int>();
            var dataProcessingAgreement = new DataProcessingAgreement { OrganizationId = organizationId };
            var itSystemUsage = new ItSystemUsage { OrganizationId = dataProcessingAgreement.OrganizationId };
            var itSystem = new ItSystem
            {
                //Add usage in same organization as dpa
                Usages = {itSystemUsage}
            };

            _systemRepository.Setup(x => x.GetSystem(systemId)).Returns(itSystem);

            //Act
            var result = _sut.AssignSystem(dataProcessingAgreement, systemId);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(itSystem, result.Value);
            Assert.True(dataProcessingAgreement.SystemUsages.Contains(itSystemUsage));
        }

        [Fact]
        public void Cannot_AssignSystem_If_System_Is_Not_UsedInOrganization()
        {
            //Arrange
            var organizationId = A<int>();
            var systemId = A<int>();
            var dataProcessingAgreement = new DataProcessingAgreement { OrganizationId = organizationId };
            var itSystem = new ItSystem
            {
                //Add usage in a different organization than the DPA so assignment is not possible
                Usages = { new ItSystemUsage { OrganizationId = A<int>() } }
            };

            _systemRepository.Setup(x => x.GetSystem(systemId)).Returns(itSystem);

            //Act
            var result = _sut.AssignSystem(dataProcessingAgreement, systemId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            Assert.Empty(dataProcessingAgreement.SystemUsages);
        }

        [Fact]
        public void Cannot_AssignSystem_If_System_Is_Not_Found()
        {
            //Arrange
            var organizationId = A<int>();
            var systemId = A<int>();
            var dataProcessingAgreement = new DataProcessingAgreement { OrganizationId = organizationId };

            _systemRepository.Setup(x => x.GetSystem(systemId)).Returns(default(ItSystem));

            //Act
            var result = _sut.AssignSystem(dataProcessingAgreement, systemId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            Assert.Empty(dataProcessingAgreement.SystemUsages);
        }

        [Fact]
        public void Can_RemoveSystem()
        {
            //Arrange
            var organizationId = A<int>();
            var systemId = A<int>();

            var itSystemUsage = new ItSystemUsage { OrganizationId = organizationId, Id = A<int>() };
            var itSystem = new ItSystem
            {
                //Add usage in same organization as dpa
                Usages = { itSystemUsage }
            };

            var dataProcessingAgreement = new DataProcessingAgreement { OrganizationId = organizationId, SystemUsages = { itSystemUsage } };

            _systemRepository.Setup(x => x.GetSystem(systemId)).Returns(itSystem);

            //Act
            var result = _sut.RemoveSystem(dataProcessingAgreement, systemId);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(itSystem, result.Value);
            Assert.Empty(dataProcessingAgreement.SystemUsages);
        }

        [Fact]
        public void Cannot_RemoveSystem_If_System_Is_Not_Found()
        {
            //Arrange
            var organizationId = A<int>();
            var systemId = A<int>();
            var dataProcessingAgreement = new DataProcessingAgreement { OrganizationId = organizationId };

            _systemRepository.Setup(x => x.GetSystem(systemId)).Returns(default(ItSystem));

            //Act
            var result = _sut.RemoveSystem(dataProcessingAgreement, systemId);

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
            var itSystem = new ItSystem
            {
                //Add usage in different organization than dpa
                Usages = { itSystemUsage }
            };

            var dataProcessingAgreement = new DataProcessingAgreement { OrganizationId = organizationId, SystemUsages = { itSystemUsage } };

            _systemRepository.Setup(x => x.GetSystem(systemId)).Returns(itSystem);

            //Act
            var result = _sut.RemoveSystem(dataProcessingAgreement, systemId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            Assert.NotEmpty(dataProcessingAgreement.SystemUsages);
        }

        private void ExpectSystemsInUse(int organizationId, params ItSystem[] systems)
        {
            _systemRepository.Setup(x => x.GetSystemsInUse(organizationId))
                .Returns(systems.AsQueryable());
        }
    }
}
