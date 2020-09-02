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
    public class DataProcessingAgreemenDomainServiceTest : WithAutoFixture
    {
        private readonly DataProcessingAgreementDomainService _sut;
        private readonly Mock<IDataProcessingAgreementRepository> _repositoryMock;

        public DataProcessingAgreemenDomainServiceTest()
        {
            _repositoryMock = new Mock<IDataProcessingAgreementRepository>();
            _sut = new DataProcessingAgreementDomainService(_repositoryMock.Object);
        }

        [Fact]
        public void Can_Create()
        {
            //Arrange
            var organizationId = A<int>();
            var name = A<string>();
            ExpectSearchReturns(organizationId, name, Enumerable.Empty<DataProcessingAgreement>());
            _repositoryMock.Setup(x => x.Add(It.IsAny<DataProcessingAgreement>())).Returns<DataProcessingAgreement>(dataProcessingAgreement => dataProcessingAgreement);

            //Act
            var result = _sut.Create(organizationId, name);

            //Assert
            Assert.True(result.Ok);
        }

        [Fact]
        public void Can_Create_Returns_Conflict_If_Existing_Item_With_Same_Name()
        {
            //Arrange
            var organizationId = A<int>();
            var name = A<string>();
            ExpectSearchReturns(organizationId, name, new[] { new DataProcessingAgreement() });

            //Act
            var result = _sut.Create(organizationId, name);

            //Assert
            AssertFailureToCreate(result, OperationFailure.Conflict);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901")] //101
        [InlineData(null)]
        public void Can_Create_Returns_BadInput_If_Name_Is_Invalid_UnAuthorizedToCreate(string name)
        {
            //Arrange
            var organizationId = A<int>();

            //Act
            var result = _sut.Create(organizationId, name);

            //Assert
            AssertFailureToCreate(result, OperationFailure.BadInput);
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
            _repositoryMock.Verify(x => x.Update(dataProcessingAgreement), Times.Once);
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

        private void AssertFailureToCreate(Result<DataProcessingAgreement, OperationError> result, OperationFailure operationFailure)
        {
            _repositoryMock.Verify(x => x.Add(It.IsAny<DataProcessingAgreement>()), Times.Never);
            Assert.True(result.Failed);
            Assert.Equal(operationFailure, result.Error.FailureType);
        }

        private void ExpectSearchReturns(int organizationId, Maybe<string> name, IEnumerable<DataProcessingAgreement> dataProcessingAgreements)
        {
            _repositoryMock.Setup(x => x.Search(organizationId, name)).Returns(dataProcessingAgreements.AsQueryable());
        }
    }
}
