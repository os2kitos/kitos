using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.GDPR;
using Core.DomainModel.Result;
using Core.DomainServices.GDPR;
using Core.DomainServices.Repositories.GDPR;
using Infrastructure.Services.Types;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.DomainServices.GDPR
{
    public class DataProcessingRegistrationNamingServiceTest : WithAutoFixture
    {
        private readonly DataProcessingRegistrationNamingService _sut;
        private readonly Mock<IDataProcessingRegistrationRepository> _repositoryMock;

        public DataProcessingRegistrationNamingServiceTest()
        {
            _repositoryMock = new Mock<IDataProcessingRegistrationRepository>();
            _sut = new DataProcessingRegistrationNamingService(_repositoryMock.Object);
        }

        [Fact]
        public void Can_ChangeName()
        {
            //Arrange
            var name = A<string>();
            var registration = new DataProcessingRegistration();

            //Act
            var result = _sut.ChangeName(registration, name);

            //Assert
            Assert.True(result.IsNone);
            Assert.Equal(name, registration.Name);
        }

        [Fact]
        public void ChangeName_Returns_Conflict()
        {
            //Arrange
            var name = A<string>();
            var registration = new DataProcessingRegistration() { OrganizationId = A<int>(), Id =  A<int>()};
            ExpectSearchReturns(registration.OrganizationId, name, new List<DataProcessingRegistration> { new DataProcessingRegistration(){Id =  registration.Id+1} });

            //Act
            var result = _sut.ChangeName(registration, name);

            //Assert
            AssertModificationFailure(result, OperationFailure.Conflict);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789011234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890")] //201
        [InlineData(null)]
        public void ChangeName_Returns_BadInput(string name)
        {
            //Arrange
            var registration = new DataProcessingRegistration() { Name = A<string>() };

            //Act
            var result = _sut.ChangeName(registration, name);

            //Assert
            AssertModificationFailure(result, OperationFailure.BadInput);
        }

        private void AssertModificationFailure(Maybe<OperationError> result, OperationFailure operationFailure)
        {
            _repositoryMock.Verify(x => x.Update(It.IsAny<DataProcessingRegistration>()), Times.Never);
            Assert.True(result.HasValue);
            Assert.Equal(operationFailure, result.Value.FailureType);
        }

        private void ExpectSearchReturns(int organizationId, Maybe<string> name, IEnumerable<DataProcessingRegistration> registrations)
        {
            _repositoryMock.Setup(x => x.Search(organizationId, name)).Returns(registrations.AsQueryable());
        }
    }
}
