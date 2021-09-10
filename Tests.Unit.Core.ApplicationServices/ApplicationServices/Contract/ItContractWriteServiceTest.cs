using System;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Contract;
using Core.ApplicationServices.Contract.Write;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Contracts.Write;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.OptionTypes;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.Organization;
using Core.DomainServices.Generic;
using FluentAssertions;
using Infrastructure.Services.DataAccess;
using Moq;
using Moq.Language.Flow;
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

        public ItContractWriteServiceTest()
        {
            _itContractServiceMock = new Mock<IItContractService>();
            _identityResolverMock = new Mock<IEntityIdentityResolver>();
            _optionResolverMock = new Mock<IOptionResolver>();
            _transactionManagerMock = new Mock<ITransactionManager>();
            _domainEventsMock = new Mock<IDomainEvents>();
            _databaseControlMock = new Mock<IDatabaseControl>();
            _sut = new ItContractWriteService(_itContractServiceMock.Object, _identityResolverMock.Object, _optionResolverMock.Object, _transactionManagerMock.Object, _domainEventsMock.Object, _databaseControlMock.Object);
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
            var (organization, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(parentUuid: parentUuid);
            var parent = new ItContract() { Id = A<int>(), Uuid = parentUuid, OrganizationId = createdContract.OrganizationId };
            ExpectGetReturns(parent.Uuid, parent);

            //Act
            var result = _sut.Create(organization.Uuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(parent, result.Value.Parent);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Can_Create_With_No_Parent()
        {
            //Arrange
            var (organization, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(parentUuid: null);

            //Act
            var result = _sut.Create(organization.Uuid, parameters);

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
            var (organization, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(parentUuid: parent.Uuid);
            var operationError = A<OperationError>();
            ExpectGetReturns(parent.Uuid, operationError);

            //Act
            var result = _sut.Create(organization.Uuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(result, "Failed to get contract with Uuid:", operationError.FailureType, transaction);
        }

        [Fact]
        public void Cannot_Create_With_Parent_If_Different_Organizations()
        {
            //Arrange
            var parent = new ItContract() { Id = A<int>(), Uuid = A<Guid>(), OrganizationId = A<int>() };
            var (organization, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(parentUuid: parent.Uuid);
            ExpectGetReturns(parent.Uuid, parent);

            //Act
            var result = _sut.Create(organization.Uuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(result, "Failed to set parent with Uuid:", OperationFailure.BadInput, transaction);
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
            var (organization, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(procurement: procurement);
            ExpectItContractOptionResolve<ProcurementStrategyType>(organization.Uuid, procurementStrategyUuid.GetValueOrDefault(), (new ProcurementStrategyType() { Uuid = procurementStrategyUuid.GetValueOrDefault() }, true));
            ExpectItContractOptionResolve<PurchaseFormType>(organization.Uuid, purchaseTypeUuid.GetValueOrDefault(), (new PurchaseFormType() { Uuid = purchaseTypeUuid.GetValueOrDefault() }, true));

            //Act
            var result = _sut.Create(organization.Uuid, parameters);

            //Assert
            Assert.True(result.Ok);
            AssertProcurement(procurement, result.Value, withPlan);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Cannot_Create_With_Procurement_If_ProcurementStrategy_Is_Not_Available()
        {
            //Arrange
            var procurementStrategyUuid = A<Guid>();
            var procurement = new ItContractProcurementModificationParameters()
            {
                ProcurementStrategyUuid = ((Guid?)procurementStrategyUuid).AsChangedValue()
            };
            var (organization, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(procurement: procurement);
            ExpectItContractOptionResolve<ProcurementStrategyType>(organization.Uuid, procurementStrategyUuid, (new ProcurementStrategyType(), false));

            //Act
            var result = _sut.Create(organization.Uuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(result, "The changed ProcurementStrategyType points to an option which is not available in the organization", OperationFailure.BadInput, transaction);
        }

        [Fact]
        public void Cannot_Create_With_Procurement_If_ProcurementStrategy_Is_Not_Found()
        {
            //Arrange
            var procurementStrategyUuid = A<Guid>();
            var procurement = new ItContractProcurementModificationParameters()
            {
                ProcurementStrategyUuid = ((Guid?)procurementStrategyUuid).AsChangedValue()
            };
            var (organization, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(procurement: procurement);

            var operationError = A<OperationError>();
            ExpectItContractOptionResolve<ProcurementStrategyType>(organization.Uuid, procurementStrategyUuid, operationError);

            //Act
            var result = _sut.Create(organization.Uuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(result, $"Failure while resolving ProcurementStrategyType option:{operationError.Message.GetValueOrEmptyString()}", operationError.FailureType, transaction);
        }

        [Fact]
        public void Cannot_Create_With_Procurement_If_PurchaseType_Is_Not_Available()
        {
            //Arrange
            var purchaseTypeUuid = A<Guid>();
            var procurement = new ItContractProcurementModificationParameters()
            {
                PurchaseTypeUuid = ((Guid?)purchaseTypeUuid).AsChangedValue()
            };
            var (organization, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(procurement: procurement);
            ExpectItContractOptionResolve<PurchaseFormType>(organization.Uuid, purchaseTypeUuid, (new PurchaseFormType(), false));

            //Act
            var result = _sut.Create(organization.Uuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(result, "The changed PurchaseFormType points to an option which is not available in the organization", OperationFailure.BadInput, transaction);
        }

        [Fact]
        public void Cannot_Create_With_Procurement_If_PurchaseType_Is_Not_Found()
        {
            //Arrange
            var purchaseTypeUuid = A<Guid>();
            var procurement = new ItContractProcurementModificationParameters()
            {
                PurchaseTypeUuid = ((Guid?)purchaseTypeUuid).AsChangedValue()
            };
            var (organization, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(procurement: procurement);

            var operationError = A<OperationError>();
            ExpectItContractOptionResolve<PurchaseFormType>(organization.Uuid, purchaseTypeUuid, operationError);

            //Act
            var result = _sut.Create(organization.Uuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(result, $"Failure while resolving PurchaseFormType option:{operationError.Message.GetValueOrEmptyString()}", operationError.FailureType, transaction);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Cannot_Create_With_Procurement_If_Not_Both_Parts_Of_ProcurementPlan_Is_Set(bool hasYear)
        {
            //Arrange
            var procurement = new ItContractProcurementModificationParameters()
            {
                HalfOfYear = (hasYear ? Maybe<byte>.None : CreateValidHalfOfYearByte()).AsChangedValue(),
                Year = (hasYear ? A<int>() : Maybe<int>.None).AsChangedValue()
            };
            var (organization, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(procurement: procurement);

            //Act
            var result = _sut.Create(organization.Uuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(result, "Failed to update procurement plan with error message: Both parts of procurement plan needs to be set", OperationFailure.BadInput, transaction);
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
                HalfOfYear = Convert.ToByte(halfOfYear).FromNullable().AsChangedValue(),
                Year = A<int>().FromNullable().AsChangedValue()
            };
            var (organization, parameters, createdContract, transaction) = SetupCreateScenarioPrerequisites(procurement: procurement);

            //Act
            var result = _sut.Create(organization.Uuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(result, "Failed to update procurement plan with error message: Half Of Year has to be either 1 or 2", OperationFailure.BadInput, transaction);
        }

        private (Guid? procurementStrategyUuid, Guid? purchaseTypeUuid, ItContractProcurementModificationParameters parameters) CreateProcurementParameters(bool withStrategy, bool withPurchase, bool withPlan)
        {
            var procurementStrategyUuid = withStrategy ? A<Guid>() : (Guid?)null;
            var purchaseTypeUuid = withPurchase ? A<Guid>() : (Guid?)null;
            var halfOfYear = withPlan ? CreateValidHalfOfYearByte() : Maybe<byte>.None;
            var year = withPlan ? A<int>() : Maybe<int>.None;
            var procurement = new ItContractProcurementModificationParameters
            {
                ProcurementStrategyUuid = procurementStrategyUuid.AsChangedValue(),
                PurchaseTypeUuid = purchaseTypeUuid.AsChangedValue(),
                HalfOfYear = halfOfYear.AsChangedValue(),
                Year = year.AsChangedValue()
            };
            return (procurementStrategyUuid, purchaseTypeUuid, procurement);
        }

        private byte CreateValidHalfOfYearByte()
        {
            return Convert.ToByte(A<int>() % 1 + 1);
        }

        private static void AssertProcurement(ItContractProcurementModificationParameters expected, ItContract actual, bool withPlan)
        {
            Assert.Equal(expected.ProcurementStrategyUuid.NewValue, actual.ProcurementStrategy?.Uuid);
            Assert.Equal(expected.PurchaseTypeUuid.NewValue, actual.PurchaseForm?.Uuid);
            if (withPlan)
            {
                Assert.Equal(expected.HalfOfYear.NewValue.Value, actual.ProcurementPlanHalf);
                Assert.Equal(expected.Year.NewValue.Value, actual.ProcurementPlanYear);
            }
            else
            {
                Assert.Null(actual.ProcurementPlanHalf);
                Assert.Null(actual.ProcurementPlanYear);
            }
        }

        private void ExpectItContractOptionResolve<T>(Guid orgUuid, Guid optionUuid, Result<(T, bool), OperationError> result) where T : OptionEntity<ItContract>
        {
            _optionResolverMock
                .Setup(x => x.GetOptionType<ItContract, T>(orgUuid, optionUuid))
                .Returns(result);
        }

        private (Organization organizationUuid, ItContractModificationParameters parameters, ItContract createdContract, Mock<IDatabaseTransaction> transaction) SetupCreateScenarioPrerequisites(
            Guid? parentUuid = null,
            ItContractProcurementModificationParameters procurement = null
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
                Procurement = procurement.FromNullable()
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
            return (organization, parameters, createdContract, transaction);
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
