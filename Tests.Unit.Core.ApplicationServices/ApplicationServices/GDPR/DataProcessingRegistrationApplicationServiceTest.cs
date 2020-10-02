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
        private readonly Mock<IDataProcessingRegistrationDataResponsibleAssignmentService> _dataResponsibleAssignmentServiceMock;
        private readonly Mock<IReferenceRepository> _referenceRepositoryMock;
        private readonly Mock<ITransactionManager> _transactionManagerMock;
        private readonly Mock<IGenericRepository<DataProcessingRegistrationRight>> _rightsRepositoryMock;
        private readonly Mock<IDataProcessingRegistrationSystemAssignmentService> _systemAssignmentServiceMock;
        private readonly Mock<IDataProcessingRegistrationDataProcessorAssignmentService> _dpAssignmentService;
        private readonly Mock<IDataProcessingRegistrationInsecureCountriesAssignmentService> _insecureThirdCountryAssignmentMock;
        private readonly Mock<IDataProcessingRegistrationBasisForTransferAssignmentService> _basisForTransferAssignmentServiceMock;
        private readonly Mock<IDataProcessingRegistrationOversightOptionsAssignmentService> _oversightOptionAssignmentServiceMock;

        public DataProcessingRegistrationApplicationServiceTest()
        {
            _authorizationContextMock = new Mock<IAuthorizationContext>();
            _repositoryMock = new Mock<IDataProcessingRegistrationRepository>();
            _namingServiceMock = new Mock<IDataProcessingRegistrationNamingService>();
            _roleAssignmentServiceMock = new Mock<IDataProcessingRegistrationRoleAssignmentsService>();
            _dataResponsibleAssignmentServiceMock = new Mock<IDataProcessingRegistrationDataResponsibleAssignmentService>();
            _referenceRepositoryMock = new Mock<IReferenceRepository>();
            _transactionManagerMock = new Mock<ITransactionManager>();
            _rightsRepositoryMock = new Mock<IGenericRepository<DataProcessingRegistrationRight>>();
            _systemAssignmentServiceMock = new Mock<IDataProcessingRegistrationSystemAssignmentService>();
            _dpAssignmentService = new Mock<IDataProcessingRegistrationDataProcessorAssignmentService>();
            _insecureThirdCountryAssignmentMock = new Mock<IDataProcessingRegistrationInsecureCountriesAssignmentService>();
            _basisForTransferAssignmentServiceMock = new Mock<IDataProcessingRegistrationBasisForTransferAssignmentService>();
            _oversightOptionAssignmentServiceMock = new Mock<IDataProcessingRegistrationOversightOptionsAssignmentService>();
            _sut = new DataProcessingRegistrationApplicationService(
                _authorizationContextMock.Object,
                _repositoryMock.Object,
                _namingServiceMock.Object,
                _roleAssignmentServiceMock.Object,
                _referenceRepositoryMock.Object,
                _dataResponsibleAssignmentServiceMock.Object,
                _systemAssignmentServiceMock.Object,
                _dpAssignmentService.Object,
                _insecureThirdCountryAssignmentMock.Object,
                _basisForTransferAssignmentServiceMock.Object,
                _oversightOptionAssignmentServiceMock.Object,
                _transactionManagerMock.Object,
                _rightsRepositoryMock.Object);
        }

        [Fact]
        public void Can_Create()
        {
            //Arrange
            var organizationId = A<int>();
            var name = A<string>();

            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManagerMock.Setup(x => x.Begin(IsolationLevel.Serializable)).Returns(transaction.Object);
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
            transaction.Verify(x => x.Commit());
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
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManagerMock.Setup(x => x.Begin(IsolationLevel.Serializable)).Returns(transaction.Object);

            //Act
            var result = _sut.Delete(id);

            //Assert
            _repositoryMock.Verify(x => x.DeleteById(id), Times.Once);
            Assert.True(result.Ok);
            transaction.Verify(x => x.Commit());
        }

        [Fact]
        public void Delete_Returns_NotFound()
        {
            Test_Command_Which_Fails_With_Dpr_NotFound(id => _sut.Delete(id));
        }

        [Fact]
        public void Delete_Returns_Forbidden()
        {
            Test_Command_Which_Fails_With_Dpr_Insufficient_WriteAccess(id => _sut.Delete(id));
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
            Test_Command_Which_Fails_With_Dpr_NotFound(id => _sut.Get(id));
        }

        [Fact]
        public void Get_Returns_Forbidden()
        {
            Test_Command_Which_Fails_With_Dpr_Insufficient_ReadAccess(id => _sut.Get(id));
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
            Test_Command_Which_Fails_With_Dpr_Insufficient_WriteAccess(id => _sut.UpdateName(id, A<string>()));
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
            Test_Command_Which_Fails_With_Dpr_Insufficient_ReadAccess(id => _sut.GetAvailableRoles(id));
        }

        [Fact]
        public void Cannot_GetAvailableRoles_If_Dpr_Is_Not_Found()
        {
            Test_Command_Which_Fails_With_Dpr_NotFound(id => _sut.GetAvailableRoles(id));
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
            Test_Command_Which_Fails_With_Dpr_Insufficient_ReadAccess(id => _sut.GetUsersWhichCanBeAssignedToRole(id, A<int>(), A<string>(), 13));
        }

        [Fact]
        public void Cannot_GetUsersWhichCanBeAssignedToRole_If_Dpr_Is_Not_found()
        {
            Test_Command_Which_Fails_With_Dpr_NotFound(id => _sut.GetUsersWhichCanBeAssignedToRole(id, A<int>(), A<string>(), 13));
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
            Test_Command_Which_Fails_With_Dpr_Insufficient_WriteAccess(id => _sut.AssignRole(id, A<int>(), A<int>()));
        }

        [Fact]
        public void Cannot_AssignRole_If_Dpr_Is_Not_found()
        {
            Test_Command_Which_Fails_With_Dpr_NotFound(id => _sut.AssignRole(id, A<int>(), A<int>()));
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
            Test_Command_Which_Fails_With_Dpr_Insufficient_WriteAccess(id => _sut.RemoveRole(id, A<int>(), A<int>()));
        }

        [Fact]
        public void Cannot_RemoveRole_If_Dpr_Is_Not_found()
        {
            Test_Command_Which_Fails_With_Dpr_NotFound(id => _sut.RemoveRole(id, A<int>(), A<int>()));
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
            Test_Command_Which_Fails_With_Dpr_Insufficient_WriteAccess(id => _sut.SetMasterReference(id, A<int>()));
        }

        [Fact]
        public void Cannot_Set_Master_Reference_On_Dpa_With_Invalid_Dpa()
        {
            Test_Command_Which_Fails_With_Dpr_NotFound(id => _sut.SetMasterReference(id, A<int>()));
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
        public void Cannot_GetSystemsWhichCanBeAssigned_If_Dpr_Is_Not_Found()
        {
            Test_Command_Which_Fails_With_Dpr_NotFound(id => _sut.GetSystemsWhichCanBeAssigned(id, A<string>(), new Random().Next(2, 100)));
        }

        [Fact]
        public void Cannot_GetSystemsWhichCanBeAssigned_If_Read_Access_Is_Denied()
        {
            Test_Command_Which_Fails_With_Dpr_Insufficient_ReadAccess(id => _sut.GetSystemsWhichCanBeAssigned(id, A<string>(), new Random().Next(2, 100)));
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
        public void Cannot_AssignSystem_If_Dpr_Is_Not_Found()
        {
            Test_Command_Which_Fails_With_Dpr_NotFound(id => _sut.AssignSystem(id, A<int>()));
        }

        [Fact]
        public void Cannot_AssignSystem_If_Write_Access_Is_Denied()
        {
            Test_Command_Which_Fails_With_Dpr_Insufficient_WriteAccess(id => _sut.AssignSystem(id, A<int>()));
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
        public void Cannot_RemoveSystem_If_Dpr_Is_Not_Found()
        {
            Test_Command_Which_Fails_With_Dpr_NotFound(id => _sut.RemoveSystem(id, A<int>()));
        }

        [Fact]
        public void Cannot_RemoveSystem_If_Write_Access_Is_Denied()
        {
            Test_Command_Which_Fails_With_Dpr_Insufficient_WriteAccess(id => _sut.RemoveSystem(id, A<int>()));
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
        public void Cannot_AssignDataProcessorIf_Dpr_Is_Not_Found()
        {
            Test_Command_Which_Fails_With_Dpr_NotFound(id => _sut.AssignDataProcessor(id, A<int>()));
        }

        [Fact]
        public void Cannot_AssignDataProcessor_If_Write_Access_Is_Denied()
        {
            Test_Command_Which_Fails_With_Dpr_Insufficient_WriteAccess(id => _sut.AssignDataProcessor(id, A<int>()));
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
        public void Cannot_RemoveDataProcessor_If_Dpr_Is_Not_Found()
        {
            Test_Command_Which_Fails_With_Dpr_NotFound(id => _sut.RemoveDataProcessor(id, A<int>()));
        }

        [Fact]
        public void Cannot_RemoveDataProcessor_If_Write_Access_Is_Denied()
        {
            Test_Command_Which_Fails_With_Dpr_Insufficient_WriteAccess(id => _sut.RemoveDataProcessor(id, A<int>()));
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
        public void Can_AssignSubDataProcessor()
        {
            //Arrange
            var id = A<int>();
            var registration = new DataProcessingRegistration();
            var organizationId = A<int>();
            var organization = new Organization();
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowModifyReturns(registration, true);
            _dpAssignmentService.Setup(x => x.AssignSubDataProcessor(registration, organizationId))
                .Returns(Result<Organization, OperationError>.Success(organization));

            var transaction = ExpectTransaction();

            //Act
            var result = _sut.AssignSubDataProcessor(id, organizationId);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(organization, result.Value);
            transaction.Verify(x => x.Commit());
        }

        [Fact]
        public void Cannot_AssignSubDataProcessorIf_Dpr_Is_Not_Found()
        {
            Test_Command_Which_Fails_With_Dpr_NotFound(id => _sut.AssignSubDataProcessor(id, A<int>()));
        }

        [Fact]
        public void Cannot_AssignSubDataProcessor_If_Write_Access_Is_Denied()
        {
            Test_Command_Which_Fails_With_Dpr_Insufficient_WriteAccess(id => _sut.AssignSubDataProcessor(id, A<int>()));
        }

        [Fact]
        public void Can_RemoveSubDataProcessor()
        {
            //Arrange
            var id = A<int>();
            var registration = new DataProcessingRegistration();
            var organizationId = A<int>();
            var organization = new Organization();
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowModifyReturns(registration, true);
            _dpAssignmentService.Setup(x => x.RemoveSubDataProcessor(registration, organizationId))
                .Returns(Result<Organization, OperationError>.Success(organization));

            var transaction = ExpectTransaction();

            //Act
            var result = _sut.RemoveSubDataProcessor(id, organizationId);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(organization, result.Value);
            transaction.Verify(x => x.Commit());
        }

        [Fact]
        public void Cannot_RemoveSubDataProcessor_If_Dpr_Is_Not_Found()
        {
            Test_Command_Which_Fails_With_Dpr_NotFound(id => _sut.RemoveSubDataProcessor(id, A<int>()));
        }

        [Fact]
        public void Cannot_RemoveSubDataProcessor_If_Write_Access_Is_Denied()
        {
            Test_Command_Which_Fails_With_Dpr_Insufficient_WriteAccess(id => _sut.RemoveSubDataProcessor(id, A<int>()));
        }

        [Fact]
        public void Can_GetSubDataProcessorsWhichCanBeAssigned()
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
            _dpAssignmentService.Setup(x => x.GetApplicableSubDataProcessors(registration)).Returns(organizations.AsQueryable());

            //Act
            var result = _sut.GetSubDataProcessorsWhichCanBeAssigned(id, nameQuery, new Random().Next(2, 100));

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(organizations, result.Value);
        }

        [Fact]
        public void Can_SetSubDataProcessorsState()
        {
            //Arrange
            var id = A<int>();
            var registration = new DataProcessingRegistration();
            var newValue = A<YesNoUndecidedOption>();
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowModifyReturns(registration, true);

            var transaction = ExpectTransaction();

            //Act
            var result = _sut.SetSubDataProcessorsState(id, newValue);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(registration.HasSubDataProcessors, newValue);
            transaction.Verify(x => x.Commit());
        }

        [Theory]
        [InlineData(YesNoUndecidedOption.No)]
        [InlineData(YesNoUndecidedOption.Undecided)]
        public void Can_SetSubDataProcessorsState_Clears_SubdataProcessors_On_Negating_Setting(YesNoUndecidedOption clearingSetting)
        {
            //Arrange
            var id = A<int>();
            var registration = new DataProcessingRegistration(){SubDataProcessors = {new Organization()}};
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowModifyReturns(registration, true);

            var transaction = ExpectTransaction();

            //Act
            var result = _sut.SetSubDataProcessorsState(id, clearingSetting);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(registration.HasSubDataProcessors, clearingSetting);
            Assert.Empty(registration.SubDataProcessors);
            transaction.Verify(x => x.Commit());
        }

        [Fact]
        public void Cannot_SetSubDataProcessorsState_If_Dpr_Is_Not_Found()
        {
            Test_Command_Which_Fails_With_Dpr_NotFound(id => _sut.SetSubDataProcessorsState(id, A<YesNoUndecidedOption>()));
        }

        [Fact]
        public void Cannot_SetSubDataProcessorsState_If_Write_Access_Is_Denied()
        {
            //Arrange
            var id = A<int>();
            var registration = new DataProcessingRegistration();
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowModifyReturns(registration, false);

            //Act
            var result = _sut.SetSubDataProcessorsState(id, A<YesNoUndecidedOption>());

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Can_Update_OversightInterval()
        {
            //Arrange
            var id = A<int>();
            var oversightInterval = A<YearMonthIntervalOption>();
            var registration = new DataProcessingRegistration();
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowModifyReturns(registration, true);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManagerMock.Setup(x => x.Begin(IsolationLevel.ReadCommitted)).Returns(transaction.Object);

            //Act
            var result = _sut.UpdateOversightInterval(id, oversightInterval);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(oversightInterval, result.Value.OversightInterval);
            transaction.Verify(x => x.Commit());
            _repositoryMock.Verify(x => x.Update(registration), Times.Once);
        }

        [Fact]
        public void Update_OversightInterval_Returns_Forbidden()
        {
            Test_Command_Which_Fails_With_Dpr_Insufficient_WriteAccess(id => _sut.UpdateOversightInterval(id, A<YearMonthIntervalOption>()));
        }

        [Fact]
        public void Can_Update_OversightIntervalRemark()
        {
            //Arrange
            var id = A<int>();
            var oversightIntervalRemark = A<string>();
            var registration = new DataProcessingRegistration();
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowModifyReturns(registration, true);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManagerMock.Setup(x => x.Begin(IsolationLevel.ReadCommitted)).Returns(transaction.Object);

            //Act
            var result = _sut.UpdateOversightIntervalRemark(id, oversightIntervalRemark);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(oversightIntervalRemark, result.Value.OversightIntervalRemark);
            transaction.Verify(x => x.Commit());
            _repositoryMock.Verify(x => x.Update(registration), Times.Once);
        }

        [Fact]
        public void Update_OversightIntervalRemark_Returns_Forbidden()
        {
            Test_Command_Which_Fails_With_Dpr_Insufficient_WriteAccess(id => _sut.UpdateOversightIntervalRemark(id, A<string>()));
        }

        private Mock<IDatabaseTransaction> ExpectTransaction()
        {
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManagerMock.Setup(x => x.Begin(IsolationLevel.ReadCommitted)).Returns(transaction.Object);
            return transaction;
        }

        [Fact]
        public void Can_Update_IsAgreementConcluded()
        {
            //Arrange
            var id = A<int>();
            var isAgreementConcluded = A<YesNoIrrelevantOption>();
            var registration = new DataProcessingRegistration();
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowModifyReturns(registration, true);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManagerMock.Setup(x => x.Begin(IsolationLevel.ReadCommitted)).Returns(transaction.Object);

            //Act
            var result = _sut.UpdateIsAgreementConcluded(id, isAgreementConcluded);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(isAgreementConcluded, result.Value.IsAgreementConcluded);
            transaction.Verify(x => x.Commit());
            _repositoryMock.Verify(x => x.Update(registration), Times.Once);
        }

        [Theory]
        [InlineData(YesNoIrrelevantOption.IRRELEVANT)]
        [InlineData(YesNoIrrelevantOption.NO)]
        [InlineData(YesNoIrrelevantOption.UNDECIDED)]
        public void Can_Update_IsAgreementConcluded_And_Clear_Date_On_Toggle_Off(YesNoIrrelevantOption clearingSetting)
        {
            //Arrange
            var id = A<int>();
            var registration = new DataProcessingRegistration {AgreementConcludedAt = A<DateTime>()};
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowModifyReturns(registration, true);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManagerMock.Setup(x => x.Begin(IsolationLevel.ReadCommitted)).Returns(transaction.Object);

            //Act
            var result = _sut.UpdateIsAgreementConcluded(id, clearingSetting);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(clearingSetting, result.Value.IsAgreementConcluded);
            Assert.Null(registration.AgreementConcludedAt);
            transaction.Verify(x => x.Commit());
            _repositoryMock.Verify(x => x.Update(registration), Times.Once);
        }

        [Fact]
        public void Update_IsAgreementConcluded_Returns_Forbidden()
        {
            Test_Command_Which_Fails_With_Dpr_Insufficient_WriteAccess(id => _sut.UpdateIsAgreementConcluded(id, A<YesNoIrrelevantOption>()));
        }

        [Fact]
        public void Can_Update_AgreementConcludedAt()
        {
            //Arrange
            var id = A<int>();
            var dateTime = A<DateTime>();
            var registration = new DataProcessingRegistration();
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowModifyReturns(registration, true);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManagerMock.Setup(x => x.Begin(IsolationLevel.ReadCommitted)).Returns(transaction.Object);

            //Act
            var result = _sut.UpdateAgreementConcludedAt(id, dateTime);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(dateTime, result.Value.AgreementConcludedAt);
            transaction.Verify(x => x.Commit());
            _repositoryMock.Verify(x => x.Update(registration), Times.Once);
        }

        [Fact]
        public void Can_Update_AgreementConcludedAt_To_Null()
        {
            //Arrange
            var id = A<int>();
            var registration = new DataProcessingRegistration();
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowModifyReturns(registration, true);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManagerMock.Setup(x => x.Begin(IsolationLevel.ReadCommitted)).Returns(transaction.Object);

            //Act
            var result = _sut.UpdateAgreementConcludedAt(id, null);

            //Assert
            Assert.True(result.Ok);
            Assert.Null(result.Value.AgreementConcludedAt);
            transaction.Verify(x => x.Commit());
            _repositoryMock.Verify(x => x.Update(registration), Times.Once);
        }

        [Fact]
        public void Update_AgreementConcludedAt_Returns_Forbidden()
        {
            Test_Command_Which_Fails_With_Dpr_Insufficient_WriteAccess(id => _sut.UpdateAgreementConcludedAt(id, A<DateTime>()));
        }

        [Fact]
        public void Can_UpdateTransferToInsecureThirdCountries()
        {
            //Arrange
            var id = A<int>();
            var registration = new DataProcessingRegistration();
            var newValue = A<YesNoUndecidedOption>();
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowModifyReturns(registration, true);

            var transaction = ExpectTransaction();

            //Act
            var result = _sut.UpdateTransferToInsecureThirdCountries(id, newValue);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(registration.TransferToInsecureThirdCountries, newValue);
            transaction.Verify(x => x.Commit());
        }

        [Theory]
        [InlineData(YesNoUndecidedOption.Undecided)]
        [InlineData(YesNoUndecidedOption.No)]
        public void Can_UpdateTransferToInsecureThirdCountries_Clears_ThirdCountries_On_Negating_Option(YesNoUndecidedOption clearingSetting)
        {
            //Arrange
            var id = A<int>();
            var registration = new DataProcessingRegistration(){InsecureCountriesSubjectToDataTransfer = {new DataProcessingCountryOption()}};
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowModifyReturns(registration, true);

            var transaction = ExpectTransaction();

            //Act
            var result = _sut.UpdateTransferToInsecureThirdCountries(id, clearingSetting);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(registration.TransferToInsecureThirdCountries, clearingSetting);
            Assert.Empty(registration.InsecureCountriesSubjectToDataTransfer);
            transaction.Verify(x => x.Commit());
        }

        [Fact]
        public void Cannot_UpdateTransferToInsecureThirdCountries_If_Dpr_Is_Not_Found()
        {
            Test_Command_Which_Fails_With_Dpr_NotFound(id => _sut.UpdateTransferToInsecureThirdCountries(id, A<YesNoUndecidedOption>()));
        }

        [Fact]
        public void Cannot_UpdateTransferToInsecureThirdCountries_If_Write_Access_Is_Denied()
        {
            Test_Command_Which_Fails_With_Dpr_Insufficient_WriteAccess(id => _sut.UpdateTransferToInsecureThirdCountries(id, A<YesNoUndecidedOption>()));
        }

        [Fact]
        public void Can_AssignInsecureThirdCountry()
        {
            //Arrange
            var id = A<int>();
            var registration = new DataProcessingRegistration();
            var countryId = A<int>();
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowModifyReturns(registration, true);

            var transaction = ExpectTransaction();
            _insecureThirdCountryAssignmentMock.Setup(x => x.Assign(registration, countryId)).Returns(Result<DataProcessingCountryOption, OperationError>.Success(new DataProcessingCountryOption()));

            //Act
            var result = _sut.AssignInsecureThirdCountry(id, countryId);

            //Assert
            Assert.True(result.Ok);

            transaction.Verify(x => x.Commit());
        }

        [Fact]
        public void Cannot_AssignInsecureThirdCountry_If_Dpr_Is_Not_Found()
        {
            Test_Command_Which_Fails_With_Dpr_NotFound(id => _sut.AssignInsecureThirdCountry(id, A<int>()));
        }

        [Fact]
        public void Cannot_AssignInsecureThirdCountry_If_Write_Access_Is_Denied()
        {
            Test_Command_Which_Fails_With_Dpr_Insufficient_WriteAccess(id => _sut.AssignInsecureThirdCountry(id, A<int>()));
        }

        [Fact]
        public void Can_RemoveInsecureThirdCountry()
        {
            //Arrange
            var id = A<int>();
            var registration = new DataProcessingRegistration();
            var countryId = A<int>();
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowModifyReturns(registration, true);

            var transaction = ExpectTransaction();
            _insecureThirdCountryAssignmentMock.Setup(x => x.Remove(registration, countryId)).Returns(Result<DataProcessingCountryOption, OperationError>.Success(new DataProcessingCountryOption()));

            //Act
            var result = _sut.RemoveInsecureThirdCountry(id, countryId);

            //Assert
            Assert.True(result.Ok);

            transaction.Verify(x => x.Commit());
        }

        [Fact]
        public void Cannot_RemoveInsecureThirdCountry_If_Dpr_Is_Not_Found()
        {
            Test_Command_Which_Fails_With_Dpr_NotFound(id => _sut.RemoveInsecureThirdCountry(id, A<int>()));
        }

        [Fact]
        public void Cannot_RemoveInsecureThirdCountry_If_Write_Access_Is_Denied()
        {
            Test_Command_Which_Fails_With_Dpr_Insufficient_WriteAccess(id => _sut.RemoveInsecureThirdCountry(id, A<int>()));
        }

        [Fact]
        public void Can_AssignBasisForTransfer()
        {
            Test_Command_Which_ModifiesState_With_Success(registration =>
            {
                //Arrange
                var optionId = A<int>();
                _basisForTransferAssignmentServiceMock.Setup(x => x.Assign(registration, optionId)).Returns(Result<DataProcessingBasisForTransferOption, OperationError>.Success(new DataProcessingBasisForTransferOption()));

                //Act
                return _sut.AssignBasisForTransfer(registration.Id, optionId);
            });
        }

        [Fact]
        public void Cannot_AssignBasisForTransfer_If_Dpr_Is_Not_Found()
        {
            Test_Command_Which_Fails_With_Dpr_NotFound(id => _sut.AssignBasisForTransfer(id, A<int>()));
        }

        [Fact]
        public void Cannot_AssignBasisForTransfer_If_Write_Access_Is_Denied()
        {
            Test_Command_Which_Fails_With_Dpr_Insufficient_WriteAccess(id => _sut.AssignBasisForTransfer(id, A<int>()));
        }

        [Fact]
        public void Can_ClearBasisForTransfer()
        {
            Test_Command_Which_ModifiesState_With_Success(registration =>
            {
                //Arrange
                _basisForTransferAssignmentServiceMock.Setup(x => x.Clear(registration)).Returns(Result<DataProcessingBasisForTransferOption, OperationError>.Success(new DataProcessingBasisForTransferOption()));

                //Act
                return _sut.ClearBasisForTransfer(registration.Id);
            });
        }

        [Fact]
        public void Cannot_ClearBasisForTransfer_If_Dpr_Is_Not_Found()
        {
            Test_Command_Which_Fails_With_Dpr_NotFound(id => _sut.ClearBasisForTransfer(id));
        }

        [Fact]
        public void Cannot_ClearBasisForTransfer_If_Write_Access_Is_Denied()
        {
            Test_Command_Which_Fails_With_Dpr_Insufficient_WriteAccess(id => _sut.ClearBasisForTransfer(id));
        }

        [Fact]
        public void Can_Update_DataResponsible()
        {
            Test_Command_Which_ModifiesState_With_Success(registration =>
            {
                //Arrange
                var optionId = A<int>();
                _dataResponsibleAssignmentServiceMock.Setup(x => x.Assign(registration, optionId)).Returns(Result<DataProcessingDataResponsibleOption, OperationError>.Success(new DataProcessingDataResponsibleOption()));

                //Act
                return _sut.AssignDataResponsible(registration.Id, optionId);
            });
        }

        [Fact]
        public void Can_Update_DataResponsible_To_Null()
        {
            Test_Command_Which_ModifiesState_With_Success(registration =>
            {
                //Arrange
                _dataResponsibleAssignmentServiceMock.Setup(x => x.Clear(registration)).Returns(Result<DataProcessingDataResponsibleOption, OperationError>.Success(new DataProcessingDataResponsibleOption()));

                //Act
                return _sut.ClearDataResponsible(registration.Id);
            });
        }

        [Fact]
        public void Update_DataResponsible_Returns_Forbidden()
        {
            Test_Command_Which_Fails_With_Dpr_Insufficient_WriteAccess(id => _sut.AssignDataResponsible(id, A<int>()));
        }

        [Fact]
        public void Update_DataResponsible_Returns_NotFound()
        {
            Test_Command_Which_Fails_With_Dpr_NotFound(id => _sut.AssignDataResponsible(id, A<int>()));
        }

        [Fact]
        public void Can_Update_DataResponsibleRemark()
        {
            Test_Command_Which_ModifiesState_With_Success(registration =>
            {
                //Act
                return _sut.UpdateDataResponsibleRemark(registration.Id, A<string>());
            });
        }

        [Fact]
        public void Update_DataResponsibleRemark_Returns_Forbidden()
        {
            Test_Command_Which_Fails_With_Dpr_Insufficient_WriteAccess(id => _sut.UpdateDataResponsibleRemark(id, A<string>()));
        }

        [Fact]
        public void Update_DataResponsibleRemark_Returns_NotFound()
        {
            Test_Command_Which_Fails_With_Dpr_NotFound(id => _sut.UpdateDataResponsibleRemark(id, A<string>()));
        }

        [Fact]
        public void Can_AssignOversightOption()
        {
            //Arrange
            var id = A<int>();
            var registration = new DataProcessingRegistration();
            var oversightOptionId = A<int>();
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowModifyReturns(registration, true);

            var transaction = ExpectTransaction();
            _oversightOptionAssignmentServiceMock.Setup(x => x.Assign(registration, oversightOptionId)).Returns(Result<DataProcessingOversightOption, OperationError>.Success(new DataProcessingOversightOption()));

            //Act
            var result = _sut.AssignOversightOption(id, oversightOptionId);

            //Assert
            Assert.True(result.Ok);

            transaction.Verify(x => x.Commit());
        }

        [Fact]
        public void Cannot_AssignOversightOption_If_Dpr_Is_Not_Found()
        {
            Test_Command_Which_Fails_With_Dpr_NotFound(id => _sut.AssignOversightOption(id, A<int>()));
        }

        [Fact]
        public void Cannot_AssignOversightOption_If_Write_Access_Is_Denied()
        {
            Test_Command_Which_Fails_With_Dpr_Insufficient_WriteAccess(id => _sut.AssignOversightOption(id, A<int>()));
        }

        [Fact]
        public void Can_RemoveOversightOption()
        {
            //Arrange
            var id = A<int>();
            var registration = new DataProcessingRegistration();
            var oversightOptionId = A<int>();
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowModifyReturns(registration, true);

            var transaction = ExpectTransaction();
            _oversightOptionAssignmentServiceMock.Setup(x => x.Remove(registration, oversightOptionId)).Returns(Result<DataProcessingOversightOption, OperationError>.Success(new DataProcessingOversightOption()));

            //Act
            var result = _sut.RemoveOversightOption(id, oversightOptionId);

            //Assert
            Assert.True(result.Ok);

            transaction.Verify(x => x.Commit());
        }

        [Fact]
        public void Cannot_RemoveOversightOption_If_Dpr_Is_Not_Found()
        {
            Test_Command_Which_Fails_With_Dpr_NotFound(id => _sut.RemoveOversightOption(id, A<int>()));
        }

        [Fact]
        public void Cannot_RemoveOversightOption_If_Write_Access_Is_Denied()
        {
            Test_Command_Which_Fails_With_Dpr_Insufficient_WriteAccess(id => _sut.RemoveOversightOption(id, A<int>()));
        }

        [Fact]
        public void Can_Update_OversightOptionRemark()
        {
            Test_Command_Which_ModifiesState_With_Success(registration =>
            {
                //Act
                return _sut.UpdateOversightOptionRemark(registration.Id, A<string>());
            });
        }

        [Fact]
        public void Update_OversightOptionRemark_Returns_Forbidden()
        {
            Test_Command_Which_Fails_With_Dpr_Insufficient_WriteAccess(id => _sut.UpdateOversightOptionRemark(id, A<string>()));
        }

        [Fact]
        public void Update_OversightOptionRemark_Returns_NotFound()
        {
            Test_Command_Which_Fails_With_Dpr_NotFound(id => _sut.UpdateOversightOptionRemark(id, A<string>()));
        }

        /// <summary>
        /// Helper test to make it easy to cover the "Modify succeeds" case
        /// </summary>
        /// <param name="command"></param>
        /// <param name="assertAdditionalConditions">Additional assertions besides Result=OK AND transaction committed</param>
        private void Test_Command_Which_ModifiesState_With_Success<TSuccess>(
            Func<DataProcessingRegistration, Result<TSuccess, OperationError>> command,
            Action<DataProcessingRegistration, TSuccess> assertAdditionalConditions = null)
        {
            //Arrange
            var id = A<int>();
            var registration = new DataProcessingRegistration { Id = id };
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowModifyReturns(registration, true);
            var transaction = ExpectTransaction();
            //Act
            var result = command(registration);

            //Assert
            Assert.True(result.Ok);
            _repositoryMock.Verify(x => x.Update(registration), Times.Once());
            transaction.Verify(x => x.Commit(), Times.Once());
        }

        /// <summary>
        /// Helper test to make it easy to cover the "Missing Write access" case
        /// </summary>
        /// <param name="command"></param>
        private void Test_Command_Which_Fails_With_Dpr_Insufficient_WriteAccess<TSuccess>(Func<int, Result<TSuccess, OperationError>> command)
        {
            //Arrange
            var id = A<int>();
            var registration = new DataProcessingRegistration();
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowModifyReturns(registration, false);

            //Act
            var result = command(id);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        /// <summary>
        /// Helper test to make it easy to cover the "Missing read access" case
        /// </summary>
        /// <param name="command"></param>
        private void Test_Command_Which_Fails_With_Dpr_Insufficient_ReadAccess<TSuccess>(Func<int, Result<TSuccess, OperationError>> command)
        {
            //Arrange
            var id = A<int>();
            var registration = new DataProcessingRegistration();
            ExpectRepositoryGetToReturn(id, registration);
            ExpectAllowReadReturns(registration, false);

            //Act
            var result = command(id);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }


        /// <summary>
        /// Helper test to make it easy to cover the "DRP not found" case
        /// </summary>
        /// <param name="command"></param>
        private void Test_Command_Which_Fails_With_Dpr_NotFound<TSuccess>(Func<int, Result<TSuccess, OperationError>> command)
        {
            //Arrange
            var id = A<int>();
            ExpectRepositoryGetToReturn(id, Maybe<DataProcessingRegistration>.None);

            //Act
            var result = command(id);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
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

        private void ExpectAllowModifyReturns(DataProcessingRegistration dataProcessingRegistration, bool value)
        {
            _authorizationContextMock.Setup(x => x.AllowModify(dataProcessingRegistration)).Returns(value);
        }

        private void ExpectAllowReadReturns(DataProcessingRegistration dataProcessingRegistration, bool value)
        {
            _authorizationContextMock.Setup(x => x.AllowReads(dataProcessingRegistration)).Returns(value);
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
