using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Contract;
using Core.ApplicationServices.Contract.Write;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.GDPR;
using Core.ApplicationServices.Generic;
using Core.ApplicationServices.Generic.Write;
using Core.ApplicationServices.Model.Contracts.Write;
using Core.ApplicationServices.Model.Shared.Write;
using Core.ApplicationServices.OptionTypes;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.References;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainModel.References;
using Core.DomainModel.Shared;
using Core.DomainServices;
using Core.DomainServices.Generic;
using Core.DomainServices.Role;
using Infrastructure.Services.DataAccess;
using Moq;
using Tests.Toolkit.Extensions;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.Contract
{
    public class ItContractWriteServiceTest : WithAutoFixture
    {
        private readonly ItContractWriteService _sut;
        private readonly Mock<IItContractService> _itContractServiceMock;
        private readonly Mock<IEntityIdentityResolver> _identityResolverMock;
        private readonly Mock<IOptionResolver> _optionResolverMock;
        private readonly Mock<ITransactionManager> _transactionManagerMock;
        private readonly Mock<IDomainEvents> _domainEventsMock;
        private readonly Mock<IDatabaseControl> _databaseControlMock;
        private readonly Mock<IGenericRepository<ItContractAgreementElementTypes>> _agreementElementTypeRepository;
        private readonly Mock<IOrganizationService> _organizationServiceMock;
        private readonly Mock<IReferenceService> _referenceServiceMock;
        private readonly Mock<IAuthorizationContext> _authContext;
        private readonly Mock<IAssignmentUpdateService> _assignmentUpdateServiceMock;
        private readonly Mock<IItSystemUsageService> _usageServiceMock;
        private readonly Mock<IRoleAssignmentService<ItContractRight, ItContractRole, ItContract>> _roleAssignmentService;
        private readonly Mock<IDataProcessingRegistrationApplicationService> _dprServiceMock;
        private readonly Mock<IEntityTreeUuidCollector> _entityTreeUuidCollector;

        public ItContractWriteServiceTest()
        {
            _itContractServiceMock = new Mock<IItContractService>();
            _identityResolverMock = new Mock<IEntityIdentityResolver>();
            _optionResolverMock = new Mock<IOptionResolver>();
            _transactionManagerMock = new Mock<ITransactionManager>();
            _domainEventsMock = new Mock<IDomainEvents>();
            _databaseControlMock = new Mock<IDatabaseControl>();
            _agreementElementTypeRepository = new Mock<IGenericRepository<ItContractAgreementElementTypes>>();
            _organizationServiceMock = new Mock<IOrganizationService>();
            _referenceServiceMock = new Mock<IReferenceService>();
            _authContext = new Mock<IAuthorizationContext>();
            _assignmentUpdateServiceMock = new Mock<IAssignmentUpdateService>();
            _usageServiceMock = new Mock<IItSystemUsageService>();
            _roleAssignmentService = new Mock<IRoleAssignmentService<ItContractRight, ItContractRole, ItContract>>();
            _dprServiceMock = new Mock<IDataProcessingRegistrationApplicationService>();
            _entityTreeUuidCollector = new Mock<IEntityTreeUuidCollector>();
            _sut = new ItContractWriteService(_itContractServiceMock.Object,
                _identityResolverMock.Object,
                _optionResolverMock.Object,
                _transactionManagerMock.Object,
                _domainEventsMock.Object,
                _databaseControlMock.Object,
                _agreementElementTypeRepository.Object,
                _authContext.Object,
                _organizationServiceMock.Object,
                _referenceServiceMock.Object,
                _assignmentUpdateServiceMock.Object,
                _usageServiceMock.Object,
                _roleAssignmentService.Object,
                _dprServiceMock.Object,
                Mock.Of<IGenericRepository<EconomyStream>>(),
                _entityTreeUuidCollector.Object);
        }

        protected override void OnFixtureCreated(Fixture fixture)
        {
            base.OnFixtureCreated(fixture);
            var outerFixture = new Fixture();

            //Ensure operation errors are always auto-created WITH both failure and message
            fixture.Register(() => new OperationError(outerFixture.Create<string>(), outerFixture.Create<OperationFailure>()));
        }

        [Fact]
        public void Can_Delete()
        {
            //Arrange
            var contractId = A<Guid>();
            var contractDbId = A<int>();

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<ItContract>(contractId, contractDbId);
            _itContractServiceMock.Setup(x => x.Delete(contractDbId)).Returns(new ItContract());

            //Act
            var error = _sut.Delete(contractId);

            //Assert
            Assert.True(error.IsNone);
        }

        [Fact]
        public void Cannot_Delete_If_Delete_Fails_In_Service()
        {
            //Arrange
            var contractId = A<Guid>();
            var contractDbId = A<int>();
            var operationFailure = A<OperationFailure>();

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<ItContract>(contractId, contractDbId);
            _itContractServiceMock.Setup(x => x.Delete(contractDbId)).Returns(operationFailure);

            //Act
            var error = _sut.Delete(contractId);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(operationFailure, error.Value.FailureType);
            Assert.Equal("Failed deleting contract", error.Value.Message.Value);
        }

        [Fact]
        public void Cannot_Delete_If_DbId_Resolution_Fails()
        {
            //Arrange
            var contractId = A<Guid>();

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<ItContract>(contractId, Maybe<int>.None);

            //Act
            var error = _sut.Delete(contractId);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.NotFound, error.Value.FailureType);
            Assert.Equal("Invalid contract uuid", error.Value.Message.Value);
        }

        [Fact]
        public void Can_Delete_With_Children()
        {
            //Arrange
            var parentUuid = A<Guid>();
            var childUuid = A<Guid>();
            var parentId = A<int>();
            var childId = A<int>();

            var child = new ItContract { Id = childId, Uuid = childUuid, ParentId = parentId};
            var parent = new ItContract { Id = parentId, Uuid = parentUuid, Children = new List<ItContract>{child}};
            child.Parent = parent;

            ExpectGetReturns(parentUuid, parent);
            ExpectGetReturns(childUuid, child);
            _itContractServiceMock.Setup(x => x.Delete(parentId)).Returns(new ItContract());
            _itContractServiceMock.Setup(x => x.Delete(childId)).Returns(new ItContract());

            ExpectTransaction();

            //Act
            var error = _sut.DeleteContractWithChildren(parentUuid);

            //Assert
            Assert.True(error.IsNone);
        }

        [Fact]
        public void Can_Transfer_Multiple()
        {
            //Arrange
            var contractUuid = A<Guid>();
            var contractUuid2 = A<Guid>();

            var request = new List<Guid> { contractUuid, contractUuid2 };
            var (_, _, createdContract, transaction) = SetupCreateScenarioPrerequisites();
            var contract1 = new ItContract{Organization = createdContract.Organization, OrganizationId = createdContract.OrganizationId};
            var contract2 = new ItContract{ Organization = createdContract.Organization, OrganizationId = createdContract.OrganizationId };

            ExpectGetReturns(createdContract.Uuid, createdContract);
            ExpectGetReturns(contractUuid, contract1);
            ExpectGetReturns(contractUuid2, contract2);
            ExpectAllowModifySuccess(createdContract);
            ExpectAllowModifySuccess(contract1);
            ExpectAllowModifySuccess(contract2);
            _entityTreeUuidCollector.Setup(x => x.CollectSelfAndDescendantUuids(contract1)).Returns(new List<Guid?>());
            _entityTreeUuidCollector.Setup(x => x.CollectSelfAndDescendantUuids(contract2)).Returns(new List<Guid?>());

            //Act
            var result = _sut.TransferContracts(createdContract.Uuid, request);

            //Assert
            Assert.True(result.IsNone);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Can_Create_With_Name()
        {
            //Arrange
            var name = A<string>();
            var parameters = new ItContractModificationParameters() { Name = name.AsChangedValue() };
            var transaction = ExpectTransaction();
            var organizationUuid = A<Guid>();
            var itContract = new ItContract() { Id = A<int>(), Uuid = A<Guid>() };
            var organizationId = A<int>();
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<Organization>(organizationUuid, organizationId);
            ExpectCreateReturns(organizationId, name, itContract);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(itContract, result.Value);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Cannot_Create_If_Creation_Fails()
        {
            //Arrange
            var name = A<string>();
            var parameters = new ItContractModificationParameters() { Name = name.AsChangedValue() };
            var transaction = ExpectTransaction();
            var organizationUuid = A<Guid>();
            var organizationId = A<int>();
            var operationError = A<OperationError>();
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<Organization>(organizationUuid, organizationId);
            ExpectCreateReturns(organizationId, name, operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownError(result, operationError, transaction);
        }

        [Fact]
        public void Cannot_Create_If_OrganizationId_Resolution_Fails()
        {
            //Arrange
            var name = A<string>();
            var parameters = new ItContractModificationParameters() { Name = name.AsChangedValue() };
            var transaction = ExpectTransaction();
            var organizationUuid = A<Guid>();
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<Organization>(organizationUuid, Maybe<int>.None);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(result, "Organization id not valid", OperationFailure.BadInput, transaction);
        }

        [Fact]
        public void Cannot_Create_Without_Defined_Name()
        {
            //Arrange
            var parameters = new ItContractModificationParameters();
            var transaction = ExpectTransaction();
            var organizationUuid = A<Guid>();

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(result, "Name must be provided", OperationFailure.BadInput, transaction);
        }

        [Fact]
        public void Can_Create_With_Parent()
        {
            //Arrange
            var parentUuid = A<Guid>();
            var (organizationUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(parentUuid: parentUuid);
            var parent = new ItContract() { Id = A<int>(), Uuid = parentUuid, OrganizationId = createdContract.OrganizationId };
            ExpectGetReturns(parent.Uuid, parent);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(parent, result.Value.Parent);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Can_Create_With_No_Parent()
        {
            //Arrange
            var (organizationUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(parentUuid: null);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Null(result.Value.Parent);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Cannot_Create_With_Parent_If_Get_Fails()
        {
            //Arrange
            var parent = new ItContract() { Id = A<int>(), Uuid = A<Guid>() };
            var (organizationUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(parentUuid: parent.Uuid);
            var operationError = A<OperationError>();
            ExpectGetReturns(parent.Uuid, operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(result, "Failed to get contract with Uuid:", operationError.FailureType, transaction);
        }

        [Fact]
        public void Cannot_Create_With_Parent_If_Different_Organizations()
        {
            //Arrange
            var parent = new ItContract() { Id = A<int>(), Uuid = A<Guid>(), OrganizationId = A<int>() };
            var (organizationUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(parentUuid: parent.Uuid);
            ExpectGetReturns(parent.Uuid, parent);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(result, "Failed to set parent with Uuid:", OperationFailure.BadInput, transaction);
        }

        [Theory]
        [InlineData(true, true, true, true, true, true)]
        [InlineData(true, true, true, true, true, false)]
        [InlineData(true, true, true, true, false, true)]
        [InlineData(true, true, true, false, true, true)]
        [InlineData(true, true, false, true, true, true)]
        [InlineData(true, false, true, true, true, true)]
        [InlineData(false, true, true, true, true, true)]
        [InlineData(false, false, false, false, false, false)]
        public void Can_Create_With_GeneralData(bool withContractType, bool withContractTemplate, bool withAgreementElements, bool withValidFrom, bool withValidTo, bool withCriticalityType)
        {
            // Arrange
            var (organizationUuid, itContractModificationParameters, createdContract, transaction) = SetupCreateScenarioPrerequisites();

            var (contractId,
                contractTypeUuid,
                contractTemplateUuid,
                enforceValid,
                validFrom,
                validTo,
                criticalityTypeUuid,
                agreementElementUuids,
                agreementElementTypes,
                parameters) = SetupGeneralSectionInput(withContractType, withContractTemplate, withAgreementElements, withValidFrom, withValidTo, withCriticalityType, createdContract, organizationUuid);

            itContractModificationParameters.General = parameters;


            // Act
            var result = _sut.Create(organizationUuid, itContractModificationParameters);

            // Assert
            Assert.True(result.Ok);
            AssertTransactionCommitted(transaction);
            var contract = result.Value;
            AssertGeneralSection(contractId, validFrom, validTo, enforceValid, agreementElementTypes, agreementElementUuids, contract, false);
        }

        [Fact]
        public void Cannot_Create_With_GeneralData_With_Duplicate_AgreementElements()
        {
            // Arrange
            var (organizationUuid, itContractModificationParameters, createdContract, transaction) = SetupCreateScenarioPrerequisites();

            var agreementElementUuids = Many<Guid>().ToList();
            var parameters = new ItContractGeneralDataModificationParameters
            {
                //One overlap
                AgreementElementUuids = agreementElementUuids.Append(agreementElementUuids.RandomItem()).AsChangedValue()
            };

            itContractModificationParameters.General = parameters;

            var agreementElementTypes = agreementElementUuids.ToDictionary(uuid => uuid, uuid => new AgreementElementType() { Id = A<int>(), Uuid = uuid });

            foreach (var agreementElementType in agreementElementTypes)
                ExpectGetOptionTypeReturnsIfInputIdIsDefined<AgreementElementType>(organizationUuid, agreementElementType.Key, (agreementElementType.Value, true));


            // Act
            var result = _sut.Create(organizationUuid, itContractModificationParameters);

            // Assert
            AssertFailureWithKnownErrorDetails(result, "agreement elements must not contain duplicates", OperationFailure.BadInput, transaction);
        }

        [Fact]
        public void Cannot_Create_With_GeneralData_With_AgreementElement_Being_Unavailable()
        {
            // Arrange
            var (organizationUuid, itContractModificationParameters, createdContract, transaction) = SetupCreateScenarioPrerequisites();

            var agreementElementUuids = Many<Guid>().ToList();
            var parameters = new ItContractGeneralDataModificationParameters
            {
                AgreementElementUuids = agreementElementUuids.AsChangedValue<IEnumerable<Guid>>()
            };

            itContractModificationParameters.General = parameters;

            var agreementElementTypes = agreementElementUuids.ToDictionary(uuid => uuid, uuid => new AgreementElementType() { Id = A<int>(), Uuid = uuid });

            var unavailableUuid = agreementElementUuids.RandomItem();
            foreach (var agreementElementType in agreementElementTypes)
                ExpectGetOptionTypeReturnsIfInputIdIsDefined<AgreementElementType>(organizationUuid, agreementElementType.Key, (agreementElementType.Value, agreementElementType.Key != unavailableUuid));


            // Act
            var result = _sut.Create(organizationUuid, itContractModificationParameters);

            // Assert
            AssertFailureWithKnownErrorDetails(result, $"Tried to add agreement element which is not available in the organization: {unavailableUuid}", OperationFailure.BadInput, transaction);
        }

        [Fact]
        public void Cannot_Create_With_GeneralData_With_AgreementElement_Failing_To_Fetch()
        {
            // Arrange
            var (organizationUuid, itContractModificationParameters, createdContract, transaction) = SetupCreateScenarioPrerequisites();

            var agreementElementUuids = Many<Guid>().ToList();
            var parameters = new ItContractGeneralDataModificationParameters
            {
                AgreementElementUuids = agreementElementUuids.AsChangedValue<IEnumerable<Guid>>()
            };

            itContractModificationParameters.General = parameters;

            var agreementElementTypes = agreementElementUuids.ToDictionary(uuid => uuid, uuid => new AgreementElementType() { Id = A<int>(), Uuid = uuid });

            var failingItemUuid = agreementElementUuids.RandomItem();
            var operationError = A<OperationError>();
            foreach (var agreementElementType in agreementElementTypes)
            {
                if (agreementElementType.Key == failingItemUuid)
                {
                    ExpectGetOptionTypeReturnsIfInputIdIsDefined<AgreementElementType>(organizationUuid, agreementElementType.Key, operationError);
                }
                else
                {
                    ExpectGetOptionTypeReturnsIfInputIdIsDefined<AgreementElementType>(organizationUuid, agreementElementType.Key, (agreementElementType.Value, true));
                }
            }


            // Act
            var result = _sut.Create(organizationUuid, itContractModificationParameters);

            // Assert
            AssertFailureWithKnownErrorDetails(result, $"Failed resolving agreement element with uuid:{failingItemUuid}. Message:{operationError.Message.GetValueOrEmptyString()}", operationError.FailureType, transaction);
        }

        [Fact]
        public void Cannot_Create_With_GeneralData_If_ValidationPeriod_Is_Invalid()
        {
            // Arrange
            var (organizationUuid, itContractModificationParameters, createdContract, transaction) = SetupCreateScenarioPrerequisites();

            var validFrom = A<DateTime>().Date;
            var validTo = validFrom.Subtract(TimeSpan.FromDays(1)).Date;
            var parameters = new ItContractGeneralDataModificationParameters
            {
                ValidFrom = validFrom.FromNullable().AsChangedValue(),
                ValidTo = validTo.FromNullable().AsChangedValue(),
            };

            itContractModificationParameters.General = parameters;


            // Act
            var result = _sut.Create(organizationUuid, itContractModificationParameters);

            // Assert
            AssertFailureWithKnownErrorDetails(result, "ValidTo must equal or proceed ValidFrom", OperationFailure.BadInput, transaction);
        }

        [Fact]
        public void Cannot_Create_With_GeneralData_If_UpdateIndependentOptionTypeAssignment_For_ContractTemplate_Fails()
        {
            // Arrange
            var (organizationUuid, itContractModificationParameters, createdContract, transaction) = SetupCreateScenarioPrerequisites();

            var contractTemplateUuid = A<Guid>();
            var parameters = new ItContractGeneralDataModificationParameters
            {
                ContractTemplateUuid = ((Guid?)contractTemplateUuid).AsChangedValue()
            };
            itContractModificationParameters.General = parameters;

            var operationError = A<OperationError>();
            ExpectUpdateIndependentOptionTypeAssignmentReturns<ItContractTemplateType>(createdContract, contractTemplateUuid, operationError);

            // Act
            var result = _sut.Create(organizationUuid, itContractModificationParameters);

            // Assert
            AssertFailureWithKnownError(result, operationError, transaction);
        }

        [Fact]
        public void Cannot_Create_With_GeneralData_If_UpdateIndependentOptionTypeAssignment_For_ContractType_Fails()
        {
            // Arrange
            var (organizationUuid, itContractModificationParameters, createdContract, transaction) = SetupCreateScenarioPrerequisites();

            var contractTypeUuid = A<Guid>();
            var parameters = new ItContractGeneralDataModificationParameters
            {
                ContractTypeUuid = ((Guid?)contractTypeUuid).AsChangedValue()
            };
            itContractModificationParameters.General = parameters;

            var operationError = A<OperationError>();
            ExpectUpdateIndependentOptionTypeAssignmentReturns<ItContractType>(createdContract, contractTypeUuid, operationError);

            // Act
            var result = _sut.Create(organizationUuid, itContractModificationParameters);

            // Assert
            AssertFailureWithKnownError(result, operationError, transaction);
        }

        [Fact]
        public void Cannot_Create_With_GeneralData_If_UpdateIndependentOptionTypeAssignment_For_CriticalityType_Fails()
        {
            // Arrange
            var (organizationUuid, itContractModificationParameters, createdContract, transaction) = SetupCreateScenarioPrerequisites();

            var criticalityTypeUuid = A<Guid>();
            var parameters = new ItContractGeneralDataModificationParameters
            {
                CriticalityUuid = ((Guid?)criticalityTypeUuid).AsChangedValue()
            };
            itContractModificationParameters.General = parameters;

            var operationError = A<OperationError>();
            ExpectUpdateIndependentOptionTypeAssignmentReturns<CriticalityType>(createdContract, criticalityTypeUuid, operationError);

            // Act
            var result = _sut.Create(organizationUuid, itContractModificationParameters);

            // Assert
            AssertFailureWithKnownError(result, operationError, transaction);
        }

        [Theory]
        [InlineData(true, true, true, true)]
        [InlineData(true, false, false, false)]
        [InlineData(false, true, false, false)]
        [InlineData(false, false, true, false)]
        [InlineData(false, false, false, true)]
        [InlineData(false, false, false, false)]
        public void Can_Create_With_Procurement(bool withStrategy, bool withPurchase, bool withPlan, bool withInitiated)
        {
            //Arrange
            var (procurementStrategyUuid, purchaseTypeUuid, procurement) = CreateProcurementParameters(withStrategy, withPurchase, withPlan, withInitiated);
            var (organizationUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(procurement: procurement);
            ExpectUpdateIndependentOptionTypeAssignmentReturns<ProcurementStrategyType>(createdContract, procurementStrategyUuid, Maybe<OperationError>.None);
            ExpectUpdateIndependentOptionTypeAssignmentReturns<PurchaseFormType>(createdContract, purchaseTypeUuid, Maybe<OperationError>.None);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            AssertProcurement(procurement, result.Value);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Cannot_Create_With_Procurement_If_UpdateIndependentOptionTypeAssignment_For_ProcurementStrategy_Fails()
        {
            //Arrange
            var procurementStrategyUuid = A<Guid>();
            var procurement = new ItContractProcurementModificationParameters()
            {
                ProcurementStrategyUuid = ((Guid?)procurementStrategyUuid).AsChangedValue()
            };
            var (organizationUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(procurement: procurement);

            var operationError = A<OperationError>();
            ExpectUpdateIndependentOptionTypeAssignmentReturns<ProcurementStrategyType>(createdContract, procurementStrategyUuid, operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownError(result, operationError, transaction);
        }

        [Fact]
        public void Cannot_Create_With_Procurement_If_UpdateIndependentOptionTypeAssignment_For_PurchaseForm_Fails()
        {
            //Arrange
            var purchaseTypeUuid = A<Guid>();
            var procurement = new ItContractProcurementModificationParameters()
            {
                PurchaseTypeUuid = ((Guid?)purchaseTypeUuid).AsChangedValue()
            };
            var (organizationUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(procurement: procurement);

            var operationError = A<OperationError>();
            ExpectUpdateIndependentOptionTypeAssignmentReturns<PurchaseFormType>(createdContract, purchaseTypeUuid, operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownError(result, operationError, transaction);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(5)]
        public void Cannot_Create_With_Procurement_If_Quarter_Of_Year_Is_Other_Than_1_To_4(int halfOfYear)
        {
            //Arrange
            var procurement = new ItContractProcurementModificationParameters()
            {
                ProcurementPlan = (Convert.ToByte(halfOfYear), A<int>()).FromNullable().AsChangedValue()
            };
            var (organizationUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(procurement: procurement);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(result, "Failed to update procurement plan with error message: Quarter Of Year has to be either 1, 2, 3 or 4", OperationFailure.BadInput, transaction);
        }

        [Fact]
        public void Can_Create_With_Responsible()
        {
            //Arrange
            var responsible = new ItContractResponsibleDataModificationParameters()
            {
                OrganizationUnitUuid = ((Guid?)A<Guid>()).AsChangedValue(),
                Signed = A<bool>().AsChangedValue(),
                SignedAt = A<DateTime?>().AsChangedValue(),
                SignedBy = A<string>().AsChangedValue()
            };
            var (organizationUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(responsible: responsible);
            var correctOrganizationUnit = new OrganizationUnit() { Uuid = responsible.OrganizationUnitUuid.NewValue.GetValueOrDefault() };
            createdContract.Organization.OrgUnits.Add(new OrganizationUnit());
            createdContract.Organization.OrgUnits.Add(correctOrganizationUnit);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            var contract = result.Value;
            Assert.Equal(responsible.OrganizationUnitUuid.NewValue, contract.ResponsibleOrganizationUnit?.Uuid);
            Assert.Equal(responsible.Signed.NewValue, contract.IsSigned);
            Assert.Equal(responsible.SignedAt.NewValue?.Date, contract.SignedDate);
            Assert.Equal(responsible.SignedBy.NewValue, contract.ContractSigner);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Cannot_Create_With_Responsible_If_Responsible_Unit_Does_Not_Exist_On_Contract_Organization()
        {
            //Arrange
            var responsible = new ItContractResponsibleDataModificationParameters()
            {
                OrganizationUnitUuid = ((Guid?)A<Guid>()).AsChangedValue(),
            };
            var (organizationUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(responsible: responsible);
            var existingUnit = new OrganizationUnit() { Uuid = Guid.NewGuid() };
            createdContract.Organization.OrgUnits.Add(existingUnit);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            AssertFailureWithKnownErrorDetails(result, "UUID of responsible organization unit does not match an organization unit on this contract's organization", OperationFailure.BadInput, transaction);
        }

        [Fact]
        public void Can_Create_With_Supplier()
        {
            //Arrange
            var supplier = new ItContractSupplierModificationParameters()
            {
                OrganizationUuid = ((Guid?)A<Guid>()).AsChangedValue(),
                Signed = A<bool>().AsChangedValue(),
                SignedAt = A<DateTime?>().AsChangedValue(),
                SignedBy = A<string>().AsChangedValue()
            };
            var (organizationUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(supplier: supplier);
            var organization = new Organization() { Uuid = supplier.OrganizationUuid.NewValue.GetValueOrDefault() };
            _organizationServiceMock.Setup(x => x.GetOrganization(organization.Uuid, null)).Returns(organization);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            var contract = result.Value;
            Assert.Equal(supplier.OrganizationUuid.NewValue, contract.Supplier?.Uuid);
            Assert.Equal(supplier.Signed.NewValue, contract.HasSupplierSigned);
            Assert.Equal(supplier.SignedAt.NewValue?.Date, contract.SupplierSignedDate);
            Assert.Equal(supplier.SignedBy.NewValue, contract.SupplierContractSigner);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Cannot_Create_With_Supplier_If_Organization_Resolution_Fails()
        {
            //Arrange
            var supplier = new ItContractSupplierModificationParameters()
            {
                OrganizationUuid = ((Guid?)A<Guid>()).AsChangedValue(),
                Signed = A<bool>().AsChangedValue(),
                SignedAt = A<DateTime?>().AsChangedValue(),
                SignedBy = A<string>().AsChangedValue()
            };
            var (organizationUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(supplier: supplier);
            var operationError = A<OperationError>();
            _organizationServiceMock.Setup(x => x.GetOrganization(supplier.OrganizationUuid.NewValue.GetValueOrDefault(), null)).Returns(operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            AssertFailureWithKnownErrorDetails(result, $"Failed to get supplier organization:{operationError.Message.GetValueOrEmptyString()}", operationError.FailureType, transaction);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_With_SystemUsages(bool hasUsages)
        {
            //Arrange
            var usageUuids = hasUsages ? Many<Guid>().ToList() : new List<Guid>();
            var (organizationUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(systemUsageUuids: usageUuids);

            ExpectUpdateMultiAssignmentReturns<ItSystemUsage, ItSystemUsage>(createdContract, usageUuids, Maybe<OperationError>.None);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Cannot_Create_With_SystemUsages_If_UpdateMultiAssignment_Fails()
        {
            //Arrange
            var usageUuids = Many<Guid>().ToList();
            var (organizationUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(systemUsageUuids: usageUuids);

            var operationError = A<OperationError>();
            ExpectUpdateMultiAssignmentReturns<ItSystemUsage, ItSystemUsage>(createdContract, usageUuids, operationError);

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
            var (organizationUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(systemUsageUuids: usageUuids);
            createdContract.AssociatedSystemUsages.Add(new ItContractItSystemUsage { ItContract = createdContract, ItContractId = createdContract.Id, ItSystemUsage = usage, ItSystemUsageId = usage.Id });

            ExpectUpdateMultiAssignmentReturns<ItSystemUsage, ItSystemUsage>(createdContract, usageUuids, Maybe<OperationError>.None);
            ExpectGetReturns(createdContract.Uuid, createdContract);
            ExpectAllowModifySuccess(createdContract);
            parameters.Name = OptionalValueChange<string>.None;

            //Act
            var result = _sut.Update(createdContract.Uuid, parameters);

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
            var (organizationUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(systemUsageUuids: usageUuids);
            createdContract.AssociatedSystemUsages.Add(new ItContractItSystemUsage { ItContract = createdContract, ItContractId = createdContract.Id, ItSystemUsage = usage, ItSystemUsageId = usage.Id });

            ExpectUpdateMultiAssignmentReturns<ItSystemUsage, ItSystemUsage>(createdContract, usageUuids, Maybe<OperationError>.None);
            ExpectGetReturns(createdContract.Uuid, createdContract);
            ExpectAllowModifySuccess(createdContract);
            parameters.Name = OptionalValueChange<string>.None;

            //Act
            var result = _sut.Update(createdContract.Uuid, parameters);

            //Assert
            Assert.True(result.Ok);
            AssertTransactionCommitted(transaction);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_With_DataProcessingRegistrations(bool hasUsages)
        {
            //Arrange
            var dprUuids = hasUsages ? Many<Guid>().ToList() : new List<Guid>();
            var (organizationUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(dataProcessingRegistrationUuids: dprUuids);

            ExpectUpdateMultiAssignmentReturns<DataProcessingRegistration, DataProcessingRegistration>(createdContract, dprUuids, Maybe<OperationError>.None);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Cannot_Create_With_DataProcessingRegistrations_If_UpdateMultiAssignment_Fails()
        {
            //Arrange
            var dprUuids = Many<Guid>().ToList();
            var (organizationUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(dataProcessingRegistrationUuids: dprUuids);

            var operationError = A<OperationError>();
            ExpectUpdateMultiAssignmentReturns<DataProcessingRegistration, DataProcessingRegistration>(createdContract, dprUuids, operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            AssertFailureWithKnownError(result, operationError, transaction);
        }

        [Fact]
        public void Can_Update_With_DataProcessingRegistrations_If_Usage_Already_Assigned()
        {
            //Arrange
            var dpr = new DataProcessingRegistration() { Id = A<int>(), Uuid = A<Guid>() };
            var dprUuids = new List<Guid> { dpr.Uuid };
            var (organizationUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(dataProcessingRegistrationUuids: dprUuids);
            createdContract.DataProcessingRegistrations.Add(dpr);

            ExpectUpdateMultiAssignmentReturns<DataProcessingRegistration, DataProcessingRegistration>(createdContract, dprUuids, Maybe<OperationError>.None);
            ExpectGetReturns(createdContract.Uuid, createdContract);
            ExpectAllowModifySuccess(createdContract);
            parameters.Name = OptionalValueChange<string>.None;

            //Act
            var result = _sut.Update(createdContract.Uuid, parameters);

            //Assert
            Assert.True(result.Ok);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Can_Update_With_DataProcessingRegistrations_To_Remove_Usage()
        {
            //Arrange
            var dpr = new DataProcessingRegistration() { Id = A<int>(), Uuid = A<Guid>() };
            var dprUuids = new List<Guid>();
            var (organizationUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(dataProcessingRegistrationUuids: dprUuids);
            createdContract.DataProcessingRegistrations.Add(dpr);

            ExpectUpdateMultiAssignmentReturns<DataProcessingRegistration, DataProcessingRegistration>(createdContract, dprUuids, Maybe<OperationError>.None);
            ExpectGetReturns(createdContract.Uuid, createdContract);
            ExpectAllowModifySuccess(createdContract);
            parameters.Name = OptionalValueChange<string>.None;

            //Act
            var result = _sut.Update(createdContract.Uuid, parameters);

            //Assert
            Assert.True(result.Ok);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Can_Create_With_ExternalReferences()
        {
            //Arrange
            var externalReferences = Many<UpdatedExternalReferenceProperties>().ToList();
            var (organizationUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(externalReferences: externalReferences);

            parameters.ExternalReferences = externalReferences.FromNullable<IEnumerable<UpdatedExternalReferenceProperties>>();

            ExpectBatchUpdateExternalReferencesReturns(createdContract, externalReferences, Maybe<OperationError>.None);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Cannot_Create_With_ExternalReferences_If_BatchUpdate_Fails()
        {
            //Arrange
            var externalReferences = Many<UpdatedExternalReferenceProperties>().ToList();
            var (organizationUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(externalReferences: externalReferences);

            parameters.ExternalReferences = externalReferences.FromNullable<IEnumerable<UpdatedExternalReferenceProperties>>();

            var updateError = A<OperationError>();
            ExpectBatchUpdateExternalReferencesReturns(createdContract, externalReferences, updateError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            AssertFailureWithKnownErrorDetails(result, $"Failed to update references with error message: {updateError.Message.GetValueOrEmptyString()}", updateError.FailureType, transaction);
        }

        [Fact]
        public void Can_Create_With_Roles()
        {
            //Arrange
            var roleAssignments = Many<UserRolePair>().ToList();
            var (organizationUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(roleAssignments: roleAssignments);
            ExpectBatchUpdateRoleAssignmentsReturn(createdContract, roleAssignments, Maybe<OperationError>.None);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Cannot_Create_With_Roles_If_BatchUpdate_Fails()
        {
            //Arrange
            var roleAssignments = Many<UserRolePair>().ToList();
            var (organizationUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(roleAssignments: roleAssignments);
            var updateError = A<OperationError>();
            ExpectBatchUpdateRoleAssignmentsReturn(createdContract, roleAssignments, updateError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            AssertFailureWithKnownErrorDetails(result, $"Failed while updating role assignments:{updateError.Message.GetValueOrEmptyString()}", updateError.FailureType, transaction);
        }

        private void ExpectBatchUpdateRoleAssignmentsReturn(ItContract createdContract, List<UserRolePair> roleAssignments, Maybe<OperationError> value)
        {
            _roleAssignmentService
                .Setup(x => x.BatchUpdateRoles(createdContract, It.Is<IEnumerable<(Guid, Guid)>>(input => MatchExpectedRoleAssignments(input, roleAssignments))))
                .Returns(value);
        }

        private static bool MatchExpectedRoleAssignments(IEnumerable<(Guid, Guid)> input, List<UserRolePair> expected)
        {
            return input.SequenceEqual(expected.Select(r => (r.RoleUuid, r.UserUuid)));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_With_PaymentModel(bool withValues)
        {
            //Arrange
            var paymentModel = CreatePaymentModel(withValues);
            var (organizationUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(paymentModel: paymentModel);
            ExpectUpdateIndependentOptionTypeAssignmentReturns<PaymentFreqencyType>(createdContract, paymentModel.PaymentFrequencyUuid.NewValue, Maybe<OperationError>.None);
            ExpectUpdateIndependentOptionTypeAssignmentReturns<PaymentModelType>(createdContract, paymentModel.PaymentModelUuid.NewValue, Maybe<OperationError>.None);
            ExpectUpdateIndependentOptionTypeAssignmentReturns<PriceRegulationType>(createdContract, paymentModel.PriceRegulationUuid.NewValue, Maybe<OperationError>.None);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            AssertTransactionCommitted(transaction);
            AssertPaymentModel(paymentModel, result.Value, withValues);
        }

        [Fact]
        public void Cannot_Create_With_PaymentModel_If_UpdatePaymentFrequencyType_Fails()
        {
            //Arrange
            var paymentModel = new ItContractPaymentModelModificationParameters()
            {
                PaymentFrequencyUuid = A<Guid?>().AsChangedValue()
            };
            var (organizationUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(paymentModel: paymentModel);
            var operationError = A<OperationError>();
            ExpectUpdateIndependentOptionTypeAssignmentReturns<PaymentFreqencyType>(createdContract, paymentModel.PaymentFrequencyUuid.NewValue, operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownError(result, operationError, transaction);
        }

        [Fact]
        public void Cannot_Create_With_PaymentModel_If_UpdatePaymentModelType_Fails()
        {
            //Arrange
            var paymentModel = new ItContractPaymentModelModificationParameters()
            {
                PaymentModelUuid = A<Guid?>().AsChangedValue()
            };
            var (organizationUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(paymentModel: paymentModel);
            var operationError = A<OperationError>();
            ExpectUpdateIndependentOptionTypeAssignmentReturns<PaymentModelType>(createdContract, paymentModel.PaymentModelUuid.NewValue, operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownError(result, operationError, transaction);
        }

        [Fact]
        public void Cannot_Create_With_PaymentModel_If_UpdatePriceRegulationType_Fails()
        {
            //Arrange
            var paymentModel = new ItContractPaymentModelModificationParameters()
            {
                PriceRegulationUuid = A<Guid?>().AsChangedValue()
            };
            var (organizationUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(paymentModel: paymentModel);
            var operationError = A<OperationError>();
            ExpectUpdateIndependentOptionTypeAssignmentReturns<PriceRegulationType>(createdContract, paymentModel.PriceRegulationUuid.NewValue, operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownError(result, operationError, transaction);
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, true, false)]
        [InlineData(true, false, true)]
        [InlineData(false, true, true)]
        [InlineData(false, false, false)]
        public void Can_Create_With_AgreementPeriod(bool withExtensionOption, bool continuous, bool hasIrrevocableDate)
        {
            //Arrange
            var (organizationUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites();
            var agreementPeriodInput = new ItContractAgreementPeriodModificationParameters
            {
                DurationYears = (continuous ? (int?)null : Math.Abs(A<int>())).AsChangedValue(),
                DurationMonths = (continuous ? (int?)null : Math.Abs(A<int>()) % 12).AsChangedValue(),
                IsContinuous = continuous.AsChangedValue(),
                ExtensionOptionsUuid = (withExtensionOption ? A<Guid>() : (Guid?)null).AsChangedValue(),
                ExtensionOptionsUsed = Math.Abs(A<int>()).AsChangedValue(),
                IrrevocableUntil = (hasIrrevocableDate ? A<DateTime>() : (DateTime?)null).AsChangedValue()
            };
            parameters.AgreementPeriod = agreementPeriodInput;

            ExpectUpdateIndependentOptionTypeAssignmentReturns<OptionExtendType>(createdContract, agreementPeriodInput.ExtensionOptionsUuid.NewValue, Maybe<OperationError>.None);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertTransactionCommitted(transaction);
            Assert.Equal(agreementPeriodInput.DurationMonths.NewValue, result.Value.DurationMonths);
            Assert.Equal(agreementPeriodInput.DurationYears.NewValue, result.Value.DurationYears);
            Assert.Equal(agreementPeriodInput.IsContinuous.NewValue, result.Value.DurationOngoing);
            Assert.Equal(agreementPeriodInput.ExtensionOptionsUsed.NewValue, result.Value.ExtendMultiplier);
            Assert.Equal(agreementPeriodInput.IrrevocableUntil.NewValue, result.Value.IrrevocableTo);
        }

        [Fact]
        public void Cannot_Create_With_AgreementPeriod_If_OptionExtend_Assignment_Fails()
        {
            //Arrange
            var (organizationUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites();
            var agreementPeriodInput = new ItContractAgreementPeriodModificationParameters
            {
                ExtensionOptionsUuid = ((Guid?)A<Guid>()).AsChangedValue()
            };
            parameters.AgreementPeriod = agreementPeriodInput;

            var operationError = A<OperationError>();
            ExpectUpdateIndependentOptionTypeAssignmentReturns<OptionExtendType>(createdContract, agreementPeriodInput.ExtensionOptionsUuid.NewValue, operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(result, operationError.Message.GetValueOrEmptyString(), operationError.FailureType, transaction);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void Cannot_Create_With_AgreementPeriod_If_ContinuousDuration_And_Fixed_Interval_Is_Also_Provided(bool hasDurationYear, bool hasDurationMonth)
        {
            //Arrange
            var (organizationUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites();
            var agreementPeriodInput = new ItContractAgreementPeriodModificationParameters
            {
                DurationYears = (!hasDurationYear ? (int?)null : Math.Abs(A<int>())).AsChangedValue(),
                DurationMonths = (!hasDurationMonth ? (int?)null : Math.Abs(A<int>()) % 12).AsChangedValue(),
                IsContinuous = true.AsChangedValue()
            };
            parameters.AgreementPeriod = agreementPeriodInput;

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(result, "If duration is ongoing then durationMonths and durationYears must be null", OperationFailure.BadInput, transaction);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(12)]
        public void Cannot_Create_With_AgreementPeriod_If_DurationMonth_Is_Invalid(int durationMonth)
        {
            //Arrange
            var (organizationUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites();
            var agreementPeriodInput = new ItContractAgreementPeriodModificationParameters
            {
                DurationMonths = ((int?)durationMonth).AsChangedValue()
            };
            parameters.AgreementPeriod = agreementPeriodInput;

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(result, "durationMonths cannot be below 0 or above 11", OperationFailure.BadInput, transaction);
        }

        [Fact]
        public void Cannot_Create_With_AgreementPeriod_If_DurationYear_Is_Invalid()
        {
            //Arrange
            var (organizationUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites();
            var agreementPeriodInput = new ItContractAgreementPeriodModificationParameters
            {
                DurationYears = ((int?)-1).AsChangedValue()
            };
            parameters.AgreementPeriod = agreementPeriodInput;

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(result, "durationYears cannot be below 0", OperationFailure.BadInput, transaction);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        public void Can_Create_With_Termination(bool withNoticePeriod, bool withOptionalData)
        {
            //Arrange
            var (organizationUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites();
            var terminationInput = new ItContractTerminationParameters()
            {
                TerminatedAt = (withOptionalData ? A<DateTime>().FromNullable() : Maybe<DateTime>.None).AsChangedValue(),
                NoticePeriodMonthsUuid = (withNoticePeriod ? A<Guid>() : (Guid?)null).AsChangedValue(),
                NoticePeriodExtendsCurrent = (withOptionalData ? A<YearSegmentOption>().FromNullable() : Maybe<YearSegmentOption>.None).AsChangedValue(),
                NoticeByEndOf = (withOptionalData ? A<YearSegmentOption>().FromNullable() : Maybe<YearSegmentOption>.None).AsChangedValue()
            };
            parameters.Termination = terminationInput;

            ExpectUpdateIndependentOptionTypeAssignmentReturns<TerminationDeadlineType>(createdContract, terminationInput.NoticePeriodMonthsUuid.NewValue, Maybe<OperationError>.None);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertTransactionCommitted(transaction);
            Assert.Equal(terminationInput.TerminatedAt.NewValue.Match(val => val, () => (DateTime?)null), result.Value.Terminated);
            Assert.Equal(terminationInput.NoticePeriodExtendsCurrent.NewValue.Match(val => val, () => (YearSegmentOption?)null), result.Value.Running);
            Assert.Equal(terminationInput.NoticeByEndOf.NewValue.Match(val => val, () => (YearSegmentOption?)null), result.Value.ByEnding);
        }

        [Fact]
        public void Cannot_Create_With_Termination_If_NoticePeriodMonths_Assignment_Fails()
        {
            //Arrange
            var (organizationUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites();
            var terminationInput = new ItContractTerminationParameters
            {
                NoticePeriodMonthsUuid = ((Guid?)A<Guid>()).AsChangedValue()
            };
            parameters.Termination = terminationInput;

            var operationError = A<OperationError>();
            ExpectUpdateIndependentOptionTypeAssignmentReturns<TerminationDeadlineType>(createdContract, terminationInput.NoticePeriodMonthsUuid.NewValue, operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(result, operationError.Message.GetValueOrEmptyString(), operationError.FailureType, transaction);
        }

        [Fact]
        public void Can_Create_With_Payments()
        {
            //Arrange
            Configure(f => f.Register<Guid?>(() => Guid.NewGuid()));
            var paymentParameters = new ItContractPaymentDataModificationParameters()
            {
                ExternalPayments = Many<ItContractPayment>().ToList().AsChangedValue<IEnumerable<ItContractPayment>>(),
                InternalPayments = Many<ItContractPayment>().ToList().AsChangedValue<IEnumerable<ItContractPayment>>(),
            };
            var (organizationUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(payments: paymentParameters);

            var organizationUnits = paymentParameters
                .InternalPayments
                .NewValue.Concat(paymentParameters.ExternalPayments.NewValue)
                .Select(x => x.OrganizationUnitUuid.GetValueOrDefault()).Distinct()
                .ToDictionary(uuid => uuid, uuid => new OrganizationUnit() { Uuid = uuid });

            createdContract.Organization.OrgUnits = organizationUnits.Values.ToList();

            //add some existing streams to ensure they are removed during the update
            createdContract.ExternEconomyStreams.Add(new EconomyStream());
            createdContract.InternEconomyStreams.Add(new EconomyStream());

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            AssertPayments(paymentParameters, result.Value);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Cannot_Create_With_Payments_If_OrganizationUnit_Not_In_Organization_Of_External_Payment()
        {
            //Arrange
            Configure(f => f.Register<Guid?>(() => Guid.NewGuid()));
            var paymentParameters = new ItContractPaymentDataModificationParameters()
            {
                ExternalPayments = Many<ItContractPayment>().ToList().AsChangedValue<IEnumerable<ItContractPayment>>(),
            };
            var (organizationUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(payments: paymentParameters);

            var organizationUnits = paymentParameters.ExternalPayments.NewValue
                .Select(x => x.OrganizationUnitUuid.GetValueOrDefault()).Distinct()
                .ToDictionary(uuid => uuid, uuid => new OrganizationUnit() { Uuid = uuid });

            var invalidOrgUnit = organizationUnits.Values.RandomItem();
            createdContract.Organization.OrgUnits = organizationUnits.Values.Except(invalidOrgUnit.WrapAsEnumerable()).ToList();

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(result, $"Failed to add external payment:Organization unit with uuid:{invalidOrgUnit.Uuid} is not part of the contract's organization", OperationFailure.BadInput, transaction);
        }

        [Fact]
        public void Cannot_Create_With_Payments_If_OrganizationUnit_Not_In_Organization_Of_Internal_Payment()
        {
            //Arrange
            Configure(f => f.Register<Guid?>(() => Guid.NewGuid()));
            var paymentParameters = new ItContractPaymentDataModificationParameters()
            {
                InternalPayments = Many<ItContractPayment>().ToList().AsChangedValue<IEnumerable<ItContractPayment>>(),
            };
            var (organizationUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(payments: paymentParameters);

            var organizationUnits = paymentParameters.InternalPayments.NewValue
                .Select(x => x.OrganizationUnitUuid.GetValueOrDefault()).Distinct()
                .ToDictionary(uuid => uuid, uuid => new OrganizationUnit() { Uuid = uuid });

            var invalidOrgUnit = organizationUnits.Values.RandomItem();
            createdContract.Organization.OrgUnits = organizationUnits.Values.Except(invalidOrgUnit.WrapAsEnumerable()).ToList();

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(result, $"Failed to add internal payment:Organization unit with uuid:{invalidOrgUnit.Uuid} is not part of the contract's organization", OperationFailure.BadInput, transaction);
        }

        [Fact]
        public void Can_Update_With_All()
        {
            //Arrange
            Configure(f => f.Register<Guid?>(() => Guid.NewGuid()));
            Configure(f => f.Register<bool>(() => false));

            var (organizationUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(
                parentUuid: A<Guid>(),
                responsible: A<ItContractResponsibleDataModificationParameters>(),
                supplier: A<ItContractSupplierModificationParameters>(),
                systemUsageUuids: Many<Guid>().ToList(),
                dataProcessingRegistrationUuids: Many<Guid>().ToList(),
                roleAssignments: Many<UserRolePair>().ToList(),
                payments: A<ItContractPaymentDataModificationParameters>());

            //Parent setup
            var parent = new ItContract() { Id = A<int>(), Uuid = parameters.ParentContractUuid.NewValue.Value, OrganizationId = createdContract.OrganizationId };
            ExpectGetReturns(parent.Uuid, parent);

            //General setup
            var (contractId,
                contractTypeUuid,
                contractTemplateUuid,
                enforceValid,
                validFrom,
                validTo,
                criticalityType,
                agreementElementUuids,
                agreementElementTypes,
                generalData) = SetupGeneralSectionInput(true, true, true, true, true, true, createdContract, organizationUuid);

            parameters.General = generalData;

            //Procurement setup
            var (procurementStrategyUuid, purchaseTypeUuid, procurement) = CreateProcurementParameters(true, true, true, true);
            parameters.Procurement = procurement;
            ExpectUpdateIndependentOptionTypeAssignmentReturns<ProcurementStrategyType>(createdContract, procurement.ProcurementStrategyUuid.NewValue, Maybe<OperationError>.None);
            ExpectUpdateIndependentOptionTypeAssignmentReturns<PurchaseFormType>(createdContract, procurement.PurchaseTypeUuid.NewValue, Maybe<OperationError>.None);

            //Responsible setup
            var correctOrganizationUnit = new OrganizationUnit() { Uuid = parameters.Responsible.Value.OrganizationUnitUuid.NewValue.GetValueOrDefault() };
            createdContract.Organization.OrgUnits.Add(correctOrganizationUnit);

            //Supplier
            var organization = new Organization() { Uuid = parameters.Supplier.Value.OrganizationUuid.NewValue.GetValueOrDefault() };
            _organizationServiceMock.Setup(x => x.GetOrganization(organization.Uuid, null)).Returns(organization);

            //System usage setup
            ExpectUpdateMultiAssignmentReturns<ItSystemUsage, ItSystemUsage>(createdContract, parameters.SystemUsageUuids, Maybe<OperationError>.None);

            //Data processing registration setup
            ExpectUpdateMultiAssignmentReturns<DataProcessingRegistration, DataProcessingRegistration>(createdContract, parameters.DataProcessingRegistrationUuids, Maybe<OperationError>.None);

            //External references setup
            var externalReferences = Many<UpdatedExternalReferenceProperties>().ToList();
            parameters.ExternalReferences = externalReferences.FromNullable<IEnumerable<UpdatedExternalReferenceProperties>>();
            ExpectBatchUpdateExternalReferencesReturns(createdContract, externalReferences, Maybe<OperationError>.None);

            //Roles setup
            ExpectBatchUpdateRoleAssignmentsReturn(createdContract, parameters.Roles.Value.ToList(), Maybe<OperationError>.None);

            //Payment model setup
            var paymentModel = CreatePaymentModel(true);
            parameters.PaymentModel = paymentModel;
            ExpectUpdateIndependentOptionTypeAssignmentReturns<PaymentFreqencyType>(createdContract, paymentModel.PaymentFrequencyUuid.NewValue, Maybe<OperationError>.None);
            ExpectUpdateIndependentOptionTypeAssignmentReturns<PaymentModelType>(createdContract, paymentModel.PaymentModelUuid.NewValue, Maybe<OperationError>.None);
            ExpectUpdateIndependentOptionTypeAssignmentReturns<PriceRegulationType>(createdContract, paymentModel.PriceRegulationUuid.NewValue, Maybe<OperationError>.None);

            //Agreement period setup
            var agreementPeriodInput = new ItContractAgreementPeriodModificationParameters
            {
                DurationYears = ((int?)null).AsChangedValue(),
                DurationMonths = ((int?)null).AsChangedValue(),
                IsContinuous = true.AsChangedValue(),
                ExtensionOptionsUuid = A<Guid?>().AsChangedValue(),
                ExtensionOptionsUsed = Math.Abs(A<int>()).AsChangedValue(),
                IrrevocableUntil = A<DateTime?>().AsChangedValue()
            };
            parameters.AgreementPeriod = agreementPeriodInput;
            ExpectUpdateIndependentOptionTypeAssignmentReturns<OptionExtendType>(createdContract, agreementPeriodInput.ExtensionOptionsUuid.NewValue, Maybe<OperationError>.None);

            //Termination setup
            var terminationInput = new ItContractTerminationParameters()
            {
                TerminatedAt = A<DateTime>().FromNullable().AsChangedValue(),
                NoticePeriodMonthsUuid = A<Guid?>().AsChangedValue(),
                NoticePeriodExtendsCurrent = A<YearSegmentOption>().FromNullable().AsChangedValue(),
                NoticeByEndOf = A<YearSegmentOption>().FromNullable().AsChangedValue()
            };
            parameters.Termination = terminationInput;
            ExpectUpdateIndependentOptionTypeAssignmentReturns<TerminationDeadlineType>(createdContract, terminationInput.NoticePeriodMonthsUuid.NewValue, Maybe<OperationError>.None);

            //Payments setup
            var organizationUnits = parameters
                .Payments
                .Value
                .InternalPayments
                .NewValue.Concat(parameters.Payments.Value.ExternalPayments.NewValue)
                .Select(x => x.OrganizationUnitUuid.GetValueOrDefault()).Distinct()
                .ToDictionary(uuid => uuid, uuid => new OrganizationUnit() { Uuid = uuid });

            foreach (var orgUnit in organizationUnits.Values.ToList())
            {
                createdContract.Organization.OrgUnits.Add(orgUnit);
            }

            //Update setup
            ExpectGetReturns(createdContract.Uuid, createdContract);
            ExpectAllowModifySuccess(createdContract);
            parameters.Name = OptionalValueChange<string>.None;

            //Act
            var result = _sut.Update(createdContract.Uuid, parameters);

            //Assert
            Assert.True(result.Ok);
            AssertTransactionCommitted(transaction);
            var contract = result.Value;

            Assert.Equal(parent, contract.Parent);

            AssertGeneralSection(contractId, validFrom, validTo, enforceValid, agreementElementTypes, agreementElementUuids, contract, false);

            AssertProcurement(parameters.Procurement.Value, contract);

            //Responsible
            Assert.Equal(parameters.Responsible.Value.OrganizationUnitUuid.NewValue, contract.ResponsibleOrganizationUnit?.Uuid);
            Assert.Equal(parameters.Responsible.Value.Signed.NewValue, contract.IsSigned);
            Assert.Equal(parameters.Responsible.Value.SignedAt.NewValue?.Date, contract.SignedDate);
            Assert.Equal(parameters.Responsible.Value.SignedBy.NewValue, contract.ContractSigner);

            //Supplier
            Assert.Equal(parameters.Supplier.Value.OrganizationUuid.NewValue, contract.Supplier?.Uuid);
            Assert.Equal(parameters.Supplier.Value.Signed.NewValue, contract.HasSupplierSigned);
            Assert.Equal(parameters.Supplier.Value.SignedAt.NewValue?.Date, contract.SupplierSignedDate);
            Assert.Equal(parameters.Supplier.Value.SignedBy.NewValue, contract.SupplierContractSigner);

            AssertPaymentModel(paymentModel, contract, true);

            //Agreement period
            Assert.Equal(agreementPeriodInput.DurationMonths.NewValue, contract.DurationMonths);
            Assert.Equal(agreementPeriodInput.DurationYears.NewValue, contract.DurationYears);
            Assert.Equal(agreementPeriodInput.IsContinuous.NewValue, contract.DurationOngoing);
            Assert.Equal(agreementPeriodInput.ExtensionOptionsUsed.NewValue, contract.ExtendMultiplier);
            Assert.Equal(agreementPeriodInput.IrrevocableUntil.NewValue, contract.IrrevocableTo);

            //Termination
            Assert.Equal(terminationInput.TerminatedAt.NewValue.Match(val => val, () => (DateTime?)null), contract.Terminated);
            Assert.Equal(terminationInput.NoticePeriodExtendsCurrent.NewValue.Match(val => val, () => (YearSegmentOption?)null), contract.Running);
            Assert.Equal(terminationInput.NoticeByEndOf.NewValue.Match(val => val, () => (YearSegmentOption?)null), contract.ByEnding);

            AssertPayments(parameters.Payments.Value, contract);
        }

        [Fact]
        public void Can_Add_Role()
        {
            //Arrange
            var (_, _, contract, transaction) = SetupCreateScenarioPrerequisites();
            var existingAssignment = A<UserRolePair>();
            contract.Rights.Add(new ItContractRight { Role = new ItContractRole { Uuid = existingAssignment.RoleUuid }, User = new User { Uuid = existingAssignment.UserUuid } });
            var newAssignment = A<UserRolePair>();

            ExpectGetReturns(contract.Uuid, contract);
            ExpectAllowModifySuccess(contract);

            _roleAssignmentService.Setup(x => x.BatchUpdateRoles(
                        contract,
                        It.Is<IEnumerable<(Guid roleUuid, Guid user)>>(assignments =>
                            MatchExpectedAssignments(assignments, new[] { existingAssignment, newAssignment }.ToList()))
                    )
                )
                .Returns(Maybe<OperationError>.None);

            //Act
            var createResult = _sut.AddRole(contract.Uuid, newAssignment);

            //Assert
            Assert.True(createResult.Ok);
            AssertTransactionCommitted(transaction);
        }
        [Fact]
        public void Can_Add_Role_Range()
        {
            //Arrange
            var (_, _, contract, transaction) = SetupCreateScenarioPrerequisites();
            var existingAssignment = A<UserRolePair>();
            contract.Rights.Add(new ItContractRight { Role = new ItContractRole { Uuid = existingAssignment.RoleUuid }, User = new User { Uuid = existingAssignment.UserUuid } });
            var newAssignment = A<UserRolePair>();
            var newAssignment2 = A<UserRolePair>();

            var assignments = new List<UserRolePair> { newAssignment, newAssignment2 };

            ExpectGetReturns(contract.Uuid, contract);
            ExpectAllowModifySuccess(contract);

            _roleAssignmentService.Setup(x => x.BatchUpdateRoles(
                        contract,
                        It.Is<IEnumerable<(Guid roleUuid, Guid user)>>(assignments =>
                            MatchExpectedAssignments(assignments, new[] { existingAssignment, newAssignment, newAssignment2 }.ToList()))
                    )
                )
                .Returns(Maybe<OperationError>.None);

            //Act
            var createResult = _sut.AddRoleRange(contract.Uuid, assignments);

            //Assert
            Assert.True(createResult.Ok);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Cannot_Add_Duplicate_Role_Assignment()
        {
            //Arrange
            var (_, _, contract, transaction) = SetupCreateScenarioPrerequisites();
            var existingAssignment = A<UserRolePair>();
            contract.Rights.Add(new ItContractRight { Role = new ItContractRole { Uuid = existingAssignment.RoleUuid }, User = new User { Uuid = existingAssignment.UserUuid } });

            ExpectGetReturns(contract.Uuid, contract);
            ExpectAllowModifySuccess(contract);

            //Act
            var createResult = _sut.AddRole(contract.Uuid, existingAssignment);

            //Assert
            Assert.True(createResult.Failed);
            Assert.Equal(OperationFailure.Conflict, createResult.Error.FailureType);
            AssertTransactionNotCommitted(transaction);
        }

        [Fact]
        public void Can_Remove_Role()
        {
            //Arrange
            var (_, _, contract, transaction) = SetupCreateScenarioPrerequisites();
            var existingAssignment1 = A<UserRolePair>();
            var existingAssignment2 = A<UserRolePair>();
            contract.Rights.Add(new ItContractRight { Role = new ItContractRole { Uuid = existingAssignment1.RoleUuid }, User = new User { Uuid = existingAssignment1.UserUuid } });
            contract.Rights.Add(new ItContractRight { Role = new ItContractRole { Uuid = existingAssignment2.RoleUuid }, User = new User { Uuid = existingAssignment2.UserUuid } });

            ExpectGetReturns(contract.Uuid, contract);
            ExpectAllowModifySuccess(contract);

            _roleAssignmentService
                .Setup(x => x.BatchUpdateRoles(
                        contract,
                        It.Is<IEnumerable<(Guid roleUuid, Guid user)>>(assignments =>
                            MatchExpectedAssignments(assignments, new[] { existingAssignment1 }.ToList()))
                    )
                )
                .Returns(Maybe<OperationError>.None);

            //Act
            var createResult = _sut.RemoveRole(contract.Uuid, existingAssignment2);

            //Assert
            Assert.True(createResult.Ok);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Cannot_Remove_Role_If_Not_Assigned()
        {
            //Arrange
            var (_, _, contract, transaction) = SetupCreateScenarioPrerequisites();
            var existingAssignment1 = A<UserRolePair>();
            var assignmentThatDoesNotExist = A<UserRolePair>();
            contract.Rights.Add(new ItContractRight { Role = new ItContractRole { Uuid = existingAssignment1.RoleUuid }, User = new User { Uuid = existingAssignment1.UserUuid } });

            ExpectGetReturns(contract.Uuid, contract);
            ExpectAllowModifySuccess(contract);

            //Act
            var createResult = _sut.RemoveRole(contract.Uuid, assignmentThatDoesNotExist);

            //Assert
            Assert.True(createResult.Failed);
            Assert.Equal(OperationFailure.BadInput, createResult.Error.FailureType);
            AssertTransactionNotCommitted(transaction);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_Set_RequireValidContract_When_Parent_Exists(bool requireValidParent)
        {
            var newParentContract = new ItContract { Uuid = A<Guid>() };
            var contract = new ItContract { Uuid = A<Guid>() };
            var parameters = new ItContractModificationParameters
            {
                ParentContractUuid = ((Guid?)newParentContract.Uuid).AsChangedValue(),
                General = new ItContractGeneralDataModificationParameters
                {
                    RequireValidParent = requireValidParent.FromNullable().AsChangedValue(),
                }
            };
            ExpectTransaction();
            ExpectGetReturns(newParentContract.Uuid, newParentContract);
            ExpectGetReturns(contract.Uuid, contract);
            ExpectAllowModifySuccess(contract);

            var result = _sut.Update(contract.Uuid, parameters);

            Assert.True(result.Ok);
            Assert.Equal(requireValidParent, result.Value.RequireValidParent);
        }

        [Fact]
        public void Can_Not_Set_RequireValidParent_To_True_When_No_Parent_Exists()
        {
            var contract = new ItContract { Uuid = A<Guid>() };
            var parameters = new ItContractModificationParameters
            {
                General = new ItContractGeneralDataModificationParameters
                {
                    RequireValidParent = true.FromNullable().AsChangedValue(),
                }
            };
            ExpectTransaction();
            ExpectGetReturns(contract.Uuid, contract);
            ExpectAllowModifySuccess(contract);

            var result = _sut.Update(contract.Uuid, parameters);

            Assert.True(result.Failed);
        }

        [Fact]
        public void RequireValidParent_Is_Set_To_False_If_Parent_Is_Cleared()
        {
            var contract = new ItContract { Uuid = A<Guid>(), Parent = new ItContract(), RequireValidParent = true };
            var parameters = new ItContractModificationParameters
            {
                ParentContractUuid = ((Guid?)null).AsChangedValue(),
            };
            ExpectTransaction();
            ExpectGetReturns(contract.Uuid, contract);
            ExpectAllowModifySuccess(contract);

            var result = _sut.Update(contract.Uuid, parameters);

            Assert.True(result.Ok);
            Assert.False(result.Value.RequireValidParent);
        }

        private static void AssertPayments(ItContractPaymentDataModificationParameters input, ItContract updatedContract)
        {
            AssertPaymentStream(input.ExternalPayments.NewValue.ToList(), updatedContract.ExternEconomyStreams.ToList());
            AssertPaymentStream(input.InternalPayments.NewValue.ToList(), updatedContract.InternEconomyStreams.ToList());
        }

        private static void AssertPaymentStream(List<ItContractPayment> expectedPayments, List<EconomyStream> actualEconomyStreams)
        {
            Assert.Equal(expectedPayments.Count, actualEconomyStreams.Count);
            for (var i = 0; i < expectedPayments.Count; i++)
            {
                var expected = expectedPayments[i];
                var actual = actualEconomyStreams[i];
                Assert.Equal(expected.Note, actual.Note);
                Assert.Equal(expected.Other, actual.Other);
                Assert.Equal(expected.Acquisition, actual.Acquisition);
                Assert.Equal(expected.Operation, actual.Operation);
                Assert.Equal(expected.AccountingEntry, actual.AccountingEntry);
                Assert.Equal(expected.OrganizationUnitUuid, actual.OrganizationUnit?.Uuid);
                Assert.Equal(expected.AuditStatus, actual.AuditStatus);
                Assert.Equal(expected.AuditDate?.Date, actual.AuditDate?.Date);
            }
        }

        private static void AssertPaymentModel(ItContractPaymentModelModificationParameters expected, ItContract actual, bool hasValues)
        {
            Assert.Equal(expected.OperationsRemunerationStartedAt.NewValue.Match<DateTime?>(date => date.Date, () => null), actual.OperationRemunerationBegun?.Date);
        }

        private ItContractPaymentModelModificationParameters CreatePaymentModel(bool withValues)
        {
            return new()
            {
                OperationsRemunerationStartedAt = (withValues ? A<DateTime>().FromNullable() : Maybe<DateTime>.None).AsChangedValue(),
                PaymentModelUuid = (withValues ? A<Guid>() : (Guid?)null).AsChangedValue(),
                PaymentFrequencyUuid = (withValues ? A<Guid>() : (Guid?)null).AsChangedValue(),
                PriceRegulationUuid = (withValues ? A<Guid>() : (Guid?)null).AsChangedValue()
            };
        }

        private void ExpectBatchUpdateExternalReferencesReturns(ItContract contract, IEnumerable<UpdatedExternalReferenceProperties> externalReferences, Maybe<OperationError> value)
        {
            _referenceServiceMock
                .Setup(x => x.UpdateExternalReferences(ReferenceRootType.Contract, contract.Id, externalReferences))
                .Returns(value);
        }

        private void ExpectAllowModifySuccess(ItContract contract)
        {
            _authContext.Setup(x => x.AllowModify(contract)).Returns(true);
        }

        private (Guid? procurementStrategyUuid, Guid? purchaseTypeUuid, ItContractProcurementModificationParameters parameters) CreateProcurementParameters(bool withStrategy, bool withPurchase, bool withPlan, bool withInitiated)
        {
            var procurementStrategyUuid = withStrategy ? A<Guid>() : (Guid?)null;
            var purchaseTypeUuid = withPurchase ? A<Guid>() : (Guid?)null;
            var procurementInitiated = withInitiated ? A<YesNoUndecidedOption>().FromNullable() : Maybe<YesNoUndecidedOption>.None;
            var procurement = new ItContractProcurementModificationParameters
            {
                ProcurementStrategyUuid = procurementStrategyUuid.AsChangedValue(),
                PurchaseTypeUuid = purchaseTypeUuid.AsChangedValue(),
                ProcurementPlan = (withPlan ? (CreateValidHalfOfYearByte(), A<int>()) : Maybe<(byte half, int year)>.None).AsChangedValue(),
                ProcurementInitiated = procurementInitiated.AsChangedValue() ?? Maybe<YesNoUndecidedOption>.None.AsChangedValue()
            };
            return (procurementStrategyUuid, purchaseTypeUuid, procurement);
        }

        private byte CreateValidHalfOfYearByte()
        {
            return Convert.ToByte(A<int>() % 1 + 1);
        }

        private static void AssertProcurement(ItContractProcurementModificationParameters expected, ItContract actual)
        {
            if (expected.ProcurementPlan.HasChange && expected.ProcurementPlan.NewValue.HasValue)
            {
                Assert.Equal(expected.ProcurementPlan.NewValue.Value.quarter, actual.ProcurementPlanQuarter);
                Assert.Equal(expected.ProcurementPlan.NewValue.Value.year, actual.ProcurementPlanYear);
            }
            else
            {
                Assert.Null(actual.ProcurementPlanQuarter);
                Assert.Null(actual.ProcurementPlanYear);
            }
            if (expected.ProcurementInitiated.HasChange && expected.ProcurementInitiated.NewValue.HasValue)
            {
                Assert.Equal(expected.ProcurementInitiated.NewValue.Value, actual.ProcurementInitiated);
            }
            else
            {
                Assert.Equal(YesNoUndecidedOption.Undecided, actual.ProcurementInitiated);
            }
        }

        private void ExpectUpdateMultiAssignmentReturns<TAssignmentInput, TAssignmentState>(ItContract contract, Maybe<IEnumerable<Guid>> assignmentUuids, Maybe<OperationError> result)
            where TAssignmentInput : class, IHasId, IHasUuid
            where TAssignmentState : class, IHasId, IHasUuid
        {
            _assignmentUpdateServiceMock
                .Setup(x => x.UpdateUniqueMultiAssignment(
                    It.IsAny<string>(),
                    contract,
                    assignmentUuids,
                    It.IsAny<Func<Guid, Result<TAssignmentInput, OperationError>>>(),
                    It.IsAny<Func<ItContract, IEnumerable<TAssignmentState>>>(),
                    It.IsAny<Func<ItContract, TAssignmentInput, Maybe<OperationError>>>(),
                    It.IsAny<Func<ItContract, TAssignmentState, Maybe<OperationError>>>(),
                    null))
                .Returns(result);
        }

        private void ExpectUpdateIndependentOptionTypeAssignmentReturns<TOption>(ItContract contract, Guid? optionUuid, Maybe<OperationError> result) where TOption : OptionEntity<ItContract>
        {
            _assignmentUpdateServiceMock
                .Setup(x => x.UpdateIndependentOptionTypeAssignment(
                    contract,
                    optionUuid,
                    It.IsAny<Action<ItContract>>(),
                    It.IsAny<Func<ItContract, TOption>>(),
                    It.IsAny<Action<ItContract, TOption>>()))
                .Returns(result);
        }

        private void ExpectGetOptionTypeReturnsIfInputIdIsDefined<TOption>(Guid organizationUuid, Guid? optionTypeUuid, Result<(TOption, bool), OperationError> result) where TOption : OptionEntity<ItContract>
        {
            if (optionTypeUuid.HasValue)
                _optionResolverMock.Setup(x => x.GetOptionType<ItContract, TOption>(organizationUuid, optionTypeUuid.Value)).Returns(result);
        }

        private (Guid organizationUuid, ItContractModificationParameters parameters, ItContract createdContract, Mock<IDatabaseTransaction> transaction) SetupCreateScenarioPrerequisites(
            Guid? parentUuid = null,
            ItContractProcurementModificationParameters procurement = null,
            ItContractResponsibleDataModificationParameters responsible = null,
            ItContractSupplierModificationParameters supplier = null,
            IEnumerable<UpdatedExternalReferenceProperties> externalReferences = null,
            IEnumerable<Guid> systemUsageUuids = null,
            IEnumerable<UserRolePair> roleAssignments = null,
            IEnumerable<Guid> dataProcessingRegistrationUuids = null,
            ItContractPaymentModelModificationParameters paymentModel = null,
            ItContractAgreementPeriodModificationParameters agreementPeriod = null,
            ItContractPaymentDataModificationParameters payments = null
            )
        {
            var organization = new Organization
            {
                Id = A<int>(),
                Uuid = A<Guid>()
            };
            var parameters = new ItContractModificationParameters
            {
                Name = A<string>().AsChangedValue(),
                ParentContractUuid = parentUuid.AsChangedValue(),
                Procurement = procurement.FromNullable(),
                Responsible = responsible.FromNullable(),
                Supplier = supplier.FromNullable(),
                ExternalReferences = externalReferences.FromNullable(),
                SystemUsageUuids = systemUsageUuids.FromNullable(),
                Roles = roleAssignments.FromNullable(),
                DataProcessingRegistrationUuids = dataProcessingRegistrationUuids.FromNullable(),
                PaymentModel = paymentModel.FromNullable(),
                AgreementPeriod = agreementPeriod.FromNullable(),
                Payments = payments.FromNullable(),
            };
            var createdContract = new ItContract
            {
                Id = A<int>(),
                Uuid = A<Guid>(),
                OrganizationId = organization.Id,
                Organization = organization
            };
            var transaction = ExpectTransaction();

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<Organization>(organization.Uuid, createdContract.OrganizationId);
            ExpectCreateReturns(createdContract.OrganizationId, parameters.Name.NewValue, createdContract);
            return (organization.Uuid, parameters, createdContract, transaction);
        }

        private (string contractId, Guid? contractTypeUuid, Guid? contractTemplateUuid, bool enforceValid, DateTime? validFrom, DateTime? validTo, Guid? criticalityTypeUuid, List<Guid> agreementElementUuids, Dictionary<Guid, AgreementElementType> agreementElementTypes, ItContractGeneralDataModificationParameters parameters) SetupGeneralSectionInput(
          bool withContractType,
          bool withContractTemplate,
          bool withAgreementElements,
          bool withValidFrom,
          bool withValidTo,
          bool withCriticalityType,
          ItContract contract,
          Guid organizationUuid)
        {
            var contractId = A<string>();
            var contractTypeUuid = withContractType ? A<Guid>() : (Guid?)null;
            var contractTemplateUuid = withContractTemplate ? A<Guid>() : (Guid?)null;
            var enforceValid = A<bool>();
            var validFrom = withValidFrom ? A<DateTime>().Date : (DateTime?)null;
            var validTo = withValidTo ? (validFrom ?? A<DateTime>()).AddDays(Math.Abs(A<int>() % 100)).Date : (DateTime?)null;
            var criticalityTypeUuid = withCriticalityType ? A<Guid>() : (Guid?)null;
            var agreementElementUuids = withAgreementElements ? Many<Guid>().ToList() : new List<Guid>();
            var parameters = new ItContractGeneralDataModificationParameters
            {
                ContractId = contractId.AsChangedValue(),
                ContractTypeUuid = ((Guid?)contractTypeUuid).AsChangedValue(),
                ContractTemplateUuid = ((Guid?)contractTemplateUuid).AsChangedValue(),
                EnforceValid = enforceValid.FromNullable().AsChangedValue(),
                ValidFrom = validFrom?.FromNullable().AsChangedValue() ?? Maybe<DateTime>.None.AsChangedValue(),
                ValidTo = validTo?.FromNullable().AsChangedValue() ?? Maybe<DateTime>.None.AsChangedValue(),
                CriticalityUuid = criticalityTypeUuid.AsChangedValue(),
                AgreementElementUuids = agreementElementUuids.AsChangedValue<IEnumerable<Guid>>()
            };

            ExpectUpdateIndependentOptionTypeAssignmentReturns<ItContractType>(contract, contractTypeUuid, Maybe<OperationError>.None);

            ExpectUpdateIndependentOptionTypeAssignmentReturns<ItContractTemplateType>(contract, contractTemplateUuid, Maybe<OperationError>.None);

            ExpectUpdateIndependentOptionTypeAssignmentReturns<CriticalityType>(contract, criticalityTypeUuid, Maybe<OperationError>.None);

            var agreementElementTypes = agreementElementUuids.ToDictionary(uuid => uuid,
                uuid => new AgreementElementType() { Id = A<int>(), Uuid = uuid });

            foreach (var agreementElementType in agreementElementTypes)
                ExpectGetOptionTypeReturnsIfInputIdIsDefined<AgreementElementType>(organizationUuid, agreementElementType.Key,
                    (agreementElementType.Value, true));
            return (contractId, contractTypeUuid, contractTemplateUuid, enforceValid, validFrom, validTo, criticalityTypeUuid, agreementElementUuids, agreementElementTypes, parameters);
        }

        private static void AssertGeneralSection(
            string expectedContractId,
            DateTime? expectedValidFrom,
            DateTime? expectedValidTo,
            bool expectedEnforceValid,
            Dictionary<Guid, AgreementElementType> expectedAgreementElementTypes,
            List<Guid> expectedAgreementElementUuids,
            ItContract actualContract,
            bool expectedRequireValidParent)
        {
            Assert.Equal(expectedContractId, actualContract.ItContractId);
            Assert.Equal(expectedValidFrom, actualContract.Concluded);
            Assert.Equal(expectedValidTo, actualContract.ExpirationDate);
            Assert.Equal(expectedEnforceValid, actualContract.Active);
            Assert.Equal(expectedAgreementElementTypes.Count, actualContract.AssociatedAgreementElementTypes.Count);
            Assert.Equal(expectedRequireValidParent, actualContract.RequireValidParent);
            var agreementElementsDiff = expectedAgreementElementUuids
                .Except(actualContract.AssociatedAgreementElementTypes.Select(x => x.AgreementElementType.Uuid)).ToList();
            Assert.Empty(agreementElementsDiff);
        }

        private void ExpectGetReturns(Guid contractUuid, Result<ItContract, OperationError> result)
        {
            _itContractServiceMock.Setup(x => x.GetContract(contractUuid)).Returns(result);
        }

        private void ExpectCreateReturns(int organizationId, string name, Result<ItContract, OperationError> result)
        {
            _itContractServiceMock.Setup(x => x.Create(organizationId, name)).Returns(result);
        }

        private void ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<T>(Guid? uuid, Maybe<int> dbId) where T : class, IHasUuid, IHasId
        {
            if (uuid.HasValue)
                _identityResolverMock.Setup(x => x.ResolveDbId<T>(uuid.Value)).Returns(dbId);
        }

        private static void AssertFailureWithKnownError(Result<ItContract, OperationError> result, OperationError operationError, Mock<IDatabaseTransaction> transaction)
        {
            Assert.True(result.Failed);
            Assert.Equal(operationError, result.Error);
            AssertTransactionNotCommitted(transaction);
        }

        private static void AssertFailureWithKnownErrorDetails(Result<ItContract, OperationError> result, string errorMessageContent, OperationFailure failure, Mock<IDatabaseTransaction> transaction)
        {
            Assert.True(result.Failed);
            Assert.Contains(errorMessageContent, result.Error.Message.GetValueOrEmptyString());
            Assert.Equal(failure, result.Error.FailureType);
            AssertTransactionNotCommitted(transaction);
        }
        private void AssertTransactionCommitted(Mock<IDatabaseTransaction> transactionMock)
        {
            _databaseControlMock.Verify(x => x.SaveChanges(), Times.AtLeastOnce);
            transactionMock.Verify(x => x.Commit());
        }
        private static void AssertTransactionNotCommitted(Mock<IDatabaseTransaction> transactionMock)
        {
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        private Mock<IDatabaseTransaction> ExpectTransaction()
        {
            var trasactionMock = new Mock<IDatabaseTransaction>();
            _transactionManagerMock.Setup(x => x.Begin()).Returns(trasactionMock.Object);
            return trasactionMock;
        }

        private static bool MatchExpectedAssignments(IEnumerable<(Guid roleUuid, Guid user)> actual, List<UserRolePair> expected)
        {
            return actual.SequenceEqual(expected.Select(p => (roleUuid: p.RoleUuid, user: p.UserUuid)));
        }
    }
}
