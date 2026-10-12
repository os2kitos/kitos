using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.Shared;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.Model
{
    public class DataProcessingRegistrationTest : WithAutoFixture
    {
        [Fact]
        public void IsValid_Returns_False_If_EnforceInvalidity()
        {
            var sut = new DataProcessingRegistration();
            sut.EnforceInvalidity = true;

            var result = sut.IsValid;

            Assert.False(result);
        }

        [Fact]
        public void IsValid_Returns_True_If_MainContract_Is_Null()
        {
            var sut = new DataProcessingRegistration();
            sut.EnforceInvalidity = false;
            sut.MainContract = null;
            var result = sut.IsValid;
            Assert.True(result);
        }

        [Fact]
        public void IsValid_Returns_False_If_MainContract_Is_Invalid()
        {
            var sut = new DataProcessingRegistration();
            sut.EnforceInvalidity = false;
            sut.MainContract = new ItContract
            {
                ExpirationDate = DateTime.Now.AddDays(-1)
            };
            var result = sut.IsValid;
            Assert.False(result);
        }

        [Fact]
        public void Can_AssignMainContract()
        {
            //Arrange
            var contractId = A<int>();
            var contract = new ItContract
            {
                Id = contractId
            };
            var dpr = new DataProcessingRegistration()
            {
                AssociatedContracts = new List<ItContract> { contract }
            };

            //Act
            var error = dpr.AssignMainContract(contractId).MatchFailure();

            //Assert
            Assert.False(error.HasValue);
            Assert.NotNull(dpr.MainContract);
            Assert.Equal(contractId, dpr.MainContract.Id);
        }

        [Fact]
        public void AssignMainContract_Returns_BadState_When_Contract_With_Id_Is_Not_Part_Of_AssociatedContract()
        {
            //Arrange
            var contractId = A<int>();
            var dpr = new DataProcessingRegistration
            {
                AssociatedContracts = new List<ItContract>()
            };

            //Act
            var error = dpr.AssignMainContract(contractId).MatchFailure();

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.BadState, error.Value.FailureType);
            Assert.Null(dpr.MainContract);
        }

        [Fact]
        public void AssignMainContract_Returns_No_Error_When_Contract_With_Same_Id_Is_Present()
        {
            //Arrange
            var contractId = A<int>();
            var dpr = CreateDprWithMainContract(contractId);

            //Act
            var error = dpr.AssignMainContract(contractId).MatchFailure();

            //Assert
            Assert.False(error.HasValue);
            Assert.Equal(contractId, dpr.MainContract.Id);
        }

        [Fact]
        public void Can_ResetMainContract()
        {
            //Arrange
            var dpr = CreateDprWithMainContract(A<int>());

            //Act
            dpr.ResetMainContract();

            //Assert
            Assert.Null(dpr.MainContract);
        }

        [Fact]
        public void CheckDprValidity_Returns_Valid_When_Not_Enforced_And_MainContract_Is_Active()
        {
            var dpr = CreateDprWithActiveMainContract();

            var result = dpr.CheckDprValidity();

            AssertValidResult(result);
        }

        [Fact]
        public void CheckDprValidity_Returns_EnforcedInvalidity_When_EnforceInvalidity_Is_True_And_MainContract_Is_Active()
        {
            var dpr = CreateDprWithActiveMainContract();
            dpr.EnforceInvalidity = true;

            var result = dpr.CheckDprValidity();

            AssertInvalidResult(result, DataProcessingRegistrationValidationError.EnforcedInvalidity);
        }

        [Fact]
        public void CheckDprValidity_Returns_True_When_MainContract_Is_Null()
        {
            var dpr = CreateDprWithoutMainContract();

            var result = dpr.CheckDprValidity();

            Assert.True(result.Result);
        }

        [Fact]
        public void CheckDprValidity_Returns_MainContractNotActive_When_MainContract_Is_Inactive()
        {
            var dpr = CreateDprWithInactiveMainContract();

            var result = dpr.CheckDprValidity();

            AssertInvalidResult(result, DataProcessingRegistrationValidationError.MainContractNotActive);
        }

        [Fact]
        public void CheckDprValidity_Returns_Both_Errors_When_EnforceInvalidity_Is_True_And_MainContract_Is_Inactive()
        {
            var dpr = CreateDprWithInactiveMainContract();
            dpr.EnforceInvalidity = true;

            var result = dpr.CheckDprValidity();

            AssertInvalidResult(result,
                DataProcessingRegistrationValidationError.EnforcedInvalidity,
                DataProcessingRegistrationValidationError.MainContractNotActive);
        }

        [Fact]
        public void AssignOversightDate_Sets_OversightOptionId()
        {
            var oversightOptionId = A<int>();
            var dpr = new DataProcessingRegistration { IsOversightCompleted = YesNoUndecidedOption.Yes };

            var result = dpr.AssignOversightDate(A<DateTime>(), A<string>(), A<string>(), A<string>(), oversightOptionId);

            Assert.True(result.Ok);
            Assert.Equal(oversightOptionId, result.Value.OversightOptionId);
        }

        [Fact]
        public void ModifyOversightDateOptionId_Updates_OversightOptionId()
        {
            var oversightDateId = A<int>();
            var initialOversightOptionId = A<int>();
            var updatedOversightOptionId = A<int>();
            var dpr = new DataProcessingRegistration
            {
                OversightDates = new List<DataProcessingRegistrationOversightDate>
                {
                    new()
                    {
                        Id = oversightDateId,
                        OversightOptionId = initialOversightOptionId
                    }
                }
            };

            var result = dpr.ModifyOversightDateOptionId(oversightDateId, updatedOversightOptionId);

            Assert.True(result.Ok);
            Assert.Equal(updatedOversightOptionId, result.Value.OversightOptionId);
        }

        private static DataProcessingRegistration CreateDprWithMainContract(int contractId)
        {
            return new DataProcessingRegistration
            {
                MainContract = new ItContract
                {
                    Id = contractId
                }
            };
        }

        private static DataProcessingRegistration CreateDprWithActiveMainContract()
        {
            return new DataProcessingRegistration
            {
                MainContract = new ItContract(),
                EnforceInvalidity = false
            };
        }

        private static DataProcessingRegistration CreateDprWithInactiveMainContract()
        {
            return new DataProcessingRegistration
            {
                MainContract = new ItContract { ExpirationDate = DateTime.Now.AddDays(-10) },
                EnforceInvalidity = false
            };
        }

        private static DataProcessingRegistration CreateDprWithoutMainContract()
        {
            return new DataProcessingRegistration
            {
                MainContract = null,
                EnforceInvalidity = false
            };
        }

        private static void AssertValidResult(DataProcessingRegistrationValidationResult result)
        {
            Assert.True(result.Result);
            Assert.Empty(result.ValidationErrors);
        }

        private static void AssertInvalidResult(DataProcessingRegistrationValidationResult result, params DataProcessingRegistrationValidationError[] expectedErrors)
        {
            Assert.False(result.Result);
            Assert.Equal(expectedErrors.OrderBy(e => e), result.ValidationErrors.OrderBy(e => e));
        }
    }
}
