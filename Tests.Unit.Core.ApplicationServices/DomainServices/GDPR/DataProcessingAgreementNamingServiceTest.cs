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
    public class DataProcessingAgreementNamingServiceTest : WithAutoFixture
    {
        private readonly DataProcessingAgreementNamingService _sut;
        private readonly Mock<IDataProcessingAgreementRepository> _repositoryMock;

        public DataProcessingAgreementNamingServiceTest()
        {
            _repositoryMock = new Mock<IDataProcessingAgreementRepository>();
            _sut = new DataProcessingAgreementNamingService(_repositoryMock.Object);
        }

        [Fact]
        public void Can_ChangeName()
        {
            //Arrange
            var name = A<string>();
            var dataProcessingAgreement = new DataProcessingAgreement();

            //Act
            var result = _sut.ChangeName(dataProcessingAgreement, name);

            //Assert
            Assert.True(result.IsNone);
            Assert.Equal(name, dataProcessingAgreement.Name);
        }

        [Fact]
        public void ChangeName_Returns_Conflict()
        {
            //Arrange
            var name = A<string>();
            var dataProcessingAgreement = new DataProcessingAgreement() { OrganizationId = A<int>(), Id =  A<int>()};
            ExpectSearchReturns(dataProcessingAgreement.OrganizationId, name, new List<DataProcessingAgreement> { new DataProcessingAgreement(){Id =  dataProcessingAgreement.Id+1} });

            //Act
            var result = _sut.ChangeName(dataProcessingAgreement, name);

            //Assert
            AssertModificationFailure(result, OperationFailure.Conflict);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901")] //101
        [InlineData(null)]
        public void ChangeName_Returns_BadInput(string name)
        {
            //Arrange
            var dataProcessingAgreement = new DataProcessingAgreement() { Name = A<string>() };

            //Act
            var result = _sut.ChangeName(dataProcessingAgreement, name);

            //Assert
            AssertModificationFailure(result, OperationFailure.BadInput);
        }

        private void AssertModificationFailure(Maybe<OperationError> result, OperationFailure operationFailure)
        {
            _repositoryMock.Verify(x => x.Update(It.IsAny<DataProcessingAgreement>()), Times.Never);
            Assert.True(result.HasValue);
            Assert.Equal(operationFailure, result.Value.FailureType);
        }

        private void ExpectSearchReturns(int organizationId, Maybe<string> name, IEnumerable<DataProcessingAgreement> dataProcessingAgreements)
        {
            _repositoryMock.Setup(x => x.Search(organizationId, name)).Returns(dataProcessingAgreements.AsQueryable());
        }
    }
}
