﻿using System;
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
using Core.ApplicationServices.Generic.Write;
using Core.ApplicationServices.Model.Contracts.Write;
using Core.ApplicationServices.OptionTypes;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Generic;
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
        private readonly Mock<IAuthorizationContext> _authContext;
        private readonly Mock<IAssignmentUpdateService> _assignmentUpdateServiceMock;
        private readonly Mock<IItSystemUsageService> _usageServiceMock;
        private readonly Mock<IDataProcessingRegistrationApplicationService> _dprServiceMock;

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
            _authContext = new Mock<IAuthorizationContext>();
            _assignmentUpdateServiceMock = new Mock<IAssignmentUpdateService>();
            _usageServiceMock = new Mock<IItSystemUsageService>();
            _dprServiceMock = new Mock<IDataProcessingRegistrationApplicationService>();
            _sut = new ItContractWriteService(_itContractServiceMock.Object, _identityResolverMock.Object, _optionResolverMock.Object, _transactionManagerMock.Object, _domainEventsMock.Object, _databaseControlMock.Object, _agreementElementTypeRepository.Object, _authContext.Object, _organizationServiceMock.Object, _assignmentUpdateServiceMock.Object, _usageServiceMock.Object, _dprServiceMock.Object);
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
        [InlineData(true, true, true, true, true)]
        [InlineData(true, true, true, true, false)]
        [InlineData(true, true, true, false, true)]
        [InlineData(true, true, false, true, true)]
        [InlineData(true, false, true, true, true)]
        [InlineData(false, true, true, true, true)]
        [InlineData(false, false, false, false, false)]
        public void Can_Create_With_GeneralData(bool withContractType, bool withContractTemplate, bool withAgreementElements, bool withValidFrom, bool withValidTo)
        {
            // Arrange
            var (organizationUuid, itContractModificationParameters, createdContract, transaction) = SetupCreateScenarioPrerequisites();

            var (contractId,
                contractTypeUuid,
                contractTemplateUuid,
                enforceValid,
                validFrom,
                validTo,
                agreementElementUuids,
                agreementElementTypes,
                parameters) = SetupGeneralSectionInput(withContractType, withContractTemplate, withAgreementElements, withValidFrom, withValidTo, createdContract, organizationUuid);

            itContractModificationParameters.General = parameters;


            // Act
            var result = _sut.Create(organizationUuid, itContractModificationParameters);

            // Assert
            Assert.True(result.Ok);
            AssertTransactionCommitted(transaction);
            var contract = result.Value;
            AssertGeneralSection(contractId, contractTypeUuid, contractTemplateUuid, validFrom, validTo, enforceValid, agreementElementTypes, agreementElementUuids, contract);
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

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, true)]
        [InlineData(false, false, false)]
        public void Can_Create_With_Procurement(bool withStrategy, bool withPurchase, bool withPlan)
        {
            //Arrange
            var (procurementStrategyUuid, purchaseTypeUuid, procurement) = CreateProcurementParameters(withStrategy, withPurchase, withPlan);
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
                ProcurementStrategyUuid = ((Guid?) procurementStrategyUuid).AsChangedValue()
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
        [InlineData(3)]
        [InlineData(10)]
        public void Cannot_Create_With_Procurement_If_Half_Of_Year_Is_Other_Than_1_Or_2(int halfOfYear)
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
            AssertFailureWithKnownErrorDetails(result, "Failed to update procurement plan with error message: Half Of Year has to be either 1 or 2", OperationFailure.BadInput, transaction);
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
            Assert.Equal(responsible.SignedAt.NewValue, contract.SignedDate);
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
            Assert.Equal(supplier.SignedAt.NewValue, contract.SupplierSignedDate);
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
            createdContract.AssociatedSystemUsages.Add(new ItContractItSystemUsage{ ItContract = createdContract, ItContractId = createdContract.Id, ItSystemUsage = usage, ItSystemUsageId = usage.Id});

            ExpectUpdateMultiAssignmentReturns<ItSystemUsage, ItSystemUsage>(createdContract, usageUuids, Maybe<OperationError>.None);
            ExpectGetReturns(createdContract.Uuid, createdContract);
            ExpectAllowModifySuccess(createdContract);
            ExpectNameValidationSuccess(createdContract.Id, parameters.Name.NewValue);

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
            ExpectNameValidationSuccess(createdContract.Id, parameters.Name.NewValue);

            //Act
            var result = _sut.Update(createdContract.Uuid, parameters);

            //Assert
            Assert.True(result.Ok);
            AssertTransactionCommitted(transaction);
        }

        private void ExpectNameValidationSuccess(int contractId, string newName)
        {
            _itContractServiceMock.Setup(x => x.ValidateNewName(contractId, newName)).Returns(Maybe<OperationError>.None);
        }

        private void ExpectAllowModifySuccess(ItContract contract)
        {
            _authContext.Setup(x => x.AllowModify(contract)).Returns(true);
        }

        private (Guid? procurementStrategyUuid, Guid? purchaseTypeUuid, ItContractProcurementModificationParameters parameters) CreateProcurementParameters(bool withStrategy, bool withPurchase, bool withPlan)
        {
            var procurementStrategyUuid = withStrategy ? A<Guid>() : (Guid?)null;
            var purchaseTypeUuid = withPurchase ? A<Guid>() : (Guid?)null;
            var procurement = new ItContractProcurementModificationParameters
            {
                ProcurementStrategyUuid = procurementStrategyUuid.AsChangedValue(),
                PurchaseTypeUuid = purchaseTypeUuid.AsChangedValue(),
                ProcurementPlan = (withPlan ? (CreateValidHalfOfYearByte(), A<int>()) : Maybe<(byte half, int year)>.None).AsChangedValue()
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
                Assert.Equal(expected.ProcurementPlan.NewValue.Value.half, actual.ProcurementPlanHalf);
                Assert.Equal(expected.ProcurementPlan.NewValue.Value.year, actual.ProcurementPlanYear);
            }
            else
            {
                Assert.Null(actual.ProcurementPlanHalf);
                Assert.Null(actual.ProcurementPlanYear);
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
                    It.IsAny<Func<ItContract, TAssignmentState, Maybe<OperationError>>>()))
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
            IEnumerable<Guid> systemUsageUuids = null
            )
        {
            var organization = new Organization()
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
                SystemUsageUuids = systemUsageUuids.FromNullable()
            };
            var createdContract = new ItContract()
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

        private (string contractId, Guid? contractTypeUuid, Guid? contractTemplateUuid, bool enforceValid, DateTime? validFrom, DateTime? validTo, List<Guid> agreementElementUuids, Dictionary<Guid, AgreementElementType> agreementElementTypes, ItContractGeneralDataModificationParameters parameters) SetupGeneralSectionInput(
          bool withContractType,
          bool withContractTemplate,
          bool withAgreementElements,
          bool withValidFrom,
          bool withValidTo,
          ItContract contract,
          Guid organizationUuid)
        {
            var contractId = A<string>();
            var contractTypeUuid = withContractType ? A<Guid>() : (Guid?)null;
            var contractTemplateUuid = withContractTemplate ? A<Guid>() : (Guid?)null;
            var enforceValid = A<bool>();
            var validFrom = withValidFrom ? A<DateTime>().Date : (DateTime?)null;
            var validTo = withValidTo ? (validFrom ?? A<DateTime>()).AddDays(Math.Abs(A<int>() % 100)).Date : (DateTime?)null;
            var agreementElementUuids = withAgreementElements ? Many<Guid>().ToList() : new List<Guid>();
            var parameters = new ItContractGeneralDataModificationParameters
            {
                ContractId = contractId.AsChangedValue(),
                ContractTypeUuid = ((Guid?)contractTypeUuid).AsChangedValue(),
                ContractTemplateUuid = ((Guid?)contractTemplateUuid).AsChangedValue(),
                EnforceValid = enforceValid.FromNullable().AsChangedValue(),
                ValidFrom = validFrom?.FromNullable().AsChangedValue() ?? Maybe<DateTime>.None.AsChangedValue(),
                ValidTo = validTo?.FromNullable().AsChangedValue() ?? Maybe<DateTime>.None.AsChangedValue(),
                AgreementElementUuids = agreementElementUuids.AsChangedValue<IEnumerable<Guid>>()
            };

            ExpectUpdateIndependentOptionTypeAssignmentReturns<ItContractType>(contract, contractTypeUuid, Maybe<OperationError>.None);

            ExpectUpdateIndependentOptionTypeAssignmentReturns<ItContractTemplateType>(contract, contractTemplateUuid, Maybe<OperationError>.None);

            var agreementElementTypes = agreementElementUuids.ToDictionary(uuid => uuid,
                uuid => new AgreementElementType() { Id = A<int>(), Uuid = uuid });

            foreach (var agreementElementType in agreementElementTypes)
                ExpectGetOptionTypeReturnsIfInputIdIsDefined<AgreementElementType>(organizationUuid, agreementElementType.Key,
                    (agreementElementType.Value, true));
            return (contractId, contractTypeUuid, contractTemplateUuid, enforceValid, validFrom, validTo, agreementElementUuids, agreementElementTypes, parameters);
        }

        private static void AssertGeneralSection(
            string expectedContractId,
            Guid? expectedContractTypeUuid,
            Guid? expectedContractTemplateUuid,
            DateTime? expectedValidFrom,
            DateTime? expectedValidTo,
            bool expectedEnforceValid,
            Dictionary<Guid, AgreementElementType> expectedAgreementElementTypes,
            List<Guid> expectedAgreementElementUuids,
            ItContract actualContract)
        {
            Assert.Equal(expectedContractId, actualContract.ItContractId);
            Assert.Equal(expectedValidFrom, actualContract.Concluded);
            Assert.Equal(expectedValidTo, actualContract.ExpirationDate);
            Assert.Equal(expectedEnforceValid, actualContract.Active);
            Assert.Equal(expectedAgreementElementTypes.Count, actualContract.AssociatedAgreementElementTypes.Count);
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
    }
}
