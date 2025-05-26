using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.Events;
using Core.DomainModel.GDPR;
using Core.DomainModel.GDPR.Events;
using Core.DomainModel.ItContract;
using Core.DomainServices.Contract;
using Core.DomainServices.Repositories.GDPR;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.DomainServices.Contract
{
    public class ContractDataProcessingRegistrationAssignmentServiceTest : WithAutoFixture
    {
        private readonly Mock<IDataProcessingRegistrationRepository> _dataProcessingRegistrationRepository;
        private readonly Mock<IDomainEvents> _domainEvents;
        private readonly ContractDataProcessingRegistrationAssignmentService _sut;

        public ContractDataProcessingRegistrationAssignmentServiceTest()
        {
            _dataProcessingRegistrationRepository = new Mock<IDataProcessingRegistrationRepository>();
            _domainEvents = new Mock<IDomainEvents>();
            _sut = new ContractDataProcessingRegistrationAssignmentService(_dataProcessingRegistrationRepository.Object, _domainEvents.Object);
        }

        [Fact]
        public void Can_GetApplicableDataProcessingRegistrations()
        {
            //Arrange
            var alreadyAssignedDataProcessingRegistration = new DataProcessingRegistration { Id = A<int>() };
            var availableDataProcessingRegistration = new DataProcessingRegistration { Id = A<int>() };

            var organizationId = A<int>();

            var contract = new ItContract
            {
                OrganizationId = organizationId,
                DataProcessingRegistrations = { alreadyAssignedDataProcessingRegistration }
            };
            ExpectDataProcessingRegistrationInOrganization(organizationId, availableDataProcessingRegistration, alreadyAssignedDataProcessingRegistration);

            //Act
            var applicableDataProcessingRegistrations = _sut.GetApplicableDataProcessingRegistrations(contract);

            //Assert
            var registration = Assert.Single(applicableDataProcessingRegistrations);
            Assert.Same(availableDataProcessingRegistration, registration);
        }

        [Fact]
        public void Can_AssignDataProcessingRegistration()
        {
            //Arrange
            var dataProcessingRegistrationId = A<int>();
            var contract = new ItContract { OrganizationId = A<int>() };
            var dataProcessingRegistration = new DataProcessingRegistration { OrganizationId = contract.OrganizationId };
            _dataProcessingRegistrationRepository.Setup(x => x.GetById(dataProcessingRegistrationId)).Returns(dataProcessingRegistration);

            //Act
            var result = _sut.AssignDataProcessingRegistration(contract, dataProcessingRegistrationId);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(dataProcessingRegistration, result.Value);
            Assert.True(contract.DataProcessingRegistrations.Contains(dataProcessingRegistration));
        }

        [Fact]
        public void Cannot_AssignDataProcessingRegistration_If_Registration_Is_Not_Found()
        {
            //Arrange
            var contract = new ItContract { OrganizationId = A<int>() };
            var dataProcessingRegistrationid = A<int>();

            _dataProcessingRegistrationRepository.Setup(x => x.GetById(dataProcessingRegistrationid)).Returns(default(DataProcessingRegistration));

            //Act
            var result = _sut.AssignDataProcessingRegistration(contract, dataProcessingRegistrationid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            Assert.Empty(contract.DataProcessingRegistrations);
        }

        [Fact]
        public void Can_RemoveDataProcessingRegistration()
        {
            //Arrange
            var organizationId = A<int>();
            var registration = new DataProcessingRegistration { OrganizationId = organizationId, Id = A<int>() };
            var contract = new ItContract { Id=A<int>(), OrganizationId = organizationId, DataProcessingRegistrations = { registration } };

            _dataProcessingRegistrationRepository.Setup(x => x.GetById(registration.Id)).Returns(registration);
            _domainEvents.Setup(x =>
                x.Raise(It.Is<DataProcessingRegistrationRemovedFromItContractEvent>(dprEvent =>
                    dprEvent.ItContract.Id == contract.Id && dprEvent.DataProcessingRegistration.Id == registration.Id)));

            //Act
            var result = _sut.RemoveDataProcessingRegistration(contract, registration.Id);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(registration, result.Value);
            Assert.Empty(contract.DataProcessingRegistrations);
        }

        [Fact]
        public void Cannot_RemoveDataProcessingRegistration_If_Registration_Is_Not_Found()
        {
            //Arrange
            var organizationId = A<int>();
            var dataProcessingRegistrationid = A<int>();
            var contract = new ItContract { OrganizationId = A<int>() };

            _dataProcessingRegistrationRepository.Setup(x => x.GetById(dataProcessingRegistrationid)).Returns(default(DataProcessingRegistration));

            //Act
            var result = _sut.RemoveDataProcessingRegistration(contract, dataProcessingRegistrationid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_RemoveDataProcessingRegistration_If_Registration_Is_Not_AssignedAlready()
        {
            //Arrange
            var organizationId = A<int>();
            var registration = new DataProcessingRegistration { OrganizationId = organizationId, Id = A<int>() };
            var contract = new ItContract { OrganizationId = organizationId, DataProcessingRegistrations = { new DataProcessingRegistration() } };

            _dataProcessingRegistrationRepository.Setup(x => x.GetById(registration.Id)).Returns(registration);

            //Act
            var result = _sut.RemoveDataProcessingRegistration(contract, registration.Id);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            Assert.NotEmpty(contract.DataProcessingRegistrations);
        }

        private void ExpectDataProcessingRegistrationInOrganization(int organizationId, params DataProcessingRegistration[] dataProcessingRegistrations)
        {
            _dataProcessingRegistrationRepository.Setup(x => x.GetDataProcessingRegistrationsFromOrganization(organizationId))
                .Returns(dataProcessingRegistrations.AsQueryable());
        }
    }
}
