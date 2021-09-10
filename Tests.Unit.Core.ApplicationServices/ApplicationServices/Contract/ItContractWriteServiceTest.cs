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
using Core.ApplicationServices.Model.Contracts.Write;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.OptionTypes;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.ItContract;
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

        public ItContractWriteServiceTest()
        {
            _itContractServiceMock = new Mock<IItContractService>();
            _identityResolverMock = new Mock<IEntityIdentityResolver>();
            _optionResolverMock = new Mock<IOptionResolver>();
            _transactionManagerMock = new Mock<ITransactionManager>();
            _domainEventsMock = new Mock<IDomainEvents>();
            _databaseControlMock = new Mock<IDatabaseControl>();
            _agreementElementTypeRepository = new Mock<IGenericRepository<ItContractAgreementElementTypes>>();
            _sut = new ItContractWriteService(_itContractServiceMock.Object, _identityResolverMock.Object, _optionResolverMock.Object, _transactionManagerMock.Object, _domainEventsMock.Object, _databaseControlMock.Object, _agreementElementTypeRepository.Object, Mock.Of<IAuthorizationContext>());
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
            var (orgUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(parentUuid: parentUuid);
            var parent = new ItContract() { Id = A<int>(), Uuid = parentUuid, OrganizationId = createdContract.OrganizationId };
            ExpectGetReturns(parent.Uuid, parent);

            //Act
            var result = _sut.Create(orgUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(parent, result.Value.Parent);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Can_Create_With_No_Parent()
        {
            //Arrange
            var (orgUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(parentUuid: null);

            //Act
            var result = _sut.Create(orgUuid, parameters);

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
            var (orgUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(parentUuid: parent.Uuid);
            var operationError = A<OperationError>();
            ExpectGetReturns(parent.Uuid, operationError);

            //Act
            var result = _sut.Create(orgUuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(result, "Failed to get contract with Uuid:", operationError.FailureType, transaction);
        }

        [Fact]
        public void Cannot_Create_With_Parent_If_Different_Organizations()
        {
            //Arrange
            var parent = new ItContract() { Id = A<int>(), Uuid = A<Guid>(), OrganizationId = A<int>() };
            var (orgUuid, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(parentUuid: parent.Uuid);
            ExpectGetReturns(parent.Uuid, parent);

            //Act
            var result = _sut.Create(orgUuid, parameters);

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

            itContractModificationParameters.General = parameters;

            ExpectGetOptionTypeReturnsIfInputIdIsDefined<ItContractType>(organizationUuid, contractTypeUuid, (new ItContractType() { Uuid = contractTypeUuid.GetValueOrDefault() }, true));
            ExpectGetOptionTypeReturnsIfInputIdIsDefined<ItContractTemplateType>(organizationUuid, contractTemplateUuid, (new ItContractTemplateType() { Uuid = contractTemplateUuid.GetValueOrDefault() }, true));
            var agreementElementTypes = agreementElementUuids.ToDictionary(uuid => uuid, uuid => new AgreementElementType() { Id = A<int>(), Uuid = uuid });

            foreach (var agreementElementType in agreementElementTypes)
                ExpectGetOptionTypeReturnsIfInputIdIsDefined<AgreementElementType>(organizationUuid, agreementElementType.Key, (agreementElementType.Value, true));


            // Act
            var result = _sut.Create(organizationUuid, itContractModificationParameters);

            // Assert
            Assert.True(result.Ok);
            AssertTransactionCommitted(transaction);
            var contract = result.Value;
            Assert.Equal(contractId, contract.ItContractId);
            Assert.Equal(contractTypeUuid, contract.ContractType?.Uuid);
            Assert.Equal(contractTemplateUuid, contract.ContractTemplate?.Uuid);
            Assert.Equal(validFrom, contract.Concluded);
            Assert.Equal(validTo, contract.ExpirationDate);
            Assert.Equal(enforceValid, contract.Active);
            Assert.Equal(agreementElementTypes.Count, contract.AssociatedAgreementElementTypes.Count);
            var agreementElementsDiff = agreementElementUuids.Except(contract.AssociatedAgreementElementTypes.Select(x => x.AgreementElementType.Uuid)).ToList();
            Assert.Empty(agreementElementsDiff);
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
        public void Cannot_Create_With_GeneralData_If_Contract_Template_Is_Not_Available()
        {
            // Arrange
            var (organizationUuid, itContractModificationParameters, createdContract, transaction) = SetupCreateScenarioPrerequisites();

            var contractTemplateId = A<Guid>();
            var parameters = new ItContractGeneralDataModificationParameters
            {
                ContractTemplateUuid = ((Guid?)contractTemplateId).AsChangedValue()
            };

            itContractModificationParameters.General = parameters;

            ExpectGetOptionTypeReturnsIfInputIdIsDefined<ItContractTemplateType>(organizationUuid, contractTemplateId, (new ItContractTemplateType(), false));

            // Act
            var result = _sut.Create(organizationUuid, itContractModificationParameters);

            // Assert
            AssertFailureWithKnownErrorDetails(result, "The changed ItContractTemplateType points to an option which is not available in the organization", OperationFailure.BadInput, transaction);
        }

        [Fact]
        public void Cannot_Create_With_GeneralData_If_Contract_Template_Fails_To_Fetch()
        {
            // Arrange
            var (organizationUuid, itContractModificationParameters, createdContract, transaction) = SetupCreateScenarioPrerequisites();

            var contractTemplateId = A<Guid>();
            var parameters = new ItContractGeneralDataModificationParameters
            {
                ContractTemplateUuid = ((Guid?)contractTemplateId).AsChangedValue()
            };

            itContractModificationParameters.General = parameters;

            var operationError = A<OperationError>();
            ExpectGetOptionTypeReturnsIfInputIdIsDefined<ItContractTemplateType>(organizationUuid, contractTemplateId, operationError);

            // Act
            var result = _sut.Create(organizationUuid, itContractModificationParameters);

            // Assert
            AssertFailureWithKnownErrorDetails(result, $"Failure while resolving ItContractTemplateType option:{operationError.Message.GetValueOrEmptyString()}", operationError.FailureType, transaction);
        }

        //TODO: from here
        [Fact]
        public void Cannot_Create_With_GeneralData_If_Contract_Type_Is_Not_Available()
        {
            // Arrange
            var (organizationUuid, itContractModificationParameters, createdContract, transaction) = SetupCreateScenarioPrerequisites();

            var contractTypeUuid = A<Guid>();
            var parameters = new ItContractGeneralDataModificationParameters
            {
                ContractTypeUuid = ((Guid?)contractTypeUuid).AsChangedValue()
            };

            itContractModificationParameters.General = parameters;

            ExpectGetOptionTypeReturnsIfInputIdIsDefined<ItContractType>(organizationUuid, contractTypeUuid, (new ItContractType(), false));

            // Act
            var result = _sut.Create(organizationUuid, itContractModificationParameters);

            // Assert
            AssertFailureWithKnownErrorDetails(result, "The changed ItContractType points to an option which is not available in the organization", OperationFailure.BadInput, transaction);
        }

        [Fact]
        public void Cannot_Create_With_GeneralData_If_Contract_Type_Fails_To_Fetch()
        {
            // Arrange
            var (organizationUuid, itContractModificationParameters, createdContract, transaction) = SetupCreateScenarioPrerequisites();

            var typeId = A<Guid>();
            var parameters = new ItContractGeneralDataModificationParameters
            {
                ContractTypeUuid = ((Guid?)typeId).AsChangedValue()
            };

            itContractModificationParameters.General = parameters;

            var operationError = A<OperationError>();
            ExpectGetOptionTypeReturnsIfInputIdIsDefined<ItContractType>(organizationUuid, typeId, operationError);

            // Act
            var result = _sut.Create(organizationUuid, itContractModificationParameters);

            // Assert
            AssertFailureWithKnownErrorDetails(result, $"Failure while resolving ItContractType option:{operationError.Message.GetValueOrEmptyString()}", operationError.FailureType, transaction);
        }

        private void ExpectGetOptionTypeReturnsIfInputIdIsDefined<TOption>(Guid organizationUuid, Guid? optionTypeUuid, Result<(TOption, bool), OperationError> result) where TOption : OptionEntity<ItContract>
        {
            if (optionTypeUuid.HasValue)
                _optionResolverMock.Setup(x => x.GetOptionType<ItContract, TOption>(organizationUuid, optionTypeUuid.Value)).Returns(result);
        }

        private (Guid organizationUuid, ItContractModificationParameters parameters, ItContract createdContract, Mock<IDatabaseTransaction> transaction) SetupCreateScenarioPrerequisites(
            Guid? parentUuid = null
            )
        {
            var organizationUuid = A<Guid>();
            var parameters = new ItContractModificationParameters
            {
                Name = A<string>().AsChangedValue(),
                ParentContractUuid = parentUuid.AsChangedValue()
            };
            var createdContract = new ItContract()
            {
                Id = A<int>(),
                Uuid = A<Guid>(),
                OrganizationId = A<int>(),
                Organization = new Organization() { Uuid = organizationUuid }
            };
            var transaction = ExpectTransaction();

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<Organization>(organizationUuid, createdContract.OrganizationId);
            ExpectCreateReturns(createdContract.OrganizationId, parameters.Name.NewValue, createdContract);
            return (organizationUuid, parameters, createdContract, transaction);
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

        private void AssertFailureWithKnownErrorDetails(Result<ItContract, OperationError> result, string errorMessageContent, OperationFailure failure, Mock<IDatabaseTransaction> transaction)
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
