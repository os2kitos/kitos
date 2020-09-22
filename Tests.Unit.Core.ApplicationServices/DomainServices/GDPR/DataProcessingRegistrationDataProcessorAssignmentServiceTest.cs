using System.Linq;
using Core.DomainModel.GDPR;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainServices.GDPR;
using Core.DomainServices.Repositories.Organization;
using Infrastructure.Services.Types;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.DomainServices.GDPR
{
    public class DataProcessingRegistrationDataProcessorAssignmentServiceTest : WithAutoFixture
    {
        private readonly Mock<IOrganizationRepository> _organizationRepositoryMock;
        private readonly DataProcessingRegistrationDataProcessorAssignmentService _sut;

        public DataProcessingRegistrationDataProcessorAssignmentServiceTest()
        {
            _organizationRepositoryMock = new Mock<IOrganizationRepository>();
            _sut = new DataProcessingRegistrationDataProcessorAssignmentService(_organizationRepositoryMock.Object);
        }

        [Fact]
        public void GetApplicableDataProcessors_Returns_Organizations_Not_Already_Assigned()
        {
            //Arrange
            var existingProcessor = new Organization { Id = A<int>() };
            var validCandidate1 = new Organization { Id = A<int>() };
            var validCandidate2 = new Organization { Id = A<int>() };
            var dataProcessingRegistration = new DataProcessingRegistration() { DataProcessors = { existingProcessor } };

            ExpectGetAllOrganizationsReturn(validCandidate1, validCandidate2, existingProcessor);

            //Act
            var applicableDataProcessors = _sut.GetApplicableDataProcessors(dataProcessingRegistration).ToList();

            //Assert
            Assert.Equal(2, applicableDataProcessors.Count);
            Assert.Equal(new[] { validCandidate1, validCandidate2 }, applicableDataProcessors);
        }

        [Fact]
        public void Can_AssignDataProcessor()
        {
            //Arrange
            var existingProcessor = new Organization { Id = A<int>() };
            var validCandidate = new Organization { Id = A<int>() };
            var dataProcessingRegistration = new DataProcessingRegistration() { DataProcessors = { existingProcessor } };

            ExpectGetOrganizationByIdReturns(validCandidate.Id, validCandidate);

            //Act
            var result = _sut.AssignDataProcessor(dataProcessingRegistration, validCandidate.Id);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(validCandidate, result.Value);
            Assert.True(dataProcessingRegistration.DataProcessors.Contains(validCandidate));
        }

        [Fact]
        public void Cannot_AssignDataProcessor_If_Not_Applicable()
        {
            //Arrange
            var existingProcessor = new Organization { Id = A<int>() };
            var dpId = A<int>();
            var dataProcessingRegistration = new DataProcessingRegistration() { DataProcessors = { existingProcessor } };

            ExpectGetOrganizationByIdReturns(dpId, Maybe<Organization>.None);

            //Act
            var result = _sut.AssignDataProcessor(dataProcessingRegistration, dpId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_AssignDataProcessor_If_Already_Assigned()
        {
            //Arrange
            var existingProcessor = new Organization { Id = A<int>() };
            var dataProcessingRegistration = new DataProcessingRegistration() { DataProcessors = { existingProcessor } };

            ExpectGetOrganizationByIdReturns(existingProcessor.Id, existingProcessor);

            //Act
            var result = _sut.AssignDataProcessor(dataProcessingRegistration, existingProcessor.Id);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Conflict, result.Error.FailureType);
        }

        [Fact]
        public void Can_RemoveDataProcessor()
        {
            //Arrange
            var existingProcessor = new Organization { Id = A<int>() };
            var dataProcessingRegistration = new DataProcessingRegistration() { DataProcessors = { existingProcessor } };

            ExpectGetOrganizationByIdReturns(existingProcessor.Id, existingProcessor);

            //Act
            var result = _sut.RemoveDataProcessor(dataProcessingRegistration, existingProcessor.Id);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(existingProcessor, result.Value);
            Assert.False(dataProcessingRegistration.DataProcessors.Contains(existingProcessor));
        }

        [Fact]
        public void Cannot_RemoveDataProcessor_If_Not_Applicable()
        {
            //Arrange
            var existingProcessor = new Organization { Id = A<int>() };
            var dpId = A<int>();
            var dataProcessingRegistration = new DataProcessingRegistration { DataProcessors = { existingProcessor } };

            ExpectGetOrganizationByIdReturns(dpId, Maybe<Organization>.None);

            //Act
            var result = _sut.RemoveDataProcessor(dataProcessingRegistration, dpId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_RemoveDataProcessor_If_Not_Present()
        {
            //Arrange
            var organization = new Organization { Id = A<int>() };
            var dataProcessingRegistration = new DataProcessingRegistration();

            ExpectGetOrganizationByIdReturns(organization.Id, organization);

            //Act
            var result = _sut.RemoveDataProcessor(dataProcessingRegistration, organization.Id);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        private void ExpectGetAllOrganizationsReturn(params Organization[] result)
        {
            _organizationRepositoryMock.Setup(x => x.GetAll())
                .Returns(result.AsQueryable());
        }

        private void ExpectGetOrganizationByIdReturns(int id, Maybe<Organization> validCandidate)
        {
            _organizationRepositoryMock.Setup(x => x.GetById(id)).Returns(validCandidate);
        }
    }
}
