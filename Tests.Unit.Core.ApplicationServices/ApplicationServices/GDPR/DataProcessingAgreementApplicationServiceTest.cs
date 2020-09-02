using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.GDPR;
using Core.DomainModel.GDPR;
using Core.DomainModel.Result;
using Core.DomainServices.Authorization;
using Core.DomainServices.GDPR;
using Core.DomainServices.Repositories.GDPR;
using Infrastructure.Services.Types;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.GDPR
{
    public class DataProcessingAgreementApplicationServiceTest : WithAutoFixture
    {
        private readonly DataProcessingAgreementApplicationService _sut;
        private readonly Mock<IAuthorizationContext> _authorizationContextMock;
        private readonly Mock<IDataProcessingAgreementRepository> _repositoryMock;

        public DataProcessingAgreementApplicationServiceTest()
        {
            _authorizationContextMock = new Mock<IAuthorizationContext>();
            _repositoryMock = new Mock<IDataProcessingAgreementRepository>();
            _sut = new DataProcessingAgreementApplicationService(_authorizationContextMock.Object, _repositoryMock.Object,new DataProcessingAgreementDomainService(_repositoryMock.Object));
        }

        [Fact]
        public void Can_Create()
        {
            //Arrange
            var organizationId = A<int>();
            var name = A<string>();
            ExpectAllowCreateReturns(organizationId, true);
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
            ExpectAllowCreateReturns(organizationId, true);
            ExpectSearchReturns(organizationId, name, new[] { new DataProcessingAgreement() });

            //Act
            var result = _sut.Create(organizationId, name);

            //Assert
            AssertFailureToCreate(result, OperationFailure.Conflict);
        }

        [Fact]
        public void Can_Create_Returns_Forbidden_If_UnAuthorizedToCreate()
        {
            //Arrange
            var organizationId = A<int>();
            var name = A<string>();
            ExpectAllowCreateReturns(organizationId, false);

            //Act
            var result = _sut.Create(organizationId, name);

            //Assert
            AssertFailureToCreate(result, OperationFailure.Forbidden);
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
            ExpectAllowCreateReturns(organizationId, true);

            //Act
            var result = _sut.Create(organizationId, name);

            //Assert
            AssertFailureToCreate(result, OperationFailure.BadInput);
        }

        [Fact]
        public void Can_Delete()
        {
            //Arrange
            var id = A<int>();
            var dataProcessingAgreement = new DataProcessingAgreement();
            ExpectRepositoryGetToReturn(id, dataProcessingAgreement);
            ExpectAllowDeleteReturns(dataProcessingAgreement, true);

            //Act
            var result = _sut.Delete(id);

            //Assert
            _repositoryMock.Verify(x => x.DeleteById(id), Times.Once);
            Assert.True(result.Ok);
        }

        [Fact]
        public void Delete_Returns_NotFound()
        {
            //Arrange
            var id = A<int>();
            ExpectRepositoryGetToReturn(id, Maybe<DataProcessingAgreement>.None);

            //Act
            var result = _sut.Delete(id);

            //Assert
            AssertFailureToDelete(id, result, OperationFailure.NotFound);
        }

        [Fact]
        public void Delete_Returns_Forbidden()
        {
            //Arrange
            var id = A<int>();
            var dataProcessingAgreement = new DataProcessingAgreement();
            ExpectRepositoryGetToReturn(id, dataProcessingAgreement);
            ExpectAllowDeleteReturns(dataProcessingAgreement, false);

            //Act
            var result = _sut.Delete(id);

            //Assert
            AssertFailureToDelete(id, result, OperationFailure.Forbidden);
        }

        [Fact]
        public void Can_Get()
        {
            //Arrange
            var id = A<int>();
            var dataProcessingAgreement = new DataProcessingAgreement();
            ExpectRepositoryGetToReturn(id, dataProcessingAgreement);
            ExpectAllowReadReturns(dataProcessingAgreement, true);

            //Act
            var result = _sut.Get(id);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(dataProcessingAgreement, result.Value);
        }

        [Fact]
        public void Get_Returns_NotFound()
        {
            //Arrange
            var id = A<int>();
            ExpectRepositoryGetToReturn(id, Maybe<DataProcessingAgreement>.None);

            //Act
            var result = _sut.Get(id);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Fact]
        public void Get_Returns_Forbidden()
        {
            //Arrange
            var id = A<int>();
            var dataProcessingAgreement = new DataProcessingAgreement();
            ExpectRepositoryGetToReturn(id, dataProcessingAgreement);
            ExpectAllowReadReturns(dataProcessingAgreement, false);

            //Act
            var result = _sut.Get(id);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Can_Update_Name()
        {
            //Arrange
            var id = A<int>();
            var name = A<string>();
            var dataProcessingAgreement = new DataProcessingAgreement();
            ExpectRepositoryGetToReturn(id, dataProcessingAgreement);
            ExpectAllowModifyReturns(dataProcessingAgreement, true);

            //Act
            var result = _sut.UpdateName(id, name);

            //Assert
            _repositoryMock.Verify(x => x.Update(dataProcessingAgreement), Times.Once);
            Assert.True(result.Ok);
            Assert.Equal(name, dataProcessingAgreement.Name);
        }

        [Fact]
        public void Update_Name_Returns_NotFound()
        {
            //Arrange
            var id = A<int>();
            var name = A<string>();
            ExpectRepositoryGetToReturn(id, Maybe<DataProcessingAgreement>.None);

            //Act
            var result = _sut.UpdateName(id, name);

            //Assert
            AssertModificationFailure(result, OperationFailure.NotFound);
        }

        [Fact]
        public void Update_Name_Returns_Forbidden()
        {
            //Arrange
            var id = A<int>();
            var name = A<string>();
            var dataProcessingAgreement = new DataProcessingAgreement();
            ExpectRepositoryGetToReturn(id, dataProcessingAgreement);
            ExpectAllowModifyReturns(dataProcessingAgreement, false);

            //Act
            var result = _sut.UpdateName(id, name);

            //Assert
            AssertModificationFailure(result, OperationFailure.Forbidden);
        }

        [Fact]
        public void Update_Name_Returns_Conflict()
        {
            //Arrange
            var id = A<int>();
            var name = A<string>();
            var dataProcessingAgreement = new DataProcessingAgreement() { OrganizationId = A<int>(), Id =  A<int>()};
            ExpectRepositoryGetToReturn(id, dataProcessingAgreement);
            ExpectAllowModifyReturns(dataProcessingAgreement, true);
            ExpectSearchReturns(dataProcessingAgreement.OrganizationId, name, new List<DataProcessingAgreement> { new DataProcessingAgreement(){Id =  dataProcessingAgreement.Id+1} });

            //Act
            var result = _sut.UpdateName(id, name);

            //Assert
            AssertModificationFailure(result, OperationFailure.Conflict);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901")] //101
        [InlineData(null)]
        public void Update_Name_Returns_BadInput(string name)
        {
            //Arrange
            var id = A<int>();
            var dataProcessingAgreement = new DataProcessingAgreement() { Name = A<string>() };
            ExpectRepositoryGetToReturn(id, dataProcessingAgreement);
            ExpectAllowModifyReturns(dataProcessingAgreement, true);

            //Act
            var result = _sut.UpdateName(id, name);

            //Assert
            AssertModificationFailure(result, OperationFailure.BadInput);
        }

        [Fact]
        public void Can_GetOrganizationData()
        {
            //Arrange
            var organizationId = A<int>();
            var skip = 1;
            var take = 1;
            var expectedMatch = new DataProcessingAgreement() { Id = 2 };

            ExpectOrganizationalReadAccess(organizationId, OrganizationDataReadAccessLevel.All);
            ExpectSearchReturns(organizationId, Maybe<string>.None, new[] { new DataProcessingAgreement() { Id = 1 }, expectedMatch, new DataProcessingAgreement() { Id = 3 } });

            //Act
            var organizationData = _sut.GetOrganizationData(organizationId, skip, take);

            //Assert
            Assert.True(organizationData.Ok);
            Assert.Same(expectedMatch, organizationData.Value.Single());
        }

        [Theory]
        [InlineData(OrganizationDataReadAccessLevel.Public)]
        [InlineData(OrganizationDataReadAccessLevel.None)]
        public void GetOrganizationData_Returns_Forbidden(OrganizationDataReadAccessLevel accessLevel)
        {
            //Arrange
            var organizationId = A<int>();
            var skip = 1;
            var take = 1;
            var expectedMatch = new DataProcessingAgreement() { Id = 2 };

            ExpectOrganizationalReadAccess(organizationId, accessLevel);

            //Act
            var organizationData = _sut.GetOrganizationData(organizationId, skip, take);

            //Assert
            Assert.True(organizationData.Failed);
            Assert.Equal(OperationFailure.Forbidden, organizationData.Error.FailureType);
        }

        [Theory]
        [InlineData(-1, 1)]
        [InlineData(0, 0)]
        [InlineData(0, 101)]
        public void GetOrganizationData_Returns_BadInput(int skip, int take)
        {
            //Arrange
            var organizationId = A<int>();

            ExpectOrganizationalReadAccess(organizationId, OrganizationDataReadAccessLevel.All);

            //Act
            var organizationData = _sut.GetOrganizationData(organizationId, skip, take);

            //Assert
            Assert.True(organizationData.Failed);
            Assert.Equal(OperationFailure.BadInput, organizationData.Error.FailureType);
        }

        private void ExpectOrganizationalReadAccess(int organizationId, OrganizationDataReadAccessLevel organizationDataReadAccessLevel)
        {
            _authorizationContextMock.Setup(x => x.GetOrganizationReadAccessLevel(organizationId)).Returns(organizationDataReadAccessLevel);
        }

        private void AssertModificationFailure(Result<DataProcessingAgreement, OperationError> result, OperationFailure operationFailure)
        {
            _repositoryMock.Verify(x => x.Update(It.IsAny<DataProcessingAgreement>()), Times.Never);
            Assert.True(result.Failed);
            Assert.Equal(operationFailure, result.Error.FailureType);
        }

        private void ExpectAllowModifyReturns(DataProcessingAgreement dataProcessingAgreement, bool value)
        {
            _authorizationContextMock.Setup(x => x.AllowModify(dataProcessingAgreement)).Returns(value);
        }

        private void ExpectAllowReadReturns(DataProcessingAgreement dataProcessingAgreement, bool value)
        {
            _authorizationContextMock.Setup(x => x.AllowReads(dataProcessingAgreement)).Returns(value);
        }

        private void AssertFailureToDelete(int id, Result<DataProcessingAgreement, OperationError> result, OperationFailure operationFailure)
        {
            _repositoryMock.Verify(x => x.DeleteById(id), Times.Never);
            Assert.True(result.Failed);
            Assert.Equal(operationFailure, result.Error.FailureType);
        }

        private void ExpectAllowDeleteReturns(DataProcessingAgreement dataProcessingAgreement, bool value)
        {
            _authorizationContextMock.Setup(x => x.AllowDelete(dataProcessingAgreement)).Returns(value);
        }

        private void ExpectRepositoryGetToReturn(int id, Maybe<DataProcessingAgreement> dataProcessingAgreement)
        {
            _repositoryMock.Setup(x => x.GetById(id)).Returns(dataProcessingAgreement);
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

        private void ExpectAllowCreateReturns(int organizationId, bool value)
        {
            _authorizationContextMock.Setup(x => x.AllowCreate<DataProcessingAgreement>(organizationId)).Returns(value);
        }
    }
}
