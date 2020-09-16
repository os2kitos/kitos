using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.GDPR;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.GDPR;
using Core.DomainServices.Repositories.GDPR;
using Core.DomainServices.Repositories.Reference;
using Infrastructure.Services.DataAccess;
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
        private readonly Mock<IDataProcessingAgreementNamingService> _domainServiceMock;
        private readonly Mock<IDataProcessingAgreementRoleAssignmentsService> _roleAssignmentServiceMock;
        private readonly Mock<IReferenceRepository> _referenceRepositoryMock;
        private readonly Mock<ITransactionManager> _transactionManagerMock;
        private readonly Mock<IGenericRepository<DataProcessingAgreementRight>> _rightsRepositoryMock;

        public DataProcessingAgreementApplicationServiceTest()
        {
            _authorizationContextMock = new Mock<IAuthorizationContext>();
            _repositoryMock = new Mock<IDataProcessingAgreementRepository>();
            _domainServiceMock = new Mock<IDataProcessingAgreementNamingService>();
            _roleAssignmentServiceMock = new Mock<IDataProcessingAgreementRoleAssignmentsService>();
            _referenceRepositoryMock = new Mock<IReferenceRepository>();
            _transactionManagerMock = new Mock<ITransactionManager>();
            _rightsRepositoryMock = new Mock<IGenericRepository<DataProcessingAgreementRight>>();
            _sut = new DataProcessingAgreementApplicationService(_authorizationContextMock.Object,
                _repositoryMock.Object,
                _domainServiceMock.Object,
                _roleAssignmentServiceMock.Object,
                _referenceRepositoryMock.Object,
                _transactionManagerMock.Object,
                _rightsRepositoryMock.Object);
        }

        [Fact]
        public void Can_Create()
        {
            //Arrange
            var organizationId = A<int>();
            var name = A<string>();

            ExpectAllowCreateReturns(organizationId, true);
            _domainServiceMock.Setup(x => x.ValidateSuggestedNewAgreement(organizationId, name)).Returns(Maybe<OperationError>.None);
            _repositoryMock
                .Setup(x => x.Add(It.Is<DataProcessingAgreement>(dpa =>
                    dpa.OrganizationId == organizationId && name == dpa.Name)))
                .Returns<DataProcessingAgreement>(x => x);

            //Act
            var result = _sut.Create(organizationId, name);

            //Assert
            Assert.True(result.Ok);
        }

        [Fact]
        public void Can_Create_Returns_Error_If_Present()
        {
            //Arrange
            var organizationId = A<int>();
            var name = A<string>();
            var operationError = new OperationError(A<OperationFailure>());

            _domainServiceMock.Setup(x => x.ValidateSuggestedNewAgreement(organizationId, name)).Returns(operationError);
            ExpectAllowCreateReturns(organizationId, true);

            //Act
            var result = _sut.Create(organizationId, name);

            //Assert
            AssertFailureToCreate(result, operationError.FailureType);
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
            _domainServiceMock.Setup(x => x.ChangeName(dataProcessingAgreement, name)).Returns(Maybe<OperationError>.None);

            //Act
            var result = _sut.UpdateName(id, name);

            //Assert
            Assert.True(result.Ok);
            _repositoryMock.Verify(x => x.Update(dataProcessingAgreement), Times.Once);
        }

        [Fact]
        public void Update_Name_Returns_Error_From_DomainService()
        {
            //Arrange
            var id = A<int>();
            var name = A<string>();
            var dataProcessingAgreement = new DataProcessingAgreement();
            var operationError = A<OperationError>();
            ExpectRepositoryGetToReturn(id, dataProcessingAgreement);
            ExpectAllowModifyReturns(dataProcessingAgreement, true);
            _domainServiceMock.Setup(x => x.ChangeName(dataProcessingAgreement, name)).Returns(operationError);

            //Act
            var result = _sut.UpdateName(id, name);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(operationError.FailureType, result.Error.FailureType);
            _repositoryMock.Verify(x => x.Update(dataProcessingAgreement), Times.Never);
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

        [Fact]
        public void Can_GetAvailableRoles_If_ReadAccess_Is_Permitted()
        {
            //Arrange
            var agreementId = A<int>();
            var dataProcessingAgreement = new DataProcessingAgreement();
            var agreementRoles = new[] { new DataProcessingAgreementRole(), new DataProcessingAgreementRole() };

            ExpectRepositoryGetToReturn(agreementId, dataProcessingAgreement);
            ExpectAllowReadReturns(dataProcessingAgreement, true);
            _roleAssignmentServiceMock.Setup(x => x.GetApplicableRoles(dataProcessingAgreement)).Returns(agreementRoles);

            //Act
            var result = _sut.GetAvailableRoles(agreementId);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(agreementRoles, result.Value.roles);
        }

        [Fact]
        public void Cannot_GetAvailableRoles_If_ReadAccess_Is_Denied()
        {
            //Arrange
            var agreementId = A<int>();
            var dataProcessingAgreement = new DataProcessingAgreement();

            ExpectRepositoryGetToReturn(agreementId, dataProcessingAgreement);
            ExpectAllowReadReturns(dataProcessingAgreement, false);

            //Act
            var result = _sut.GetAvailableRoles(agreementId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_GetAvailableRoles_If_Dpa_Is_Not_Found()
        {
            //Arrange
            var agreementId = A<int>();

            ExpectRepositoryGetToReturn(agreementId, Maybe<DataProcessingAgreement>.None);

            //Act
            var result = _sut.GetAvailableRoles(agreementId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Fact]
        public void Can_GetUsersWhichCanBeAssignedToRole_If_ReadAccess_Is_Permitted()
        {
            //Arrange
            var agreementId = A<int>();
            var roleId = A<int>();
            var dataProcessingAgreement = new DataProcessingAgreement();
            var nameEmailQuery = A<string>();
            var pageSize = new Random().Next(1, 10);
            var usersFromService = Enumerable.Range(0, pageSize + 1).Select((index) => new User { Id = index }).ToList(); //one more than pagesize to verify

            ExpectRepositoryGetToReturn(agreementId, dataProcessingAgreement);
            ExpectAllowReadReturns(dataProcessingAgreement, true);
            _roleAssignmentServiceMock
                .Setup(x => x.GetUsersWhichCanBeAssignedToRole(dataProcessingAgreement, roleId,
                    It.Is<Maybe<string>>(m => m.Select(q => q == nameEmailQuery).GetValueOrDefault())))
                .Returns(Result<IQueryable<User>, OperationError>.Success(usersFromService.AsQueryable()));

            //Act
            var result = _sut.GetUsersWhichCanBeAssignedToRole(agreementId, roleId, nameEmailQuery, pageSize);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(usersFromService.Take(pageSize), result.Value);
        }

        [Fact]
        public void Cannot_GetUsersWhichCanBeAssignedToRole_If_ReadAccess_Is_Denied()
        {
            //Arrange
            var agreementId = A<int>();
            var dataProcessingAgreement = new DataProcessingAgreement();

            ExpectRepositoryGetToReturn(agreementId, dataProcessingAgreement);
            ExpectAllowReadReturns(dataProcessingAgreement, false);

            //Act
            var result = _sut.GetUsersWhichCanBeAssignedToRole(agreementId, A<int>(), A<string>(), 13);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_GetUsersWhichCanBeAssignedToRole_If_Dpa_Is_Not_found()
        {
            //Arrange
            var agreementId = A<int>();
            ExpectRepositoryGetToReturn(agreementId, Maybe<DataProcessingAgreement>.None);

            //Act
            var result = _sut.GetUsersWhichCanBeAssignedToRole(agreementId, A<int>(), A<string>(), 13);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_AssignRole_If_WriteAccess_Is_Permitted(bool serviceSucceeds)
        {
            //Arrange
            var agreementId = A<int>();
            var roleId = A<int>();
            var userId = A<int>();
            var dataProcessingAgreement = new DataProcessingAgreement();
            var transaction = new Mock<IDatabaseTransaction>();
            var serviceResult = serviceSucceeds
                ? Result<DataProcessingAgreementRight, OperationError>.Success(new DataProcessingAgreementRight())
                : Result<DataProcessingAgreementRight, OperationError>.Failure(A<OperationError>());

            ExpectRepositoryGetToReturn(agreementId, dataProcessingAgreement);
            ExpectAllowModifyReturns(dataProcessingAgreement, true);
            _transactionManagerMock.Setup(x => x.Begin(IsolationLevel.ReadCommitted)).Returns(transaction.Object);
            _roleAssignmentServiceMock.Setup(x => x.AssignRole(dataProcessingAgreement, roleId, userId)).Returns(serviceResult);

            //Act

            var result = _sut.AssignRole(agreementId, roleId, userId);

            //Assert
            Assert.Equal(serviceSucceeds, result.Ok);
            Assert.Same(serviceResult, result);
            VerifyExpectedDbSideEffect(serviceSucceeds, dataProcessingAgreement, transaction);
        }

        [Fact]
        public void Cannot_AssignRole_If_WriteAccess_Is_Denied()
        {
            //Arrange
            var agreementId = A<int>();
            var dataProcessingAgreement = new DataProcessingAgreement();
            ExpectRepositoryGetToReturn(agreementId, dataProcessingAgreement);
            ExpectAllowModifyReturns(dataProcessingAgreement, false);

            //Act
            var result = _sut.AssignRole(agreementId, A<int>(), A<int>());

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_AssignRole_If_Dpa_Is_Not_found()
        {
            //Arrange
            var agreementId = A<int>();
            ExpectRepositoryGetToReturn(agreementId, Maybe<DataProcessingAgreement>.None);

            //Act
            var result = _sut.AssignRole(agreementId, A<int>(), A<int>());

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_RemoveRole_If_WriteAccess_Is_Permitted(bool serviceSucceeds)
        {
            //Arrange
            var agreementId = A<int>();
            var roleId = A<int>();
            var userId = A<int>();
            var dataProcessingAgreement = new DataProcessingAgreement();
            var transaction = new Mock<IDatabaseTransaction>();
            var agreementRight = new DataProcessingAgreementRight();
            var serviceResult = serviceSucceeds
                ? Result<DataProcessingAgreementRight, OperationError>.Success(agreementRight)
                : Result<DataProcessingAgreementRight, OperationError>.Failure(A<OperationError>());

            ExpectRepositoryGetToReturn(agreementId, dataProcessingAgreement);
            ExpectAllowModifyReturns(dataProcessingAgreement, true);
            _transactionManagerMock.Setup(x => x.Begin(IsolationLevel.ReadCommitted)).Returns(transaction.Object);
            _roleAssignmentServiceMock.Setup(x => x.RemoveRole(dataProcessingAgreement, roleId, userId)).Returns(serviceResult);

            //Act
            var result = _sut.RemoveRole(agreementId, roleId, userId);

            //Assert
            Assert.Equal(serviceSucceeds, result.Ok);
            Assert.Same(serviceResult, result);
            VerifyExpectedDbSideEffect(serviceSucceeds, dataProcessingAgreement, transaction);
            _rightsRepositoryMock.Verify(x => x.Delete(agreementRight), serviceSucceeds ? Times.Once() : Times.Never());
        }

        [Fact]
        public void Cannot_RemoveRole_If_WriteAccess_Is_Denied()
        {
            //Arrange
            var agreementId = A<int>();
            var dataProcessingAgreement = new DataProcessingAgreement();
            ExpectRepositoryGetToReturn(agreementId, dataProcessingAgreement);
            ExpectAllowModifyReturns(dataProcessingAgreement, false);

            //Act
            var result = _sut.RemoveRole(agreementId, A<int>(), A<int>());

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_RemoveRole_If_Dpa_Is_Not_found()
        {
            //Arrange
            var agreementId = A<int>();
            ExpectRepositoryGetToReturn(agreementId, Maybe<DataProcessingAgreement>.None);

            //Act
            var result = _sut.RemoveRole(agreementId, A<int>(), A<int>());

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        private void VerifyExpectedDbSideEffect(bool expectSideEffect, DataProcessingAgreement dataProcessingAgreement, Mock<IDatabaseTransaction> transaction)
        {
            _repositoryMock.Verify(x => x.Update(dataProcessingAgreement), expectSideEffect ? Times.Once() : Times.Never());
            transaction.Verify(x => x.Commit(), expectSideEffect ? Times.Once() : Times.Never());
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

        private static void AssertFailureToCreate(Result<DataProcessingAgreement, OperationError> result, OperationFailure operationFailure)
        {
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
