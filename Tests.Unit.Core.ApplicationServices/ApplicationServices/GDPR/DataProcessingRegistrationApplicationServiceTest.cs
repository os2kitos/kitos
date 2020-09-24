using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.GDPR;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainModel.Shared;
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
    public class DataProcessingRegistrationApplicationServiceTest : WithAutoFixture
    {
        private readonly DataProcessingRegistrationApplicationService _sut;
        private readonly Mock<IAuthorizationContext> _authorizationContextMock;
        private readonly Mock<IDataProcessingRegistrationRepository> _repositoryMock;
        private readonly Mock<IDataProcessingRegistrationNamingService> _namingServiceMock;
        private readonly Mock<IDataProcessingRegistrationRoleAssignmentsService> _roleAssignmentServiceMock;
        private readonly Mock<IReferenceRepository> _referenceRepositoryMock;
        private readonly Mock<ITransactionManager> _transactionManagerMock;
        private readonly Mock<IGenericRepository<DataProcessingRegistrationRight>> _rightsRepositoryMock;
        private readonly Mock<IDataProcessingRegistrationSystemAssignmentService> _systemAssignmentServiceMock;
        private readonly Mock<IDataProcessingRegistrationDataProcessorAssignmentService> _dpAssignmentService;

        public DataProcessingRegistrationApplicationServiceTest()
        {
            _authorizationContextMock = new Mock<IAuthorizationContext>();
            _repositoryMock = new Mock<IDataProcessingRegistrationRepository>();
            _namingServiceMock = new Mock<IDataProcessingRegistrationNamingService>();
            _roleAssignmentServiceMock = new Mock<IDataProcessingRegistrationRoleAssignmentsService>();
            _referenceRepositoryMock = new Mock<IReferenceRepository>();
            _transactionManagerMock = new Mock<ITransactionManager>();
            _rightsRepositoryMock = new Mock<IGenericRepository<DataProcessingRegistrationRight>>();
            _systemAssignmentServiceMock = new Mock<IDataProcessingRegistrationSystemAssignmentService>();
            _dpAssignmentService = new Mock<IDataProcessingRegistrationDataProcessorAssignmentService>();
            _sut = new DataProcessingRegistrationApplicationService(
                _authorizationContextMock.Object,
                _repositoryMock.Object,
                _namingServiceMock.Object,
                _roleAssignmentServiceMock.Object,
                _referenceRepositoryMock.Object,
                _systemAssignmentServiceMock.Object,
                _dpAssignmentService.Object,
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
            _namingServiceMock.Setup(x => x.ValidateSuggestedNewRegistrationName(organizationId, name)).Returns(Maybe<OperationError>.None);
            _repositoryMock
                .Setup(x => x.Add(It.Is<DataProcessingRegistration>(dpa =>
                    dpa.OrganizationId == organizationId && name == dpa.Name)))
                .Returns<DataProcessingRegistration>(x => x);

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

            _namingServiceMock.Setup(x => x.ValidateSuggestedNewRegistrationName(organizationId, name)).Returns(operationError);
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
            var registration = new DataProcessingRegistration();
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowDeleteReturns(registration, true);

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
            ExpectRepositoryGetToReturn(id, Maybe<DataProcessingRegistration>.None);

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
            var registration = new DataProcessingRegistration();
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowDeleteReturns(registration, false);

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
            var registration = new DataProcessingRegistration();
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowReadReturns(registration, true);

            //Act
            var result = _sut.Get(id);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(registration, result.Value);
        }

        [Fact]
        public void Get_Returns_NotFound()
        {
            //Arrange
            var id = A<int>();
            ExpectRepositoryGetToReturn(id, Maybe<DataProcessingRegistration>.None);

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
            var registration = new DataProcessingRegistration();
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowReadReturns(registration, false);

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
            var registration = new DataProcessingRegistration();
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowModifyReturns(registration, true);
            _namingServiceMock.Setup(x => x.ChangeName(registration, name)).Returns(Maybe<OperationError>.None);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManagerMock.Setup(x => x.Begin(IsolationLevel.ReadCommitted)).Returns(transaction.Object);

            //Act
            var result = _sut.UpdateName(id, name);

            //Assert
            Assert.True(result.Ok);
            transaction.Verify(x => x.Commit());
            _repositoryMock.Verify(x => x.Update(registration), Times.Once);
        }

        [Fact]
        public void Update_Name_Returns_Error_From_DomainService()
        {
            //Arrange
            var id = A<int>();
            var name = A<string>();
            var registration = new DataProcessingRegistration();
            var operationError = A<OperationError>();
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowModifyReturns(registration, true);
            _namingServiceMock.Setup(x => x.ChangeName(registration, name)).Returns(operationError);

            //Act
            var result = _sut.UpdateName(id, name);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(operationError.FailureType, result.Error.FailureType);
            _repositoryMock.Verify(x => x.Update(registration), Times.Never);
        }

        [Fact]
        public void Update_Name_Returns_Forbidden()
        {
            //Arrange
            var id = A<int>();
            var name = A<string>();
            var registration = new DataProcessingRegistration();
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowModifyReturns(registration, false);

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
            var expectedMatch = new DataProcessingRegistration() { Id = 2 };

            ExpectOrganizationalReadAccess(organizationId, OrganizationDataReadAccessLevel.All);
            ExpectSearchReturns(organizationId, Maybe<string>.None, new[] { new DataProcessingRegistration() { Id = 1 }, expectedMatch, new DataProcessingRegistration() { Id = 3 } });

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
            var registration = new DataProcessingRegistration();
            var agreementRoles = new[] { new DataProcessingRegistrationRole(), new DataProcessingRegistrationRole() };

            ExpectRepositoryGetToReturn(agreementId, registration);
            ExpectAllowReadReturns(registration, true);
            _roleAssignmentServiceMock.Setup(x => x.GetApplicableRoles(registration)).Returns(agreementRoles);

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
            var registration = new DataProcessingRegistration();

            ExpectRepositoryGetToReturn(agreementId, registration);
            ExpectAllowReadReturns(registration, false);

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

            ExpectRepositoryGetToReturn(agreementId, Maybe<DataProcessingRegistration>.None);

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
            var registration = new DataProcessingRegistration();
            var nameEmailQuery = A<string>();
            var pageSize = new Random().Next(1, 10);
            var usersFromService = Enumerable.Range(0, pageSize + 1).Select((index) => new User { Id = index }).ToList(); //one more than pagesize to verify

            ExpectRepositoryGetToReturn(agreementId, registration);
            ExpectAllowReadReturns(registration, true);
            _roleAssignmentServiceMock
                .Setup(x => x.GetUsersWhichCanBeAssignedToRole(registration, roleId,
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
            var registration = new DataProcessingRegistration();

            ExpectRepositoryGetToReturn(agreementId, registration);
            ExpectAllowReadReturns(registration, false);

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
            ExpectRepositoryGetToReturn(agreementId, Maybe<DataProcessingRegistration>.None);

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
            var registration = new DataProcessingRegistration();
            var transaction = new Mock<IDatabaseTransaction>();
            var serviceResult = serviceSucceeds
                ? Result<DataProcessingRegistrationRight, OperationError>.Success(new DataProcessingRegistrationRight())
                : Result<DataProcessingRegistrationRight, OperationError>.Failure(A<OperationError>());

            ExpectRepositoryGetToReturn(agreementId, registration);
            ExpectAllowModifyReturns(registration, true);
            _transactionManagerMock.Setup(x => x.Begin(IsolationLevel.ReadCommitted)).Returns(transaction.Object);
            _roleAssignmentServiceMock.Setup(x => x.AssignRole(registration, roleId, userId)).Returns(serviceResult);

            //Act

            var result = _sut.AssignRole(agreementId, roleId, userId);

            //Assert
            Assert.Equal(serviceSucceeds, result.Ok);
            Assert.Same(serviceResult, result);
            VerifyExpectedDbSideEffect(serviceSucceeds, registration, transaction);
        }

        [Fact]
        public void Cannot_AssignRole_If_WriteAccess_Is_Denied()
        {
            //Arrange
            var agreementId = A<int>();
            var registration = new DataProcessingRegistration();
            ExpectRepositoryGetToReturn(agreementId, registration);
            ExpectAllowModifyReturns(registration, false);

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
            ExpectRepositoryGetToReturn(agreementId, Maybe<DataProcessingRegistration>.None);

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
            var registration = new DataProcessingRegistration();
            var transaction = new Mock<IDatabaseTransaction>();
            var agreementRight = new DataProcessingRegistrationRight();
            var serviceResult = serviceSucceeds
                ? Result<DataProcessingRegistrationRight, OperationError>.Success(agreementRight)
                : Result<DataProcessingRegistrationRight, OperationError>.Failure(A<OperationError>());

            ExpectRepositoryGetToReturn(agreementId, registration);
            ExpectAllowModifyReturns(registration, true);
            _transactionManagerMock.Setup(x => x.Begin(IsolationLevel.ReadCommitted)).Returns(transaction.Object);
            _roleAssignmentServiceMock.Setup(x => x.RemoveRole(registration, roleId, userId)).Returns(serviceResult);

            //Act
            var result = _sut.RemoveRole(agreementId, roleId, userId);

            //Assert
            Assert.Equal(serviceSucceeds, result.Ok);
            Assert.Same(serviceResult, result);
            VerifyExpectedDbSideEffect(serviceSucceeds, registration, transaction);
            _rightsRepositoryMock.Verify(x => x.Delete(agreementRight), serviceSucceeds ? Times.Once() : Times.Never());
        }

        [Fact]
        public void Cannot_RemoveRole_If_WriteAccess_Is_Denied()
        {
            //Arrange
            var agreementId = A<int>();
            var registration = new DataProcessingRegistration();
            ExpectRepositoryGetToReturn(agreementId, registration);
            ExpectAllowModifyReturns(registration, false);

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
            ExpectRepositoryGetToReturn(agreementId, Maybe<DataProcessingRegistration>.None);

            //Act
            var result = _sut.RemoveRole(agreementId, A<int>(), A<int>());

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Fact]
        public void Can_Set_Master_Reference_On_Dpa()
        {
            //Arrange
            var agreementId = A<int>();
            var referenceId = A<int>();
            var expectedNewMasterReference = new ExternalReference() { Id = referenceId };

            var registration = new DataProcessingRegistration
            {
                ExternalReferences = { expectedNewMasterReference, new ExternalReference() { Id = A<int>() } }
            };

            ExpectRepositoryGetToReturn(agreementId, registration);
            ExpectAllowModifyReturns(registration, true);
            _referenceRepositoryMock.Setup(x => x.Get(referenceId)).Returns(expectedNewMasterReference);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManagerMock.Setup(x => x.Begin(IsolationLevel.ReadCommitted)).Returns(transaction.Object);

            //Act
            var result = _sut.SetMasterReference(agreementId, referenceId);

            //Assert
            Assert.True(result.Ok);
            transaction.Verify(x => x.Commit());
        }

        [Fact]
        public void Cannot_Set_Master_Reference_On_Dpa_If_ExternalReferenceId_Is_Invalid()
        {
            //Arrange
            var agreementId = A<int>();
            var referenceId = A<int>();

            var registration = new DataProcessingRegistration();
            ExpectRepositoryGetToReturn(agreementId, registration);
            ExpectAllowModifyReturns(registration, true);
            _referenceRepositoryMock.Setup(x => x.Get(referenceId)).Returns(Maybe<ExternalReference>.None);

            //Act
            var result = _sut.SetMasterReference(agreementId, referenceId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_Set_Master_Reference_On_Dpa_If_ExternalReference_Does_Not_Belong_To_Dpa()
        {
            //Arrange
            var agreementId = A<int>();
            var referenceId = A<int>();
            var fetchedMasterReference = new ExternalReference { Id = referenceId };

            var registration = new DataProcessingRegistration
            {
                ExternalReferences = { new ExternalReference { Id = A<int>() }, new ExternalReference { Id = A<int>() } }
            };

            ExpectRepositoryGetToReturn(agreementId, registration);
            ExpectAllowModifyReturns(registration, true);
            _referenceRepositoryMock.Setup(x => x.Get(referenceId)).Returns(fetchedMasterReference);

            //Act
            var result = _sut.SetMasterReference(agreementId, referenceId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_Set_Master_Reference_On_Dpa_If_WriteAccess_Is_Not_Permitted()
        {
            //Arrange
            var agreementId = A<int>();

            var registration = new DataProcessingRegistration();
            ExpectRepositoryGetToReturn(agreementId, registration);
            ExpectAllowModifyReturns(registration, false);

            //Act
            var result = _sut.SetMasterReference(agreementId, A<int>());

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_Set_Master_Reference_On_Dpa_With_Invalid_Dpa()
        {
            //Arrange
            var agreementId = A<int>();
            ExpectRepositoryGetToReturn(agreementId, Maybe<DataProcessingRegistration>.None);

            //Act
            var result = _sut.SetMasterReference(agreementId, A<int>());

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Fact]
        public void Can_GetSystemsWhichCanBeAssigned()
        {
            //Arrange
            var id = A<int>();
            var system1Id = A<int>();
            var system2Id = system1Id + 1;
            var nameQuery = A<string>();
            var registration = new DataProcessingRegistration();
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowReadReturns(registration, true);
            var itSystems = new[] { new ItSystem { Id = system1Id, Name = $"{nameQuery}{1}" }, new ItSystem { Id = system2Id, Name = $"{nameQuery}{2}" } };
            _systemAssignmentServiceMock.Setup(x => x.GetApplicableSystems(registration)).Returns(itSystems.AsQueryable());

            //Act
            var result = _sut.GetSystemsWhichCanBeAssigned(id, nameQuery, new Random().Next(2, 100));

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(itSystems, result.Value);
        }

        [Fact]
        public void Cannot_GetSystemsWhichCanBeAssigned_If_Dpa_Is_Not_Found()
        {
            //Arrange
            var id = A<int>();
            var nameQuery = A<string>();
            ExpectRepositoryGetToReturn(id, Maybe<DataProcessingRegistration>.None);

            //Act
            var result = _sut.GetSystemsWhichCanBeAssigned(id, nameQuery, new Random().Next(2, 100));

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_GetSystemsWhichCanBeAssigned_If_Read_Access_Is_Denied()
        {
            //Arrange
            var id = A<int>();
            var nameQuery = A<string>();
            var registration = new DataProcessingRegistration();
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowReadReturns(registration, false);

            //Act
            var result = _sut.GetSystemsWhichCanBeAssigned(id, nameQuery, new Random().Next(2, 100));

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Can_AssignSystem()
        {
            //Arrange
            var id = A<int>();
            var registration = new DataProcessingRegistration();
            var systemId = A<int>();
            var itSystem = new ItSystem();
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowModifyReturns(registration, true);
            _systemAssignmentServiceMock.Setup(x => x.AssignSystem(registration, systemId)).Returns(itSystem);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManagerMock.Setup(x => x.Begin(IsolationLevel.ReadCommitted)).Returns(transaction.Object);

            //Act
            var result = _sut.AssignSystem(id, systemId);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(itSystem, result.Value);
            transaction.Verify(x => x.Commit());
        }

        [Fact]
        public void Cannot_AssignSystem_If_Dpa_Is_Not_Found()
        {
            //Arrange
            var id = A<int>();
            ExpectRepositoryGetToReturn(id, Maybe<DataProcessingRegistration>.None);

            //Act
            var result = _sut.AssignSystem(id, A<int>());

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_AssignSystem_If_Write_Access_Is_Denied()
        {
            //Arrange
            var id = A<int>();
            var registration = new DataProcessingRegistration();
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowModifyReturns(registration, false);

            //Act
            var result = _sut.AssignSystem(id, A<int>());

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Can_RemoveSystem()
        {
            //Arrange
            var id = A<int>();
            var registration = new DataProcessingRegistration();
            var systemId = A<int>();
            var itSystem = new ItSystem();
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowModifyReturns(registration, true);
            _systemAssignmentServiceMock.Setup(x => x.RemoveSystem(registration, systemId)).Returns(itSystem);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManagerMock.Setup(x => x.Begin(IsolationLevel.ReadCommitted)).Returns(transaction.Object);

            //Act
            var result = _sut.RemoveSystem(id, systemId);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(itSystem, result.Value);
            transaction.Verify(x => x.Commit());
        }

        [Fact]
        public void Cannot_RemoveSystem_If_Dpa_Is_Not_Found()
        {
            //Arrange
            var id = A<int>();
            ExpectRepositoryGetToReturn(id, Maybe<DataProcessingRegistration>.None);

            //Act
            var result = _sut.RemoveSystem(id, A<int>());

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_RemoveSystem_If_Write_Access_Is_Denied()
        {
            //Arrange
            var id = A<int>();
            var registration = new DataProcessingRegistration();
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowModifyReturns(registration, false);

            //Act
            var result = _sut.RemoveSystem(id, A<int>());

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Can_AssignDataProcessor()
        {
            //Arrange
            var id = A<int>();
            var registration = new DataProcessingRegistration();
            var organizationId = A<int>();
            var organization = new Organization();
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowModifyReturns(registration, true);
            _dpAssignmentService.Setup(x => x.AssignDataProcessor(registration, organizationId))
                .Returns(Result<Organization, OperationError>.Success(organization));

            var transaction = ExpectTransaction();

            //Act
            var result = _sut.AssignDataProcessor(id, organizationId);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(organization, result.Value);
            transaction.Verify(x => x.Commit());
        }

        [Fact]
        public void Cannot_AssignDataProcessorIf_Dpa_Is_Not_Found()
        {
            //Arrange
            var id = A<int>();
            ExpectRepositoryGetToReturn(id, Maybe<DataProcessingRegistration>.None);

            //Act
            var result = _sut.AssignDataProcessor(id, A<int>());

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_AssignDataProcessor_If_Write_Access_Is_Denied()
        {
            //Arrange
            var id = A<int>();
            var registration = new DataProcessingRegistration();
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowModifyReturns(registration, false);

            //Act
            var result = _sut.AssignDataProcessor(id, A<int>());

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Can_RemoveDataProcessor()
        {
            //Arrange
            var id = A<int>();
            var registration = new DataProcessingRegistration();
            var organizationId = A<int>();
            var organization = new Organization();
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowModifyReturns(registration, true);
            _dpAssignmentService.Setup(x => x.RemoveDataProcessor(registration, organizationId))
                .Returns(Result<Organization, OperationError>.Success(organization));

            var transaction = ExpectTransaction();

            //Act
            var result = _sut.RemoveDataProcessor(id, organizationId);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(organization, result.Value);
            transaction.Verify(x => x.Commit());
        }

        [Fact]
        public void Cannot_RemoveDataProcessor_If_Dpa_Is_Not_Found()
        {
            //Arrange
            var id = A<int>();
            ExpectRepositoryGetToReturn(id, Maybe<DataProcessingRegistration>.None);

            //Act
            var result = _sut.RemoveDataProcessor(id, A<int>());

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_RemoveDataProcessor_If_Write_Access_Is_Denied()
        {
            //Arrange
            var id = A<int>();
            var registration = new DataProcessingRegistration();
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowModifyReturns(registration, false);

            //Act
            var result = _sut.AssignDataProcessor(id, A<int>());

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Can_GetDataProcessorsWhichCanBeAssigned()
        {
            //Arrange
            var id = A<int>();
            var org1Id = A<int>();
            var org2Id = org1Id + 1;
            var nameQuery = A<string>();
            var registration = new DataProcessingRegistration();
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowReadReturns(registration, true);
            var organizations = new[] { new Organization { Id = org1Id, Name = $"{nameQuery}{1}" }, new Organization { Id = org2Id, Name = $"{nameQuery}{2}" } };
            _dpAssignmentService.Setup(x => x.GetApplicableDataProcessors(registration)).Returns(organizations.AsQueryable());

            //Act
            var result = _sut.GetDataProcessorsWhichCanBeAssigned(id, nameQuery, new Random().Next(2, 100));

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(organizations, result.Value);
        }

        [Fact]
        public void Can_Update_OversightInterval()
        {
            //Arrange
            var id = A<int>();
            var oversightInterval = A<YearMonthIntervalOption>();
            var registration = new DataProcessingRegistration();
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowReadReturns(registration, true);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManagerMock.Setup(x => x.Begin(IsolationLevel.ReadCommitted)).Returns(transaction.Object);

            //Act
            var result = _sut.UpdateOversightInterval(id, oversightInterval);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(oversightInterval,result.Value.OversightInterval);
            transaction.Verify(x => x.Commit());
            _repositoryMock.Verify(x => x.Update(registration),Times.Once);
        }

        [Fact]
        public void Can_Update_OversightInterval_To_Null()
        {
            //Arrange
            var id = A<int>();
            var registration = new DataProcessingRegistration();
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowReadReturns(registration, true);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManagerMock.Setup(x => x.Begin(IsolationLevel.ReadCommitted)).Returns(transaction.Object);

            //Act
            var result = _sut.UpdateOversightInterval(id, null);

            //Assert
            Assert.True(result.Ok);
            Assert.Null(result.Value.OversightInterval);
            transaction.Verify(x => x.Commit());
            _repositoryMock.Verify(x => x.Update(registration), Times.Once);
        }

        [Fact]
        public void Update_OversightInterval_Returns_Forbidden()
        {
            //Arrange
            var id = A<int>();
            var oversightInterval = A<YearMonthIntervalOption>();
            var registration = new DataProcessingRegistration();
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowReadReturns(registration, false);

            //Act
            var result = _sut.UpdateOversightInterval(id, oversightInterval);

            //Assert
            AssertModificationFailure(result,OperationFailure.Forbidden);
        }

        [Fact]
        public void Can_Update_OversightIntervalNote()
        {
            //Arrange
            var id = A<int>();
            var oversightIntervalNote = A<string>();
            var registration = new DataProcessingRegistration();
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowReadReturns(registration, true);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManagerMock.Setup(x => x.Begin(IsolationLevel.ReadCommitted)).Returns(transaction.Object);

            //Act
            var result = _sut.UpdateOversightIntervalNote(id, oversightIntervalNote);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(oversightIntervalNote, result.Value.OversightIntervalNote);
            transaction.Verify(x => x.Commit());
            _repositoryMock.Verify(x => x.Update(registration), Times.Once);
        }

        [Fact]
        public void Update_OversightIntervalNote_Returns_Forbidden()
        {
            //Arrange
            var id = A<int>();
            var oversightIntervalNote = A<string>();
            var registration = new DataProcessingRegistration();
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowReadReturns(registration, false);

            //Act
            var result = _sut.UpdateOversightIntervalNote(id, oversightIntervalNote);

            //Assert
            AssertModificationFailure(result, OperationFailure.Forbidden);
        }

        private Mock<IDatabaseTransaction> ExpectTransaction()
        {
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManagerMock.Setup(x => x.Begin(IsolationLevel.ReadCommitted)).Returns(transaction.Object);
            return transaction;
        }

        private void VerifyExpectedDbSideEffect(bool expectSideEffect, DataProcessingRegistration dataProcessingRegistration, Mock<IDatabaseTransaction> transaction)
        {
            _repositoryMock.Verify(x => x.Update(dataProcessingRegistration), expectSideEffect ? Times.Once() : Times.Never());
            transaction.Verify(x => x.Commit(), expectSideEffect ? Times.Once() : Times.Never());
        }

        private void ExpectOrganizationalReadAccess(int organizationId, OrganizationDataReadAccessLevel organizationDataReadAccessLevel)
        {
            _authorizationContextMock.Setup(x => x.GetOrganizationReadAccessLevel(organizationId)).Returns(organizationDataReadAccessLevel);
        }

        private void AssertModificationFailure(Result<DataProcessingRegistration, OperationError> result, OperationFailure operationFailure)
        {
            _repositoryMock.Verify(x => x.Update(It.IsAny<DataProcessingRegistration>()), Times.Never);
            Assert.True(result.Failed);
            Assert.Equal(operationFailure, result.Error.FailureType);
        }

        private void ExpectAllowModifyReturns(DataProcessingRegistration dataProcessingRegistration, bool value)
        {
            _authorizationContextMock.Setup(x => x.AllowModify(dataProcessingRegistration)).Returns(value);
        }

        private void ExpectAllowReadReturns(DataProcessingRegistration dataProcessingRegistration, bool value)
        {
            _authorizationContextMock.Setup(x => x.AllowReads(dataProcessingRegistration)).Returns(value);
        }

        private void AssertFailureToDelete(int id, Result<DataProcessingRegistration, OperationError> result, OperationFailure operationFailure)
        {
            _repositoryMock.Verify(x => x.DeleteById(id), Times.Never);
            Assert.True(result.Failed);
            Assert.Equal(operationFailure, result.Error.FailureType);
        }

        private void ExpectAllowDeleteReturns(DataProcessingRegistration dataProcessingRegistration, bool value)
        {
            _authorizationContextMock.Setup(x => x.AllowDelete(dataProcessingRegistration)).Returns(value);
        }

        private void ExpectRepositoryGetToReturn(int id, Maybe<DataProcessingRegistration> registration)
        {
            _repositoryMock.Setup(x => x.GetById(id)).Returns(registration);
        }

        private static void AssertFailureToCreate(Result<DataProcessingRegistration, OperationError> result, OperationFailure operationFailure)
        {
            Assert.True(result.Failed);
            Assert.Equal(operationFailure, result.Error.FailureType);
        }

        private void ExpectSearchReturns(int organizationId, Maybe<string> name, IEnumerable<DataProcessingRegistration> registrations)
        {
            _repositoryMock.Setup(x => x.Search(organizationId, name)).Returns(registrations.AsQueryable());
        }

        private void ExpectAllowCreateReturns(int organizationId, bool value)
        {
            _authorizationContextMock.Setup(x => x.AllowCreate<DataProcessingRegistration>(organizationId)).Returns(value);
        }
    }
}
