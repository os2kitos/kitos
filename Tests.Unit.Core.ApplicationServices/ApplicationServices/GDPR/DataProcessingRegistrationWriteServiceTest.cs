using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.GDPR.Write;
using Core.ApplicationServices.Generic.Write;
using Core.ApplicationServices.Model;
using Core.ApplicationServices.Model.GDPR.Write;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.Shared.Write;
using Core.ApplicationServices.References;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainModel.References;
using Core.DomainModel.Shared;
using Core.DomainServices.GDPR;
using Core.DomainServices;
using Core.DomainServices.Generic;
using Core.DomainServices.Repositories.GDPR;
using Core.DomainServices.Role;
using Infrastructure.Services.DataAccess;
using Moq;
using Serilog;
using Tests.Toolkit.Extensions;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.GDPR
{
    public class DataProcessingRegistrationWriteServiceTest : WithAutoFixture
    {
        private readonly DataProcessingRegistrationWriteService _sut;
        private readonly Mock<IEntityIdentityResolver> _identityResolverMock;
        private readonly Mock<IReferenceService> _referenceServiceMock;
        private readonly Mock<IDomainEvents> _domainEventsMock;
        private readonly Mock<ITransactionManager> _transactionManagerMock;
        private readonly Mock<IDatabaseControl> _databaseControlMock;
        private readonly Mock<IAssignmentUpdateService> _assignmentUpdateServiceMock;
        private readonly Mock<IAuthorizationContext> _authorizationContextMock;
        private readonly Mock<IAuthorizationModel> _authorizationModelMock;
        private readonly Mock<IDataProcessingRegistrationNamingService> _namingServiceMock;
        private readonly Mock<IDataProcessingRegistrationRepository> _repositoryMock;
        private readonly Mock<IGenericRepository<DataProcessingRegistrationOversightDate>> _oversightDateRepositoryMock;

        private readonly Mock<IRoleAssignmentService<DataProcessingRegistrationRight, DataProcessingRegistrationRole,
                DataProcessingRegistration>> _roleAssignmentsServiceMock;

        private readonly Mock<IGenericRepository<SubDataProcessor>> _sdpRepositoryMock;

        private readonly Mock<IDataProcessingRegistrationBasisForTransferAssignmentService>
            _basisForTransferAssignmentServiceMock;

        private readonly Mock<IDataProcessingRegistrationDataResponsibleAssignmentService>
            _dataResponsibleAssignmentServiceMock;

        public DataProcessingRegistrationWriteServiceTest()
        {
            _identityResolverMock = new Mock<IEntityIdentityResolver>();
            _referenceServiceMock = new Mock<IReferenceService>();
            var loggerMock = new Mock<ILogger>();
            _domainEventsMock = new Mock<IDomainEvents>();
            _transactionManagerMock = new Mock<ITransactionManager>();
            _databaseControlMock = new Mock<IDatabaseControl>();
            _assignmentUpdateServiceMock = new Mock<IAssignmentUpdateService>();
            _authorizationContextMock = new Mock<IAuthorizationContext>();
            _authorizationModelMock = new Mock<IAuthorizationModel>();
            _namingServiceMock = new Mock<IDataProcessingRegistrationNamingService>();
            _repositoryMock = new Mock<IDataProcessingRegistrationRepository>();
            _oversightDateRepositoryMock = new Mock<IGenericRepository<DataProcessingRegistrationOversightDate>>();
            _roleAssignmentsServiceMock =
                new Mock<IRoleAssignmentService<DataProcessingRegistrationRight, DataProcessingRegistrationRole,
                    DataProcessingRegistration>>();
            var dataProcessingRegistrationOversightDateRepositoryMock = new Mock<IGenericRepository<DataProcessingRegistrationOversightDate>>();
            var oversightOptionAssignmentServiceMock = new Mock<IDataProcessingRegistrationOversightOptionsAssignmentService>();
            _sdpRepositoryMock = new Mock<IGenericRepository<SubDataProcessor>>();
            var systemAssignmentServiceMock = new Mock<IDataProcessingRegistrationSystemAssignmentService>();
            var dataProcessingRegistrationDataProcessorAssignmentServiceMock = new Mock<IDataProcessingRegistrationDataProcessorAssignmentService>();
            var countryAssignmentServiceMock = new Mock<IDataProcessingRegistrationInsecureCountriesAssignmentService>();
            _basisForTransferAssignmentServiceMock =
                new Mock<IDataProcessingRegistrationBasisForTransferAssignmentService>();
            _dataResponsibleAssignmentServiceMock =
                new Mock<IDataProcessingRegistrationDataResponsibleAssignmentService>();

            // Setup authorization behavior
            _authorizationContextMock
                .Setup(x => x.GetAuthorizationModel(It.IsAny<IEntityOwnedByOrganization>()))
                .Returns(_authorizationModelMock.Object);

            _authorizationModelMock
                .Setup(x => x.AuthorizeUpdate(It.IsAny<IEntityOwnedByOrganization>(),
                    It.IsAny<ISupplierAssociatedEntityUpdateParameters>()))
                .Returns((Result<bool, OperationError>)true);

            _authorizationModelMock
                .Setup(x => x.AuthorizeChildEntityDelete(It.IsAny<IEntityOwnedByOrganization>(),
                    It.IsAny<object>()))
                .Returns(true);

            // Initialize the system under test (SUT)
            _sut = new DataProcessingRegistrationWriteService(
                _identityResolverMock.Object,
                _referenceServiceMock.Object,
                loggerMock.Object,
                _domainEventsMock.Object,
                _transactionManagerMock.Object,
                _databaseControlMock.Object,
                _assignmentUpdateServiceMock.Object,
                _authorizationContextMock.Object,
                _namingServiceMock.Object,
                _oversightDateRepositoryMock.Object,
                _roleAssignmentsServiceMock.Object,
                dataProcessingRegistrationOversightDateRepositoryMock.Object,
                oversightOptionAssignmentServiceMock.Object,
                _sdpRepositoryMock.Object,
                systemAssignmentServiceMock.Object,
                dataProcessingRegistrationDataProcessorAssignmentServiceMock.Object,
                countryAssignmentServiceMock.Object,
                _basisForTransferAssignmentServiceMock.Object,
                _dataResponsibleAssignmentServiceMock.Object, 
                _repositoryMock.Object
            );
        }

        protected override void OnFixtureCreated(Fixture fixture)
        {
            base.OnFixtureCreated(fixture);
            var outerFixture = new Fixture();

            //Ensure operation errors are always auto-created WITH both failure and message
            fixture.Register(() =>
                new OperationError(outerFixture.Create<string>(), outerFixture.Create<OperationFailure>()));
        }

        [Fact]
        public void Can_Create_With_Name_Only()
        {
            //Arrange
            var (organizationUuid, parameters, createdRegistration, _) = SetupCreateScenarioPrerequisites();
            
            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(createdRegistration, result.Value);
            AssertDBControlSaveChanges();
        }

        private void AssertDBControlSaveChanges()
        {
            _databaseControlMock.Verify(_ => _.SaveChanges());
        }


        [Fact]
        public void Cannot_Create_If_Dpr_Creation_Forbidden()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var parameters = new DataProcessingRegistrationCreationParameters
            {
                Name = A<string>().AsChangedValue()
            };
            var transaction = ExpectTransaction();
            var orgDbId = A<int>();

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<Organization>(organizationUuid, orgDbId);
            AllowCreateReturns(false);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownError(result, new OperationError(OperationFailure.Forbidden), transaction);
        }

        [Fact]
        public void Cannot_Create_If_Dpr_Name_Invalid()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var parameters = new DataProcessingRegistrationCreationParameters
            {
                Name = A<string>().AsChangedValue()
            };
            var transaction = ExpectTransaction();
            var orgDbId = A<int>();

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<Organization>(organizationUuid, orgDbId);
            AllowCreateReturns();
            _namingServiceMock.Setup(x => x.ValidateSuggestedNewRegistrationName(It.IsAny<int>(), parameters.Name.NewValue))
                .Returns(Maybe<OperationError>.Some(A<OperationError>()));

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithUnknownError(result, new OperationError(OperationFailure.Forbidden), transaction);
        }
        

        [Fact]
        public void Cannot_Create_If_Name_Is_Not_Provided()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var parameters = new DataProcessingRegistrationCreationParameters
            {
                Name = OptionalValueChange<string>.None
            };
            var transaction = ExpectTransaction();
            var orgDbId = A<int>();

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<Organization>(organizationUuid, orgDbId);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(result, "Name must be provided", OperationFailure.BadInput, transaction);
        }

        

        [Fact]
        public void Cannot_Create_If_Organization_Id_Resolution_Fails()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var parameters = new DataProcessingRegistrationCreationParameters
            {
                Name = A<string>().AsChangedValue()
            };
            var transaction = ExpectTransaction();
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<Organization>(organizationUuid, Maybe<int>.None);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(result, "Unable to resolve Organization with uuid", OperationFailure.BadInput, transaction);
        }
        

        [Fact]
        public void Can_Update_Name()
        {
            //Arrange
            var parameters = new DataProcessingRegistrationModificationParameters
            {
                Name = A<string>().AsChangedValue()
            };
            var dprUuid = A<Guid>();
            var dprId = A<int>();
            var dataProcessingRegistration = new DataProcessingRegistration
            {
                Id = dprId,
                Uuid = dprUuid,
                Organization = new Organization() { Uuid = A<Guid>() }
            };
            var transaction = ExpectTransaction();
            SetupUpdateSuccess(dataProcessingRegistration);
            _namingServiceMock.Setup(_ => _.ChangeName(dataProcessingRegistration, It.IsAny<string>()))
                .Returns(Maybe<OperationError>.None);
            //Act
            var result = _sut.Update(dprUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(dataProcessingRegistration, result.Value);
            AssertTransactionCommitted(transaction);
            _domainEventsMock.Verify(x => x.Raise(It.IsAny<EntityUpdatedEventWithSnapshot<DataProcessingRegistration, DprSnapshot>>()), Times.Once);
        }

     

        [Fact]
        public void Update_Name_Does_Not_Change_Anything_If_No_NameChange_Is_Defined()
        {
            //Arrange
            var parameters = new DataProcessingRegistrationModificationParameters
            {
                Name = OptionalValueChange<string>.None
            };
            var dprUuid = A<Guid>();
            var dataProcessingRegistration = new DataProcessingRegistration
            {
                Id = A<int>(),
                Uuid = dprUuid,
                Organization = new Organization() {Uuid = A<Guid>() }
            };
            var transaction = ExpectTransaction();
            SetupUpdateSuccess(dataProcessingRegistration);

            //Act
            var result = _sut.Update(dprUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(dataProcessingRegistration, result.Value);
            AssertTransactionCommitted(transaction);
        }


        [Fact]
        public void Cannot_Update_Name_If_NamingServiceUpdate_Fails()
        {
            //Arrange
            var parameters = new DataProcessingRegistrationModificationParameters
            {
                Name = A<string>().AsChangedValue()
            };
            var dprUuid = A<Guid>();
            var dataProcessingRegistration = new DataProcessingRegistration
            {
                Id = A<int>(),
                Uuid = dprUuid,
                Organization = new Organization() { Uuid = A<Guid>() }
            };
            var transaction = ExpectTransaction();
            var operationError = A<OperationError>();
            SetupUpdateSuccess(dataProcessingRegistration);
            _namingServiceMock.Setup(_ => _.ChangeName(dataProcessingRegistration, parameters.Name.NewValue))
                .Returns(Maybe<OperationError>.Some(operationError));

            //Act
            var result = _sut.Update(dprUuid, parameters);

            //Assert
            AssertFailureWithKnownError(result, operationError, transaction);
        }

        

        [Fact]
        public void Cannot_Update_Name_If_Dpr_Resolution_Fails()
        {
            //Arrange
            var parameters = new DataProcessingRegistrationModificationParameters
            {
                Name = A<string>().AsChangedValue()
            };

            var transaction = ExpectTransaction();
            var dprUuid = A<Guid>();
            var operationError = new OperationError(OperationFailure.NotFound);
            SetupGetFromRepository(new DataProcessingRegistration());
            
            //Act
            var result = _sut.Update(dprUuid, parameters);

            //Assert
            AssertFailureWithKnownError(result, operationError, transaction);
        }

        [Fact]
        public void Can_Create_With_GeneralData_DataResponsible()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                DataResponsibleUuid = new Guid?(A<Guid>()).AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, _) = SetupCreateScenarioPrerequisites(generalData: generalData);
            var responsibleId = A<int>();
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingDataResponsibleOption>(generalData.DataResponsibleUuid.NewValue, responsibleId);
            SetupGetFromRepository(createdRegistration);
            SetupDataResponsibleAssignmentClearAndAssign(createdRegistration, responsibleId);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(createdRegistration.OrganizationId, result.Value.OrganizationId);
            Assert.Same(createdRegistration.Name, result.Value.Name);
            AssertDBControlSaveChanges();
        }

        [Fact]
        public void Can_Create_With_GeneralData_With_Null_DataResponsible()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                DataResponsibleUuid = ((Guid?)null).AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, _) = SetupCreateScenarioPrerequisites(generalData: generalData);
            var responsibleId = A<int>();

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingDataResponsibleOption>(generalData.DataResponsibleUuid.NewValue, responsibleId);
            SetupGetFromRepository(createdRegistration);
            SetupDataResponsibleAssignmentClearAndAssign(createdRegistration);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(createdRegistration, result.Value);
            AssertDBControlSaveChanges();
        }

        
        [Fact]
        public void Can_Create_With_GeneralData_With_Null_DataResponsible_If_DprAssignmentService_Responds_With_BadState()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                DataResponsibleUuid = ((Guid?)null).AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, _) = SetupCreateScenarioPrerequisites(generalData: generalData);
            var responsibleId = A<int>();

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingDataResponsibleOption>(generalData.DataResponsibleUuid.NewValue, responsibleId);
            SetupGetFromRepository(createdRegistration);
            SetupDataResponsibleAssignmentClearAndAssign(createdRegistration, 0, new OperationError(OperationFailure.BadState));

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(createdRegistration, result.Value);
            AssertDBControlSaveChanges();
        }
        

        [Fact]
        public void Cannot_Create_With_GeneralData_With_Null_DataResponsible_If_DprService_Fails_With_AnythingBut_BadState()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                DataResponsibleUuid = ((Guid?)null).AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);
            var responsibleId = A<int>();
            var clearError = new OperationError(EnumRange.AllExcept(OperationFailure.BadState).RandomItem());

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingDataResponsibleOption>(generalData.DataResponsibleUuid.NewValue, responsibleId);
            SetupGetFromRepository(createdRegistration);
            SetupDataResponsibleAssignmentClearAndAssign(createdRegistration, 0, clearError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownError(result, clearError, transaction);
        }

        
        [Fact]
        public void Cannot_Create_With_GeneralData_DataResponsible_If_DataResponsible_Id_Cannot_Be_Resolved()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                DataResponsibleUuid = new Guid?(A<Guid>()).AsChangedValue()
            };
            var (organizationUuid, parameters, _, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingDataResponsibleOption>(generalData.DataResponsibleUuid.NewValue, Maybe<int>.None);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(result, "Data responsible option with uuid", OperationFailure.BadInput, transaction);
        }
        
        [Fact]
        public void Create_With_GeneralData_DataResponsible_Set_To_NoChanges()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                DataResponsibleUuid = OptionalValueChange<Guid?>.None
            };
            var (organizationUuid, parameters, dpr, _) = SetupCreateScenarioPrerequisites(generalData: generalData);
            SetupGetFromRepository(dpr);
            SetupDataResponsibleAssignmentClearAndAssign(dpr);
            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            _dataResponsibleAssignmentServiceMock.Verify(assignmentService => assignmentService.Assign(dpr, It.IsAny<int>()), Times.Never);
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_With_GeneralData_DataResponsibleRemark(bool inputIsNull)
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                DataResponsibleRemark = (inputIsNull ? null : A<string>()).AsChangedValue()!
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);
            SetupGetFromRepository(createdRegistration);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(createdRegistration, result.Value);
            AssertDBControlSaveChanges();
        }

        [Fact]
        public void Create_With_GeneralData_DataResponsibleRemark_Set_To_NoChanges()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                DataResponsibleRemark = OptionalValueChange<string>.None
            };
            var (organizationUuid, parameters, _, _) = SetupCreateScenarioPrerequisites(generalData: generalData);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_With_GeneralData_IsAgreementConcluded(bool inputIsNull)
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                IsAgreementConcluded = (inputIsNull ? (YesNoIrrelevantOption?)null : A<YesNoIrrelevantOption>()).AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, _) = SetupCreateScenarioPrerequisites(generalData: generalData);
            SetupGetFromRepository(createdRegistration);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(createdRegistration, result.Value);
            AssertDBControlSaveChanges();
        }
        
        
        [Fact]
        public void Create_With_GeneralData_IsAgreementConcluded_Set_To_NoChanges()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                IsAgreementConcluded = OptionalValueChange<YesNoIrrelevantOption?>.None
            };
            var (organizationUuid, parameters, _, _) = SetupCreateScenarioPrerequisites(generalData: generalData);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
        }
        

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_With_GeneralData_IsAgreementConcludedRemark(bool inputIsNull)
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                IsAgreementConcludedRemark = (inputIsNull ? null : A<string>()).AsChangedValue()!
            };
            var (organizationUuid, parameters, createdRegistration, _) = SetupCreateScenarioPrerequisites(generalData: generalData);
            SetupGetFromRepository(createdRegistration);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(createdRegistration, result.Value);
            AssertDBControlSaveChanges();
        }
        
        [Fact]
        public void Create_With_GeneralData_IsAgreementConcludedRemark_Set_To_NoChanges()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                IsAgreementConcludedRemark = OptionalValueChange<string>.None
            };
            var (organizationUuid, parameters, _, _) = SetupCreateScenarioPrerequisites(generalData: generalData);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            RepositoryAssertAnyDprAdded();
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_With_GeneralData_AgreementConcludedAt(bool inputIsNull)
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                AgreementConcludedAt = (inputIsNull ? (DateTime?)null : A<DateTime>()).AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, _) = SetupCreateScenarioPrerequisites(generalData: generalData);
            SetupGetFromRepository(createdRegistration);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(createdRegistration, result.Value);
            AssertDBControlSaveChanges();
        }

        [Fact]
        public void Create_With_GeneralData_AgreementConcludedAt_Set_To_NoChanges()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                AgreementConcludedAt = OptionalValueChange<DateTime?>.None
            };
            var (organizationUuid, parameters, _, _) = SetupCreateScenarioPrerequisites(generalData: generalData);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            RepositoryAssertAnyDprAdded();
        }
        
        [Fact]
        public void Can_Create_With_GeneralData_BasisForTransfer()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                BasisForTransferUuid = new Guid?(A<Guid>()).AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, _) = SetupCreateScenarioPrerequisites(generalData: generalData);
            var responsibleId = A<int>();

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingBasisForTransferOption>(generalData.BasisForTransferUuid.NewValue, responsibleId);
            SetupGetFromRepository(createdRegistration);
            _basisForTransferAssignmentServiceMock.Setup(assignmentService => assignmentService.Assign(createdRegistration, It.IsAny<int>()))
                .Returns(new DataProcessingBasisForTransferOption());

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(createdRegistration, result.Value);
            AssertDBControlSaveChanges();
        }
        
        [Fact]
        public void Can_Create_With_GeneralData_With_Null_BasisForTransfer()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                BasisForTransferUuid = ((Guid?)null).AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, _) = SetupCreateScenarioPrerequisites(generalData: generalData);
            var responsibleId = A<int>();

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingBasisForTransferOption>(generalData.BasisForTransferUuid.NewValue, responsibleId);
            _basisForTransferAssignmentServiceMock.Setup(service => service.Clear(createdRegistration))
                .Returns(new DataProcessingBasisForTransferOption());
            SetupGetFromRepository(createdRegistration);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(createdRegistration, result.Value);
            AssertDBControlSaveChanges();
        }
        
        [Fact]
        public void Can_Create_With_GeneralData_With_Null_BasisForTransfer_If_DprService_Responds_With_BadState()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                BasisForTransferUuid = ((Guid?)null).AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, _) = SetupCreateScenarioPrerequisites(generalData: generalData);
            var responsibleId = A<int>();

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingBasisForTransferOption>(generalData.BasisForTransferUuid.NewValue, responsibleId);
            SetupGetFromRepository(createdRegistration);
            SetupBasisForTransferAssignmentClearAndAssign(createdRegistration, 0, new OperationError(OperationFailure.BadState));

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(createdRegistration.Id, result.Value.Id);
            Assert.Same(createdRegistration.Name, result.Value.Name);
            AssertDBControlSaveChanges();
        }
        
        [Fact]
        public void Cannot_Create_With_GeneralData_With_Null_BasisForTransfer_If_DprService_Fails_With_AnythingBut_BadState()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                BasisForTransferUuid = ((Guid?)null).AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);
            var responsibleId = A<int>();
            var clearError = new OperationError(EnumRange.AllExcept(OperationFailure.BadState).RandomItem());

            _identityResolverMock.Setup(resolver => resolver.ResolveDbId<ItContract>(It.IsAny<Guid>())).Returns(Maybe<int>.None);
            SetupGetFromRepository(createdRegistration);
            SetupBasisForTransferAssignmentClearAndAssign(createdRegistration, 0, clearError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownError(result, clearError, transaction);
        }
        
        [Fact]
        public void Cannot_Create_With_GeneralData_BasisForTransfer_If_BasisForTransfer_Id_Cannot_Be_Resolved()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                BasisForTransferUuid = new Guid?(A<Guid>()).AsChangedValue()
            };
            var (organizationUuid, parameters, dpr, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingBasisForTransferOption>(generalData.BasisForTransferUuid.NewValue, Maybe<int>.None);
            SetupGetFromRepository(dpr);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(result, "Basis for transfer option with uuid", OperationFailure.BadInput, transaction);
        }
        
        [Fact]
        public void Create_With_GeneralData_BasisForTransfer_Set_To_NoChanges()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                BasisForTransferUuid = OptionalValueChange<Guid?>.None
            };
            var (organizationUuid, parameters, dpr, _) = SetupCreateScenarioPrerequisites(generalData: generalData);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            _basisForTransferAssignmentServiceMock.Verify(service => service.Assign(dpr, A<int>()), Times.Never);
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_With_GeneralData_TransferToInsecureThirdCountries(bool inputIsNull)
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                TransferToInsecureThirdCountries = (inputIsNull ? (YesNoUndecidedOption?)null : A<YesNoUndecidedOption>()).AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);
            SetupGetFromRepository(createdRegistration);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(createdRegistration, result.Value);
            AssertDBControlSaveChanges();
        }

        
        [Fact]
        public void Create_With_GeneralData_TransferToInsecureThirdCountries_Set_To_NoChanges()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                TransferToInsecureThirdCountries = OptionalValueChange<YesNoUndecidedOption?>.None
            };
            var (organizationUuid, parameters, _, _) = SetupCreateScenarioPrerequisites(generalData: generalData);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_With_GeneralData_HasSubDataProcessors(bool inputIsNull)
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                HasSubDataProcessors = (inputIsNull ? (YesNoUndecidedOption?)null : A<YesNoUndecidedOption>()).AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, _) = SetupCreateScenarioPrerequisites(generalData: generalData);
            SetupGetFromRepository(createdRegistration);
            
            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(createdRegistration.Name, result.Value.Name);
            Assert.Equal(createdRegistration.Id, result.Value.Id);
            _sdpRepositoryMock.Verify(repository => repository.RemoveRange(It.IsAny<IEnumerable<SubDataProcessor>>()));
            AssertDBControlSaveChanges();
        }
        
        
        [Fact]
        public void Create_With_GeneralData_HasSubDataProcessors_Set_To_NoChanges()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                HasSubDataProcessors = OptionalValueChange<YesNoUndecidedOption?>.None
            };
            var (organizationUuid, parameters, _, _) = SetupCreateScenarioPrerequisites(generalData: generalData);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_With_GeneralData_InsecureCountriesSubjectToDataTransfer(bool hasInsecureCountries)
        {
            //Arrange
            var inputUuids = hasInsecureCountries ? Many<Guid>().ToList() : new List<Guid>();
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                InsecureCountriesSubjectToDataTransferUuids = inputUuids.FromNullable<IEnumerable<Guid>>().AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);

            ExpectUpdateMultiAssignmentReturns<int, DataProcessingCountryOption>(createdRegistration, inputUuids, Maybe<OperationError>.None);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            AssertDBControlSaveChanges();
        }
        
        [Fact]
        public void Cannot_Create_With_GeneralData_InsecureCountriesSubjectToDataTransfer_If_UpdateMultiAssignment_Fails()
        {
            //Arrange
            var inputUuids = Many<Guid>().ToList();
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                InsecureCountriesSubjectToDataTransferUuids = inputUuids.FromNullable<IEnumerable<Guid>>().AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);

            var operationError = A<OperationError>();
            ExpectUpdateMultiAssignmentReturns<int, DataProcessingCountryOption>(createdRegistration, inputUuids, operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            AssertFailureWithKnownError(result, operationError, transaction);
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_With_GeneralData_DataProcessor(bool hasDataProcessors)
        {
            //Arrange
            var inputUuids = hasDataProcessors ? Many<Guid>().ToList() : new List<Guid>();
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                DataProcessorUuids = inputUuids.FromNullable<IEnumerable<Guid>>().AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, _) = SetupCreateScenarioPrerequisites(generalData: generalData);

            ExpectUpdateMultiAssignmentReturns<int, Organization>(createdRegistration, inputUuids, Maybe<OperationError>.None);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            AssertDBControlSaveChanges();
        }
        
        [Fact]
        public void Cannot_Create_With_GeneralData_DataProcessor_If_UpdateMultiAssignment_Fails()
        {
            //Arrange
            var inputUuids = Many<Guid>().ToList();
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                DataProcessorUuids = inputUuids.FromNullable<IEnumerable<Guid>>().AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);

            var operationError = A<OperationError>();
            ExpectUpdateMultiAssignmentReturns<int, Organization>(createdRegistration, inputUuids, operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            AssertFailureWithKnownError(result, operationError, transaction);
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_With_GeneralData_SubDataProcessor(bool hasSubDataProcessors)
        {
            //Arrange
            var sdpInputs = hasSubDataProcessors ? Many<SubDataProcessorParameter>().ToList() : new List<SubDataProcessorParameter>();
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                SubDataProcessors = sdpInputs.AsEnumerable().FromNullable().AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, _) = SetupCreateScenarioPrerequisites(generalData: generalData);
            sdpInputs.ForEach(sdp =>
            {
                //All ids return correct data
                ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<Organization>(sdp.OrganizationUuid, A<int>());
                if (sdp.BasisForTransferOptionUuid.HasValue) ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingBasisForTransferOption>(sdp.BasisForTransferOptionUuid.Value, A<int>());
                if (sdp.InsecureCountrySubjectToDataTransferUuid.HasValue) ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingCountryOption>(sdp.InsecureCountrySubjectToDataTransferUuid.Value, A<int>());
            });
            ExpectUpdateMultiAssignmentReturns<int, Organization>(createdRegistration, sdpInputs.Select(x => x.OrganizationUuid).FromNullable(), Maybe<OperationError>.None);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            AssertDBControlSaveChanges();
        }
        
        [Fact]
        public void Cannot_Create_With_GeneralData_SubDataProcessor_If_UpdateMultiAssignment_Fails()
        {
            //Arrange
            var sdpInputs = Many<SubDataProcessorParameter>().ToList();
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                SubDataProcessors = sdpInputs.AsEnumerable().FromNullable().AsChangedValue()
            };

            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);

            var operationError = A<OperationError>();
            sdpInputs.ForEach(sdp =>
            {
                //All ids return correct data
                ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<Organization>(sdp.OrganizationUuid, A<int>());
                if (sdp.BasisForTransferOptionUuid.HasValue) ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingBasisForTransferOption>(sdp.BasisForTransferOptionUuid.Value, A<int>());
                if (sdp.InsecureCountrySubjectToDataTransferUuid.HasValue) ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingCountryOption>(sdp.InsecureCountrySubjectToDataTransferUuid.Value, A<int>());
            });
            ExpectUpdateMultiAssignmentReturns<int, Organization>(createdRegistration, sdpInputs.Select(x => x.OrganizationUuid).FromNullable(), operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            AssertFailureWithKnownError(result, operationError, transaction);
        }

        [Fact]
        public void Cannot_Create_With_GeneralData_SubDataProcessor_If_BasisForTransferIdLookup_Fails()
        {
            //Arrange
            var sdpInputs = Many<SubDataProcessorParameter>().ToList();
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                SubDataProcessors = sdpInputs.AsEnumerable().FromNullable().AsChangedValue()
            };

            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);

            var operationError = A<OperationError>();
            sdpInputs.Take(1).ToList().ForEach(sdp =>
            {
                //All ids return correct data
                ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingBasisForTransferOption>(sdp.BasisForTransferOptionUuid!.Value, Maybe<int>.None);
                operationError = new OperationError($"Provided id for basis for transfer {sdp.BasisForTransferOptionUuid.Value} does not point to a valid entity", OperationFailure.BadInput);
            });
            ExpectUpdateMultiAssignmentReturns<int, Organization>(createdRegistration, sdpInputs.Select(x => x.OrganizationUuid).FromNullable(), operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            AssertFailureWithKnownError(result, operationError, transaction);
        }

        [Fact]
        public void Cannot_Create_With_GeneralData_SubDataProcessor_If_InsecureCountry_Fails()
        {
            //Arrange
            var sdpInputs = Many<SubDataProcessorParameter>().ToList();
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                SubDataProcessors = sdpInputs.AsEnumerable().FromNullable().AsChangedValue()
            };

            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);

            var operationError = A<OperationError>();
            sdpInputs.Take(1).ToList().ForEach(sdp =>
            {
                if (sdp.BasisForTransferOptionUuid.HasValue) ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingBasisForTransferOption>(sdp.BasisForTransferOptionUuid.Value, A<int>());
                if (sdp.InsecureCountrySubjectToDataTransferUuid.HasValue)
                {
                    ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingCountryOption>(sdp.InsecureCountrySubjectToDataTransferUuid.Value, Maybe<int>.None);
                    operationError = new OperationError($"Provided id for country {sdp.InsecureCountrySubjectToDataTransferUuid.Value} does not point to a valid entity", OperationFailure.BadInput);
                }
            });
            ExpectUpdateMultiAssignmentReturns<int, Organization>(createdRegistration, sdpInputs.Select(x => x.OrganizationUuid).FromNullable(), operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            AssertFailureWithKnownError(result, operationError, transaction);
        }

        [Fact]
        public void Cannot_Create_With_GeneralData_SubDataProcessor_If_Organization_Fails()
        {
            //Arrange
            var sdpInputs = Many<SubDataProcessorParameter>().ToList();
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                SubDataProcessors = sdpInputs.AsEnumerable().FromNullable().AsChangedValue()
            };

            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);

            var operationError = A<OperationError>();
            sdpInputs.Take(1).ToList().ForEach(sdp =>
            {
                if (sdp.BasisForTransferOptionUuid.HasValue) ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingBasisForTransferOption>(sdp.BasisForTransferOptionUuid.Value, A<int>());
                if (sdp.InsecureCountrySubjectToDataTransferUuid.HasValue) ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingCountryOption>(sdp.InsecureCountrySubjectToDataTransferUuid.Value, A<int>());
                ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<Organization>(sdp.OrganizationUuid, Maybe<int>.None);
                operationError = new OperationError($"Provided org id {sdp.OrganizationUuid} does not point to a valid entity", OperationFailure.BadInput);
            });
            ExpectUpdateMultiAssignmentReturns<int, Organization>(createdRegistration, sdpInputs.Select(x => x.OrganizationUuid).FromNullable(), operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            AssertFailureWithKnownError(result, operationError, transaction);
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_With_GeneralData_MainContract(bool hasMainContract)
        {
            //Arrange
            var inputUuids = hasMainContract ? A<Guid?>() : null;
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                MainContractUuid = inputUuids.AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, _) = SetupCreateScenarioPrerequisites(generalData: generalData);

            var contract = createdRegistration.AssociatedContracts.FirstOrDefault();
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<ItContract>(generalData.MainContractUuid.NewValue, contract!.Id);
            SetupGetFromRepository(createdRegistration);
      
            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            AssertDBControlSaveChanges();
        }

        [Fact]
        public void Cannot_Create_With_GeneralData_MainContract_If_MainContract_Id_Cannot_Be_Resolved()
        {
            //Arrange
            var inputUuids = A<Guid?>();
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                MainContractUuid = inputUuids.AsChangedValue()
            };
            var (organizationUuid, parameters, _, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<ItContract>(generalData.MainContractUuid.NewValue, Maybe<int>.None);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            AssertFailureWithKnownErrorDetails(result, $"It contract with uuid {generalData.MainContractUuid.NewValue} could not be found", OperationFailure.BadInput, transaction);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_With_SystemUsages(bool hasUsages)
        {
            //Arrange
            var usageUuids = hasUsages ? Many<Guid>().ToList() : new List<Guid>();
            var (organizationUuid, parameters, registration, _) = SetupCreateScenarioPrerequisites(systemUsageUuids: usageUuids);

            ExpectUpdateMultiAssignmentReturns<int, ItSystemUsage>(registration, usageUuids, Maybe<OperationError>.None);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            AssertDBControlSaveChanges();
        }

        [Fact]
        public void Cannot_Create_With_SystemUsages_If_UpdateMultiAssignment_Fails()
        {
            //Arrange
            var usageUuids = Many<Guid>().ToList();
            var (organizationUuid, parameters, registration, transaction) = SetupCreateScenarioPrerequisites(systemUsageUuids: usageUuids);

            var operationError = A<OperationError>();
            ExpectUpdateMultiAssignmentReturns<int, ItSystemUsage>(registration, usageUuids, operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            AssertFailureWithKnownError(result, operationError, transaction);
        }
        

        [Fact]
        public void Can_Update_With_SystemUsages_If_Usage_Already_Assigned()
        {
            //Arrange
            var usage = new ItSystemUsage() { Id = A<int>(), Uuid = A<Guid>() };
            var usageUuids = new List<Guid> { usage.Uuid };
            var (_, parameters, registration, transaction) = SetupModifyScenarioPrerequisites(systemUsageUuids: usageUuids);
            registration.SystemUsages.Add(usage);
            SetupGetFromRepository(registration);
            ExpectUpdateMultiAssignmentReturns<int, ItSystemUsage>(registration, usageUuids, Maybe<OperationError>.None);
            parameters.Name = OptionalValueChange<string>.None;
            AllowReadsReturns();
           
            //Act
            var result = _sut.Update(registration.Uuid, parameters);

            //Assert
            Assert.True(result.Ok);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Can_Update_With_SystemUsages_To_Remove_Usage()
        {
            //Arrange
            var usage = new ItSystemUsage() { Id = A<int>(), Uuid = A<Guid>() };
            var usageUuids = new List<Guid>();
            var (_, parameters, registration, transaction) = SetupModifyScenarioPrerequisites(systemUsageUuids: usageUuids);
            registration.SystemUsages.Add(usage);

            SetupGetFromRepository(registration);
            ExpectUpdateMultiAssignmentReturns<int, ItSystemUsage>(registration, usageUuids, Maybe<OperationError>.None);
            parameters.Name = OptionalValueChange<string>.None;
            AllowReadsReturns();

            //Act
            var result = _sut.Update(registration.Uuid, parameters);

            //Assert
            Assert.True(result.Ok);
            AssertTransactionCommitted(transaction);
        }


        [Fact]
        public void Can_Delete()
        {
            //Arrange
            var uuid = A<Guid>();
            var resolvedDbId = A<int>();
            AllowDeleteReturns();
            ExpectTransaction();
            SetupGetFromRepository(new DataProcessingRegistration());
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingRegistration>(uuid, resolvedDbId);

            //Act
            var result = _sut.Delete(uuid);

            //Assert
            Assert.True(result.IsNone, "No errors should occur during deletion");
        }

        [Fact]
        public void Cannot_Delete_If_IdentityResolutionFails()
        {
            //Arrange
            var uuid = A<Guid>();
            var resolvedDbId = A<int>();
            var operationError = A<OperationError>();
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingRegistration>(uuid, Maybe<int>.None);

            //Act
            var result = _sut.Delete(uuid);

            //Assert
            Assert.True(result.HasValue);
            Assert.Equal(OperationFailure.NotFound, result.Value.FailureType);
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_With_Oversight_OversightOptions(bool hasOversightOptions)
        {
            //Arrange
            var inputUuids = hasOversightOptions ? Many<Guid>().ToList() : new List<Guid>();
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters()
            {
                OversightOptionUuids = inputUuids.FromNullable<IEnumerable<Guid>>().AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, _) = SetupCreateScenarioPrerequisites(oversightData: oversightData);

            ExpectUpdateMultiAssignmentReturns<int, DataProcessingOversightOption>(createdRegistration, inputUuids, Maybe<OperationError>.None);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            AssertDBControlSaveChanges();
        }

        [Fact]
        public void Cannot_Create_With_Oversight_OversightOptions_If_UpdateMultiAssignment_Fails()
        {
            //Arrange
            var inputUuids = Many<Guid>().ToList();
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters()
            {
                OversightOptionUuids = inputUuids.FromNullable<IEnumerable<Guid>>().AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(oversightData: oversightData);

            var operationError = A<OperationError>();
            ExpectUpdateMultiAssignmentReturns<int, DataProcessingOversightOption>(createdRegistration, inputUuids, operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            AssertFailureWithKnownError(result, operationError, transaction);
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_With_OversightData_OversightOptionsRemark(bool inputIsNull)
        {
            //Arrange
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters()
            {
                OversightOptionsRemark = (inputIsNull ? null : A<string>()).AsChangedValue()!
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(oversightData: oversightData);
            SetupGetFromRepository(createdRegistration);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertCreatedRegistrationNameAndId(createdRegistration, result);
            AssertDBControlSaveChanges();
        }

        [Fact]
        public void Can_Create_With_OversightData_OversightOptionsRemark_Set_To_NoChanges()
        {
            //Arrange
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters()
            {
                OversightOptionsRemark = OptionalValueChange<string>.None
            };
            var (organizationUuid, parameters, dpr, _) = SetupCreateScenarioPrerequisites(oversightData: oversightData);
            SetupGetFromRepository(dpr);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_With_OversightData_OversightInterval(bool inputIsNull)
        {
            //Arrange
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters()
            {
                OversightInterval = (inputIsNull ? (YearMonthIntervalOption?)null : A<YearMonthIntervalOption>()).AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, _) = SetupCreateScenarioPrerequisites(oversightData: oversightData);
            SetupGetFromRepository(createdRegistration);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertCreatedRegistrationNameAndId(createdRegistration, result);
            AssertDBControlSaveChanges();
        }

        [Fact]
        public void Can_Create_With_OversightData_OversightInterval_Set_To_NoChanges()
        {
            //Arrange
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters()
            {
                OversightInterval = OptionalValueChange<YearMonthIntervalOption?>.None
            };
            var (organizationUuid, parameters, _, _) = SetupCreateScenarioPrerequisites(oversightData: oversightData);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
        }

        

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_With_OversightData_OversightIntervalRemark(bool inputIsNull)
        {
            //Arrange
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters()
            {
                OversightIntervalRemark = (inputIsNull ? null : A<string>()).AsChangedValue()!
            };
            var (organizationUuid, parameters, createdRegistration, _) = SetupCreateScenarioPrerequisites(oversightData: oversightData);

            SetupGetFromRepository(createdRegistration);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertCreatedRegistrationNameAndId(createdRegistration, result);
            AssertDBControlSaveChanges();
        }

        [Fact]
        public void Can_Create_With_OversightData_OversightIntervalRemark_Set_To_NoChanges()
        {
            //Arrange
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters()
            {
                OversightIntervalRemark = OptionalValueChange<string>.None
            };
            var (organizationUuid, parameters, dpr, _) = SetupCreateScenarioPrerequisites(oversightData: oversightData);
            SetupGetFromRepository(dpr);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_OversightIntervalRemark(bool inputIsNull)
        {
            //Arrange
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters
            {
                OversightScheduledInspectionDate = (inputIsNull ? null : A<DateTime?>()).AsChangedValue()
            };

            var (organizationUuid, parameters, createdRegistration, _) = SetupCreateScenarioPrerequisites(oversightData: oversightData);
            SetupGetFromRepository(createdRegistration);
          //  ExpectTransaction();

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertCreatedRegistrationNameAndId(createdRegistration, result);
            AssertDBControlSaveChanges();
        }

        [Fact]
        public void Can_Create_OversightIntervalRemark_Set_To_NoChanges()
        {
            //Arrange
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters()
            {
                OversightScheduledInspectionDate = OptionalValueChange<DateTime?>.None
            };
            var (organizationUuid, parameters, dpr, _) = SetupCreateScenarioPrerequisites(oversightData: oversightData);
            SetupGetFromRepository(dpr);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_With_OversightData_IsOversightCompleted(bool inputIsNull)
        {
            //Arrange
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters()
            {
                IsOversightCompleted = (inputIsNull ? (YesNoUndecidedOption?)null : A<YesNoUndecidedOption>()).AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, _) = SetupCreateScenarioPrerequisites(oversightData: oversightData);
            SetupGetFromRepository(createdRegistration);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertCreatedRegistrationNameAndId(createdRegistration, result);
            AssertDBControlSaveChanges();
        }

        [Fact]
        public void Can_Create_With_OversightData_IsOversightCompleted_Set_To_NoChanges()
        {
            //Arrange
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters()
            {
                IsOversightCompleted = OptionalValueChange<YesNoUndecidedOption?>.None
            };
            var (organizationUuid, parameters, dpr, _) = SetupCreateScenarioPrerequisites(oversightData: oversightData);
            SetupGetFromRepository(dpr);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_With_OversightData_OversightCompletedRemark(bool inputIsNull)
        {
            //Arrange
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters()
            {
                OversightCompletedRemark = (inputIsNull ? null : A<string>()).AsChangedValue()!
            };
            var (organizationUuid, parameters, createdRegistration, _) = SetupCreateScenarioPrerequisites(oversightData: oversightData);
            SetupGetFromRepository(createdRegistration);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertCreatedRegistrationNameAndId(createdRegistration, result);
            AssertDBControlSaveChanges();
        }

        [Fact]
        public void Can_Create_With_OversightData_OversightCompletedRemark_Set_To_NoChanges()
        {
            //Arrange
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters()
            {
                OversightCompletedRemark = OptionalValueChange<string>.None
            };
            var (organizationUuid, parameters, dpr, _) = SetupCreateScenarioPrerequisites(oversightData: oversightData);
            SetupGetFromRepository(dpr);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
        }
        
        [Fact]
        public void Can_Create_With_OversightData_OversightDates()
        {
            //Arrange
            var dates = Many<UpdatedDataProcessingRegistrationOversightDate>().ToList();
            dates.ForEach(x => x.OversightOptionUuid = null);
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters()
            {
                OversightDates = dates.FromNullable<IEnumerable<UpdatedDataProcessingRegistrationOversightDate>>().AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(oversightData: oversightData);
            createdRegistration.IsOversightCompleted = YesNoUndecidedOption.Yes;
            SetupGetFromRepository(createdRegistration);
            
            //Act
            var result = _sut.Create(organizationUuid, parameters);
            //Assert
            Assert.True(result.Ok);
            Assert.Same(createdRegistration, result.Value);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Can_Create_With_OversightData_OversightDates_Set_To_NoChanges()
        {
            //Arrange
            var dates = Many<UpdatedDataProcessingRegistrationOversightDate>().ToList();
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters()
            {
                OversightDates = OptionalValueChange<Maybe<IEnumerable<UpdatedDataProcessingRegistrationOversightDate>>>.None
            };
            var (organizationUuid, parameters, createdRegistration, _) = SetupCreateScenarioPrerequisites(oversightData: oversightData);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(createdRegistration, result.Value);
            AssertDBControlSaveChanges();
        }
        
        [Fact]
        public void Can_Create_With_Roles()
        {
            //Arrange
            var roleId = A<int>();
            var roleUuid = A<Guid>();
            var userId = A<int>();
            var userUuid = A<Guid>();

            var rolePairs = new List<UserRolePair>() { CreateUserRolePair(roleUuid, userUuid) };
            var roles = CreateUpdatedDataProcessingRegistrationRoles(rolePairs);
            var (organizationUuid, parameters, createdRegistration, _) = SetupCreateScenarioPrerequisites(roles: roles);
            var right = CreateRight(createdRegistration, roleUuid, roleId, userUuid, userId);
            SetupGetFromRepository(createdRegistration);

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<User>(userUuid, userId);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingRegistrationRole>(roleUuid, roleId);
            ExpectRoleAssignmentReturns(right);

            //Act
            var createResult = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(createResult.Ok);
            _roleAssignmentsServiceMock.Verify(_ => _.AssignRole(createdRegistration, roleId, userId), Times.Once);
            AssertDBControlSaveChanges();
        }

        
        [Fact]
        public void Can_Create_With_No_Roles()
        {
            //Arrange
            var roles = new UpdatedDataProcessingRegistrationRoles() { UserRolePairs = Maybe<IEnumerable<UserRolePair>>.None.AsChangedValue() };
            var (organizationUuid, parameters, createdRegistration, _) = SetupCreateScenarioPrerequisites(roles: roles);

            //Act
            var createResult = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(createResult.Ok);
            _roleAssignmentsServiceMock.Verify(x => x.AssignRole(createdRegistration, It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            _roleAssignmentsServiceMock.Verify(x => x.RemoveRole(createdRegistration, It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            AssertDBControlSaveChanges();
        }
        
        [Fact]
        public void Cannot_Create_With_Roles_If_RoleUuid_Not_Found()
        {
            //Arrange
            var roleUuid = A<Guid>();
            var userId = A<int>();
            var userUuid = A<Guid>();

            var rolePairs = new List<UserRolePair>() { CreateUserRolePair(roleUuid, userUuid) };

            var roles = CreateUpdatedDataProcessingRegistrationRoles(rolePairs);

            var (organizationUuid, parameters, _, transaction) = SetupCreateScenarioPrerequisites(roles: roles);

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<User>(userUuid, userId);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingRegistrationRole>(roleUuid, Maybe<int>.None);

            //Act
            var createResult = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(createResult, $"Could not find role with Uuid: {roleUuid}", OperationFailure.BadInput, transaction);
        }
        
        [Fact]
        public void Cannot_Create_With_Roles_If_UserUuid_Not_Found()
        {
            //Arrange
            var roleId = A<int>();
            var roleUuid = A<Guid>();
            var userUuid = A<Guid>();

            var rolePairs = new List<UserRolePair>() { CreateUserRolePair(roleUuid, userUuid) };

            var roles = CreateUpdatedDataProcessingRegistrationRoles(rolePairs);

            var (organizationUuid, parameters, _, transaction) = SetupCreateScenarioPrerequisites(roles: roles);


            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<User>(userUuid, Maybe<int>.None);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingRegistrationRole>(roleUuid, roleId);

            //Act
            var createResult = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(createResult, $"Could not find user with Uuid: {userUuid}", OperationFailure.BadInput, transaction);
        }

        [Fact]
        public void Cannot_Create_With_Roles_If_Role_Assignment_Fails()
        {
            //Arrange
            var roleId = A<int>();
            var roleUuid = A<Guid>();
            var userId = A<int>();
            var userUuid = A<Guid>();

            var rolePairs = new List<UserRolePair>() { CreateUserRolePair(roleUuid, userUuid) };

            var roles = CreateUpdatedDataProcessingRegistrationRoles(rolePairs);
            var (organizationUuid, parameters, dpr, transaction) = SetupCreateScenarioPrerequisites(roles: roles);
            SetupGetFromRepository(dpr);

            var error = A<OperationError>();

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<User>(userUuid, userId);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingRegistrationRole>(roleUuid, roleId);
            ExpectRoleAssignmentReturns(error);

            //Act
            var createResult = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(createResult, $"Failed to assign role with Uuid: {roleUuid} from user with Uuid: {userUuid}, with following error message:", error.FailureType, transaction);
        }

        [Fact]
        public void Cannot_Create_With_Roles_If_Duplicates()
        {
            //Arrange
            var roleId = A<int>();
            var roleUuid = A<Guid>();
            var userId = A<int>();
            var userUuid = A<Guid>();

            var rolePairs = new List<UserRolePair>() { CreateUserRolePair(roleUuid, userUuid), CreateUserRolePair(roleUuid, userUuid) };

            var roles = CreateUpdatedDataProcessingRegistrationRoles(rolePairs);

            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(roles: roles);
            var right = CreateRight(createdRegistration, roleUuid, roleId, userUuid, userId);

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<User>(userUuid, userId);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingRegistrationRole>(roleUuid, roleId);
            ExpectRoleAssignmentReturns(right);

            //Act
            var createResult = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(createResult, "Duplicates of 'User Role Pairs' are not allowed", OperationFailure.BadInput, transaction);
        }
        
        [Fact]
        public void Can_Update_Roles_To_Remove_Them()
        {
            //Arrange
            var (_, _, createdRegistration, transaction) = SetupCreateScenarioPrerequisites();

            var newRight = CreateRight(createdRegistration, A<Guid>(), A<int>(), A<Guid>(), A<int>());
            var newUserRolePair = CreateUserRolePair(newRight.Role.Uuid, newRight.User.Uuid);

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<User>(newRight.User.Uuid, newRight.UserId);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingRegistrationRole>(newRight.Role.Uuid, newRight.RoleId);

            ExpectRoleAssignmentReturns(newRight);

            var rightToRemove = CreateRight(createdRegistration, A<Guid>(), A<int>(), A<Guid>(), A<int>());

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<User>(rightToRemove.User.Uuid, rightToRemove.UserId);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingRegistrationRole>(rightToRemove.Role.Uuid, rightToRemove.RoleId);

            var rightToKeep = CreateRight(createdRegistration, A<Guid>(), A<int>(), A<Guid>(), A<int>());
            var userRolePairToKeep = CreateUserRolePair(rightToKeep.Role.Uuid, rightToKeep.User.Uuid);
            createdRegistration.Rights.Add(rightToRemove);
            createdRegistration.Rights.Add(rightToKeep);

            ExpectRoleRemovalReturns(createdRegistration, rightToRemove);
            SetupGetFromRepository(createdRegistration);
            AllowReadsReturns();
            var roles = CreateUpdatedDataProcessingRegistrationRoles(new List<UserRolePair>
            {
                newUserRolePair, userRolePairToKeep
            });

            var updateParameters = new DataProcessingRegistrationModificationParameters
            {
                Roles = roles
            };

            //Act
            var updateResult = _sut.Update(createdRegistration.Uuid, updateParameters);

            //Assert
            Assert.True(updateResult.Ok);
            AssertTransactionCommitted(transaction);
            _roleAssignmentsServiceMock.Verify(x => x.AssignRole(createdRegistration, newRight.RoleId, newRight.UserId), Times.Once);
            _roleAssignmentsServiceMock.Verify(x => x.RemoveRole(createdRegistration, rightToRemove.RoleId, rightToRemove.UserId), Times.Once);

            _roleAssignmentsServiceMock.Verify(x => x.AssignRole(createdRegistration, rightToKeep.RoleId, rightToKeep.UserId), Times.Never);
            _roleAssignmentsServiceMock.Verify(x => x.RemoveRole(createdRegistration, rightToKeep.RoleId, rightToKeep.UserId), Times.Never);
        }
        
        [Fact]
        public void Can_Create_With_ExternalReferences()
        {
            //Arrange
            var externalReferences = Many<UpdatedExternalReferenceProperties>().ToList();
            var (organizationUuid, parameters, createdRegistration, _) = SetupCreateScenarioPrerequisites(externalReferences: externalReferences);

            ExpectBatchUpdateExternalReferencesReturns(createdRegistration, externalReferences, Maybe<OperationError>.None);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            AssertDBControlSaveChanges();
        }

        [Fact]
        public void Cannot_Create_With_ExternalReferences_If_BatchUpdate_Fails()
        {
            //Arrange
            var externalReferences = Many<UpdatedExternalReferenceProperties>().ToList();
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(externalReferences: externalReferences);

            var operationError = A<OperationError>();

            ExpectBatchUpdateExternalReferencesReturns(createdRegistration, externalReferences, operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(result, "Failed to update references with error message", operationError.FailureType, transaction);
        }
        
        [Fact]
        public void Can_Add_Role()
        {
            //Arrange
            var existingAssignment = CreateUpdatedDataProcessingRegistrationRoles(A<UserRolePair>().WrapAsEnumerable());
            var (_, _, dpr, transaction) = SetupCreateScenarioPrerequisites(roles: existingAssignment);
            var newRight = CreateRight(dpr, A<Guid>(), A<int>(), A<Guid>(), A<int>());
            var newAssignment = CreateUserRolePair(newRight.Role.Uuid, newRight.User.Uuid);

            SetupGetFromRepository(dpr);

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<User>(newAssignment.UserUuid, newRight.UserId);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingRegistrationRole>(newAssignment.RoleUuid, newRight.RoleId);
            AllowReadsReturns();
            ExpectRoleAssignmentReturns(newRight);

            //Act
            var createResult = _sut.AddRole(dpr.Uuid, newAssignment);

            //Assert
            Assert.True(createResult.Ok);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Can_Remove_Role()
        {
            //Arrange
            var userRolePairToRemove = A<UserRolePair>();
            var userId = A<int>();
            var roleId = A<int>();
            var existingAssignment = CreateUpdatedDataProcessingRegistrationRoles(new List<UserRolePair>
                { userRolePairToRemove, A<UserRolePair>() });
            var (_, _, dpr, transaction) = SetupCreateScenarioPrerequisites(roles: existingAssignment);
            var rightToRemove = CreateRight(dpr, userRolePairToRemove.RoleUuid, roleId, userRolePairToRemove.UserUuid, userId);
            dpr.Rights = new List<DataProcessingRegistrationRight> { rightToRemove };

            SetupGetFromRepository(dpr);
            AllowReadsReturns();
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<User>(userRolePairToRemove.UserUuid, userId);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingRegistrationRole>(userRolePairToRemove.RoleUuid, roleId);

            ExpectRoleRemovalReturns(dpr, rightToRemove);

            //Act
            var createResult = _sut.RemoveRole(dpr.Uuid, userRolePairToRemove);

            //Assert
            Assert.True(createResult.Ok);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Can_Create_With_Responsible_Unit()
        {
            Guid? orgUnitUuid = A<Guid>();
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters { ResponsibleUnitUuid = orgUnitUuid.AsChangedValue() };
            var (orgUuid, parameters, dpr, _) = SetupCreateScenarioPrerequisites(generalData: generalData);
            dpr.Organization = new Organization { OrgUnits = { new OrganizationUnit { Uuid = orgUnitUuid.Value } } };

            var result = _sut.Create(orgUuid, parameters);

            Assert.True(result.Ok);
            Assert.Equal(result.Value.ResponsibleOrganizationUnit.Uuid, orgUnitUuid);
        }

        [Fact]
        public void Can_Update_With_Responsible_Unit()
        {
            //Arrange
            Guid? orgUnitUuid = A<Guid>();
            var parameters = new DataProcessingRegistrationModificationParameters
            {
                General = new UpdatedDataProcessingRegistrationGeneralDataParameters { ResponsibleUnitUuid = orgUnitUuid.AsChangedValue() }
            };
            var dataProcessingRegistration = new DataProcessingRegistration
            {
                Organization = new Organization { OrgUnits = { new OrganizationUnit { Uuid = orgUnitUuid.Value } } }, Uuid = A<Guid>()
            };
            var transaction = ExpectTransaction();
            var dprUuid = dataProcessingRegistration.Uuid;
            AllowReadsReturns();
            SetupGetFromRepository(dataProcessingRegistration);
            
            //Act
            var result = _sut.Update(dprUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(result.Value.ResponsibleOrganizationUnit.Uuid, orgUnitUuid);
            AssertTransactionCommitted(transaction);
        }
        
        [Fact]
        public void Can_Reset_Responsible_Unit()
        {
            //Arrange
            Guid? orgUnitUuid = null;
            var parameters = new DataProcessingRegistrationModificationParameters
            {
                General = new UpdatedDataProcessingRegistrationGeneralDataParameters { ResponsibleUnitUuid = orgUnitUuid.AsChangedValue() }
            };
            var dataProcessingRegistration = new DataProcessingRegistration
            {
                ResponsibleOrganizationUnit = new OrganizationUnit { Uuid = A<Guid>() }, Uuid = A<Guid>(),
                Organization = new Organization() { Uuid = A<Guid>() }
            };
            var transaction = ExpectTransaction();
            var dprUuid = dataProcessingRegistration.Uuid;
            SetupGetFromRepository(dataProcessingRegistration);
            AllowReadsReturns();

            //Act
            var result = _sut.Update(dprUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Null(result.Value.ResponsibleOrganizationUnit);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Can_Not_Update_If_Unit_Does_Not_Exist_In_Organization()
        {
            //Arrange
            Guid? orgUnitUuid = A<Guid>();
            var parameters = new DataProcessingRegistrationModificationParameters
            {
                General = new UpdatedDataProcessingRegistrationGeneralDataParameters { ResponsibleUnitUuid = orgUnitUuid.AsChangedValue() }
            };
            var dataProcessingRegistration = new DataProcessingRegistration
            {
                Organization = new Organization(),
                Uuid = A<Guid>()
            };
            var transaction = ExpectTransaction();
            var dprUuid = dataProcessingRegistration.Uuid;
            SetupGetFromRepository(dataProcessingRegistration);

            //Act
            var result = _sut.Update(dprUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            AssertTransactionNotCommitted(transaction);
        }

        
        [Fact]
        public void Can_AddOversightDate()
        {
            // Arrange
            var registrationUuid = A<Guid>();
            var parameters = A<UpdatedDataProcessingRegistrationOversightDateParameters>();
            parameters.OversightOptionUuid = OptionalValueChange<Guid?>.None;

            var transaction = ExpectTransaction();
            SetupGetFromRepository(new DataProcessingRegistration() { Uuid = registrationUuid, IsOversightCompleted = YesNoUndecidedOption.Yes, Organization = new Organization() { Uuid = A<Guid>() } });
            AllowReadsReturns();

            // Act
            var result = _sut.AddOversightDate(registrationUuid, parameters);

            // Assert
            Assert.True(result.Ok);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Can_AddOversightDate_With_OversightOption()
        {
            // Arrange
            var registrationUuid = A<Guid>();
            var oversightOptionUuid = A<Guid>();
            var oversightOptionId = A<int>();
            var parameters = A<UpdatedDataProcessingRegistrationOversightDateParameters>();
            parameters.OversightOptionUuid = OptionalValueChange<Guid?>.With(oversightOptionUuid);

            var transaction = ExpectTransaction();
            SetupGetFromRepository(new DataProcessingRegistration { Uuid = registrationUuid, IsOversightCompleted = YesNoUndecidedOption.Yes, Organization = new Organization() { Uuid = A<Guid>() } });
            _identityResolverMock.Setup(x => x.ResolveDbId<DataProcessingOversightOption>(oversightOptionUuid)).Returns(oversightOptionId);
            AllowReadsReturns();

            // Act
            var result = _sut.AddOversightDate(registrationUuid, parameters);

            // Assert
            Assert.True(result.Ok);
            Assert.Equal(oversightOptionId, result.Value.OversightOptionId);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Cannot_AddOversightDate_If_OversightOption_Does_Not_Exist()
        {
            // Arrange
            var registrationUuid = A<Guid>();
            var oversightOptionUuid = A<Guid>();
            var parameters = A<UpdatedDataProcessingRegistrationOversightDateParameters>();
            parameters.OversightOptionUuid = OptionalValueChange<Guid?>.With(oversightOptionUuid);

            var transaction = ExpectTransaction();
            SetupGetFromRepository(new DataProcessingRegistration { Uuid = registrationUuid, IsOversightCompleted = YesNoUndecidedOption.Yes,
                Organization = new Organization() { Uuid = A<Guid>() }
            });
            _identityResolverMock.Setup(x => x.ResolveDbId<DataProcessingOversightOption>(oversightOptionUuid)).Returns(Maybe<int>.None);
            AllowReadsReturns();

            // Act
            var result = _sut.AddOversightDate(registrationUuid, parameters);

            // Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            AssertTransactionNotCommitted(transaction);
        }
        
        [Fact]
        public void Can_UpdateOversightDate()
        {
            // Arrange
            var registrationUuid = A<Guid>();
            var oversightDateUuid = A<Guid>();
            var registrationId = A<int>();
            var oversightDateId = A<int>();
            var parameters = A<UpdatedDataProcessingRegistrationOversightDateParameters>();
            parameters.OversightOptionUuid = OptionalValueChange<Guid?>.None;
            var oversightDate = new DataProcessingRegistrationOversightDate
            {
                Id = oversightDateId,
                Uuid = oversightDateUuid
            };
            var dpr = new DataProcessingRegistration
            {
                Id = registrationId,
                OversightDates = new List<DataProcessingRegistrationOversightDate>{oversightDate},
                IsOversightCompleted = YesNoUndecidedOption.Yes,
                Uuid = registrationUuid,
                Organization = new Organization() { Uuid = A<Guid>() },
            };

            var transaction = ExpectTransaction();
            SetupGetFromRepository(dpr);
            AllowReadsReturns();

            // Act
            var result = _sut.UpdateOversightDate(registrationUuid, oversightDateUuid, parameters);

            // Assert
            Assert.True(result.Ok);
            Assert.NotNull(result.Value);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Can_UpdateOversightDate_With_OversightOption()
        {
            // Arrange
            var registrationUuid = A<Guid>();
            var oversightDateUuid = A<Guid>();
            var oversightOptionUuid = A<Guid>();
            var oversightOptionId = A<int>();
            var oversightDateId = A<int>();
            var parameters = A<UpdatedDataProcessingRegistrationOversightDateParameters>();
            parameters.OversightOptionUuid = OptionalValueChange<Guid?>.With(oversightOptionUuid);
            var oversightDate = new DataProcessingRegistrationOversightDate
            {
                Id = oversightDateId,
                Uuid = oversightDateUuid
            };
            var dpr = new DataProcessingRegistration
            {
                OversightDates = new List<DataProcessingRegistrationOversightDate> { oversightDate },
                IsOversightCompleted = YesNoUndecidedOption.Yes,
                Uuid = registrationUuid,
                Organization = new Organization() { Uuid = A<Guid>() },
            };

            var transaction = ExpectTransaction();
            SetupGetFromRepository(dpr);
            _identityResolverMock.Setup(x => x.ResolveDbId<DataProcessingOversightOption>(oversightOptionUuid)).Returns(oversightOptionId);
            AllowReadsReturns();

            // Act
            var result = _sut.UpdateOversightDate(registrationUuid, oversightDateUuid, parameters);

            // Assert
            Assert.True(result.Ok);
            Assert.Equal(oversightOptionId, result.Value.OversightOptionId);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Cannot_UpdateOversightDate_If_OversightOption_Does_Not_Exist()
        {
            // Arrange
            var registrationUuid = A<Guid>();
            var oversightDateUuid = A<Guid>();
            var oversightOptionUuid = A<Guid>();
            var oversightDateId = A<int>();
            var parameters = A<UpdatedDataProcessingRegistrationOversightDateParameters>();
            parameters.OversightOptionUuid = OptionalValueChange<Guid?>.With(oversightOptionUuid);
            var oversightDate = new DataProcessingRegistrationOversightDate
            {
                Id = oversightDateId,
                Uuid = oversightDateUuid
            };
            var dpr = new DataProcessingRegistration
            {
                OversightDates = new List<DataProcessingRegistrationOversightDate> { oversightDate },
                IsOversightCompleted = YesNoUndecidedOption.Yes,
                Uuid = registrationUuid,
                Organization = new Organization() { Uuid = A<Guid>() }
            };

            var transaction = ExpectTransaction();
            SetupGetFromRepository(dpr);
            _identityResolverMock.Setup(x => x.ResolveDbId<DataProcessingOversightOption>(oversightOptionUuid)).Returns(Maybe<int>.None);
            AllowReadsReturns();

            // Act
            var result = _sut.UpdateOversightDate(registrationUuid, oversightDateUuid, parameters);

            // Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            AssertTransactionNotCommitted(transaction);
        }

        [Fact]
        public void Given_NegativeResponseFromAuthorizationModel_UpdateOversightDate_Returns_ForbiddenError()
        {
            var parameters = A<UpdatedDataProcessingRegistrationOversightDateParameters>();
            var registrationUuid = A<Guid>();
            var oversightDateUuid = A<Guid>();
            var dpr = new DataProcessingRegistration()
            {
                OversightDates = new List<DataProcessingRegistrationOversightDate>()
                {
                    new()
                    {
                        Uuid = oversightDateUuid,
                        Id = A<int>()
                    }
                },
                Uuid = registrationUuid,
                Organization = new Organization() { Uuid = A<Guid>() }
            };
            SetupGetFromRepository(dpr);
            SetupAuthorizationModelReturns(false, dpr, parameters);

            var result = _sut.UpdateOversightDate(registrationUuid, oversightDateUuid, parameters);

            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Given_NegativeResponseFromAuthorizationModel_Update_Returns_ForbiddenError()
        {
            var parameters = A<DataProcessingRegistrationModificationParameters>();
            var registrationUuid = A<Guid>();
            var dpr = new DataProcessingRegistration()
            {
                Uuid = registrationUuid
            };
            SetupGetFromRepository(dpr);
            SetupAuthorizationModelReturns(false, dpr, parameters);

            var result = _sut.Update(registrationUuid, parameters);

            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Can_DeleteOversightDate()
        {
            // Arrange
            var registrationUuid = A<Guid>();
            var oversightDateUuid = A<Guid>();
            var registrationId = A<int>();
            var oversightDateId = A<int>();
            var oversightDate = new DataProcessingRegistrationOversightDate
            {
                Id = oversightDateId,
                Uuid = oversightDateUuid
            };
            var dpr = new DataProcessingRegistration
            {
                Id = registrationId,
                OversightDates = new List<DataProcessingRegistrationOversightDate> { oversightDate },
                IsOversightCompleted = YesNoUndecidedOption.Yes,
                Uuid = registrationUuid
            };

            var transaction = ExpectTransaction();
            SetupGetFromRepository(dpr);
            AllowReadsReturns();

            // Act
            var result = _sut.DeleteOversightDate(registrationUuid, oversightDateUuid);

            // Assert
            Assert.True(result.IsNone);
            _oversightDateRepositoryMock.Verify(repository => repository.Delete(oversightDate));
            transaction.Verify(x => x.Commit());
        }

        private void SetupAuthorizationModelReturns(bool value, DataProcessingRegistration dpr, ISupplierAssociatedEntityUpdateParameters parameters)
        {
            _authorizationModelMock.Setup(_ => _.AuthorizeUpdate(dpr, parameters)).Returns((Result<bool, OperationError>)value);
        }

        private void ExpectBatchUpdateExternalReferencesReturns(DataProcessingRegistration dpr, IEnumerable<UpdatedExternalReferenceProperties> externalReferences, Maybe<OperationError> value)
        {
            _referenceServiceMock
                .Setup(x => x.UpdateExternalReferences(ReferenceRootType.DataProcessingRegistration, dpr.Id, externalReferences))
                .Returns(value);
        }

        private static DataProcessingRegistrationRight CreateRight(DataProcessingRegistration dpr, Guid roleUuid, int roleId, Guid userUuid, int userId)
        {
            return new DataProcessingRegistrationRight()
            {
                Object = dpr,
                Role = new DataProcessingRegistrationRole()
                {
                    Id = roleId,
                    Uuid = roleUuid
                },
                RoleId = roleId,
                User = new User()
                {
                    Id = userId,
                    Uuid = userUuid
                },
                UserId = userId
            };
        }

        private void ExpectRoleAssignmentReturns(Result<DataProcessingRegistrationRight, OperationError> result)
        {
            _roleAssignmentsServiceMock.Setup(x => x.AssignRole(It.IsAny<DataProcessingRegistration>(), It.IsAny<int>(), It.IsAny<int>())).Returns(result);
        }

        private void ExpectRoleRemovalReturns(DataProcessingRegistration dpr, Result<DataProcessingRegistrationRight, OperationError> result)
        {
            _roleAssignmentsServiceMock.Setup(x => x.RemoveRole(dpr, It.IsAny<int>(), It.IsAny<int>())).Returns(result);
        }

        private static UpdatedDataProcessingRegistrationRoles CreateUpdatedDataProcessingRegistrationRoles(IEnumerable<UserRolePair> roles)
        {
            return new UpdatedDataProcessingRegistrationRoles
            {
                UserRolePairs = roles.FromNullable().AsChangedValue()
            };
        }
        private static UserRolePair CreateUserRolePair(Guid roleUuid, Guid userUuid)
        {
            return new UserRolePair(userUuid, roleUuid);
        }

        private void AssertCreatedRegistrationNameAndId(DataProcessingRegistration createdRegistration, Result<DataProcessingRegistration, OperationError> result)
        {
            Assert.True(result.Ok);
            Assert.Same(createdRegistration.Name, result.Value.Name);
            Assert.Equal(createdRegistration.Id, result.Value.Id);
        }

        private void AllowDeleteReturns(bool value = true)
        {
            _authorizationContextMock.Setup(context => context.AllowDelete(It.IsAny<DataProcessingRegistration>())).Returns(value);
        }

        private void RepositoryAssertAnyDprAdded()
        {
            _repositoryMock.Verify(repository => repository.Add(It.IsAny<DataProcessingRegistration>()));
        }

        private void SetupBasisForTransferAssignmentClearAndAssign(DataProcessingRegistration dpr, int responsibleId = 0, OperationError? clearOperationError = null)
        {
            if (clearOperationError != null)
            {
                _basisForTransferAssignmentServiceMock.Setup(assignmentService => assignmentService.Clear(dpr)).Returns(Result<DataProcessingBasisForTransferOption, OperationError>.Failure(clearOperationError));
            }
            else
            {
                _basisForTransferAssignmentServiceMock.Setup(assignmentService => assignmentService.Clear(dpr)).Returns(new DataProcessingBasisForTransferOption());

            }
            _basisForTransferAssignmentServiceMock.Setup(assignmentService => assignmentService.Assign(dpr, responsibleId)).Returns(new DataProcessingBasisForTransferOption());
        }

        private void SetupDataResponsibleAssignmentClearAndAssign(DataProcessingRegistration dpr, int responsibleId = 0, OperationError? clearOperationError = null)
        {
            if (clearOperationError != null)
            {
                _dataResponsibleAssignmentServiceMock.Setup(assignmentService => assignmentService.Clear(dpr)).Returns(Result<DataProcessingDataResponsibleOption, OperationError>.Failure(clearOperationError));
            }
            else
            {
                _dataResponsibleAssignmentServiceMock.Setup(assignmentService => assignmentService.Clear(dpr)).Returns(new DataProcessingDataResponsibleOption());

            }
            _dataResponsibleAssignmentServiceMock.Setup(assignmentService => assignmentService.Assign(dpr, responsibleId)).Returns(new DataProcessingDataResponsibleOption());
        }

        private void ExpectUpdateMultiAssignmentReturns<TAssignmentInput, TAssignmentState>(DataProcessingRegistration registration, Maybe<IEnumerable<Guid>> assignmentUuids, Maybe<OperationError> result)
            where TAssignmentState : class, IHasId, IHasUuid
        {
            _assignmentUpdateServiceMock
                .Setup(x => x.UpdateUniqueMultiAssignment(
                    It.IsAny<string>(),
                    registration,
                    It.Is<Maybe<IEnumerable<Guid>>>(uuids =>
                        uuids.HasValue == assignmentUuids.HasValue &&
                        (uuids.IsNone || uuids.Value.SequenceEqual(assignmentUuids.Value))),
                    It.IsAny<Func<Guid, Result<TAssignmentInput, OperationError>>>(),
                    It.IsAny<Func<DataProcessingRegistration, IEnumerable<TAssignmentState>>>(),
                    It.IsAny<Func<DataProcessingRegistration, TAssignmentInput, Maybe<OperationError>>>(),
                    It.IsAny<Func<DataProcessingRegistration, TAssignmentState, Maybe<OperationError>>>(),
                    It.IsAny<Func<DataProcessingRegistration, TAssignmentState, Maybe<OperationError>>>()))
                .Returns(result);
        }

        private static void AssertFailureWithKnownError(Result<DataProcessingRegistration, OperationError> result, OperationError operationError, Mock<IDatabaseTransaction> transaction)
        {
            Assert.True(result.Failed);
            Assert.Equal(operationError.FailureType, result.Error.FailureType);
            AssertTransactionNotCommitted(transaction);
        }

        private static void AssertFailureWithUnknownError(Result<DataProcessingRegistration, OperationError> result, OperationError operationError, Mock<IDatabaseTransaction> transaction)
        {
            Assert.True(result.Failed);
            AssertTransactionNotCommitted(transaction);
        }

        private void SetupUpdateSuccess(DataProcessingRegistration dataProcessingRegistration)
        {
            AllowReadsReturns();
            SetupGetFromRepository(dataProcessingRegistration);
        }

        private void SetupGetFromRepository(DataProcessingRegistration dataProcessingRegistration)
        {
            var dprsQueryable = new List<DataProcessingRegistration>() { dataProcessingRegistration }.AsQueryable();
            _repositoryMock.Setup(repository => repository.AsQueryable())
                .Returns(dprsQueryable);
            _repositoryMock.Setup(repository => repository.GetById(It.IsAny<int>())).Returns(dataProcessingRegistration);
        }


        private static void AssertTransactionNotCommitted(Mock<IDatabaseTransaction> transactionMock)
        {
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        private void AssertTransactionCommitted(Mock<IDatabaseTransaction> transactionMock)
        {
            _databaseControlMock.Verify(x => x.SaveChanges(), Times.AtLeastOnce);
            transactionMock.Verify(x => x.Commit());
        }

        private Mock<IDatabaseTransaction> ExpectTransaction()
        {
            var transactionMock = new Mock<IDatabaseTransaction>();
            _transactionManagerMock.Setup(x => x.Begin()).Returns(transactionMock.Object);
            return transactionMock;
        }

        private (Guid organizationUuid, DataProcessingRegistrationCreationParameters parameters,
            DataProcessingRegistration createdRegistration, Mock<IDatabaseTransaction> transaction)
            SetupCreateScenarioPrerequisites(
                UpdatedDataProcessingRegistrationGeneralDataParameters? generalData = null,
                IEnumerable<Guid>? systemUsageUuids = null,
                UpdatedDataProcessingRegistrationOversightDataParameters? oversightData = null,
                UpdatedDataProcessingRegistrationRoles? roles = null,
                IEnumerable<UpdatedExternalReferenceProperties>? externalReferences = null)
        {
            return SetupScenarioPrerequisites<DataProcessingRegistrationCreationParameters>(generalData,
                systemUsageUuids, oversightData, roles, externalReferences);
        }

        private (Guid organizationUuid, DataProcessingRegistrationModificationParameters parameters,
            DataProcessingRegistration createdRegistration, Mock<IDatabaseTransaction> transaction)
            SetupModifyScenarioPrerequisites(
                UpdatedDataProcessingRegistrationGeneralDataParameters? generalData = null,
                IEnumerable<Guid>? systemUsageUuids = null,
                UpdatedDataProcessingRegistrationOversightDataParameters? oversightData = null,
                UpdatedDataProcessingRegistrationRoles? roles = null,
                IEnumerable<UpdatedExternalReferenceProperties>? externalReferences = null)
        {
            return SetupScenarioPrerequisites<DataProcessingRegistrationModificationParameters>(generalData,
                systemUsageUuids, oversightData, roles, externalReferences);
        }

        private (Guid organizationUuid, TReturnParameters parameters,
            DataProcessingRegistration createdRegistration, Mock<IDatabaseTransaction> transaction)
            SetupScenarioPrerequisites<TReturnParameters>(
                UpdatedDataProcessingRegistrationGeneralDataParameters? generalData = null,
                IEnumerable<Guid>? systemUsageUuids = null,
                UpdatedDataProcessingRegistrationOversightDataParameters? oversightData = null,
                UpdatedDataProcessingRegistrationRoles? roles = null,
                IEnumerable<UpdatedExternalReferenceProperties>? externalReferences = null) where TReturnParameters : BaseDataProcessingRegistrationParameters, new()
        {
            var organizationUuid = A<Guid>();
            var organization = new Organization() { Uuid = organizationUuid };
            var parameters = new TReturnParameters
            {
                Name = A<string>().AsChangedValue(),
                General = generalData.FromNullable(),
                SystemUsageUuids = systemUsageUuids.FromNullable(),
                Oversight = oversightData.FromNullable(),
                Roles = roles.FromNullable(),
                ExternalReferences = externalReferences.FromNullable()
            };
            var createdRegistration = new DataProcessingRegistration
            {
                Id = A<int>(),
                Uuid = A<Guid>(),
                AssociatedContracts = new List<ItContract> { new() { Id = A<int>() } },
                Organization = organization,
            };
            var transaction = ExpectTransaction();
            var orgDbId = A<int>();

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<Organization>(organizationUuid, orgDbId);
            _namingServiceMock.Setup(service => service.ValidateSuggestedNewRegistrationName(It.IsAny<int>(), It.IsAny<string>())).Returns(Maybe<OperationError>.None);
            _repositoryMock.Setup(repository => repository.Add(It.IsAny<DataProcessingRegistration>())).Returns(createdRegistration);
            AllowCreateReturns();

            return (organizationUuid, parameters, createdRegistration, transaction);
        }

        private void AllowCreateReturns(bool value = true)
        {
            _authorizationContextMock.Setup(context => context.AllowCreate<DataProcessingRegistration>(It.IsAny<int>())).Returns(value);
        }

        private void AllowReadsReturns(bool value = true)
        {
            _authorizationContextMock.Setup(context => context.AllowReads(It.IsAny<DataProcessingRegistration>())).Returns(value);
        }

        private void ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<T>(Guid? uuid, Maybe<int> dbId) where T : class, IHasUuid, IHasId
        {
            if (uuid.HasValue)
                _identityResolverMock.Setup(x => x.ResolveDbId<T>(uuid.Value)).Returns(dbId);
        }

        private void AssertFailureWithKnownErrorDetails(Result<DataProcessingRegistration, OperationError> result, string errorMessageContent, OperationFailure failure, Mock<IDatabaseTransaction> transaction)
        {
            Assert.True(result.Failed);
            Assert.Contains(errorMessageContent, result.Error.Message.GetValueOrEmptyString());
            Assert.Equal(failure, result.Error.FailureType);
            AssertTransactionNotCommitted(transaction);
        }
    }
}
