using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.Model
{
    public class ItContractTest : WithAutoFixture
    {
        [Fact]
        public void Can_ResetContractType()
        {
            //Arrange
            var sut = new ItContract()
            {
                ContractType = new ItContractType()
            };

            //Act
            sut.ResetContractType();

            //Assert
            Assert.Null(sut.ContractType);
        }

        [Fact]
        public void Can_ResetContractTemplate()
        {
            //Arrange
            var sut = new ItContract()
            {
                ContractTemplate = new ItContractTemplateType()
            };

            //Act
            sut.ResetContractTemplate();

            //Assert
            Assert.Null(sut.ContractTemplate);
        }

        [Fact]
        public void Can_UpdateContractValidityPeriod()
        {
            //Arrange
            var sut = new ItContract();
            var validFrom = A<DateTime>();
            var validTo = validFrom.AddDays(1);

            //Act
            var result = sut.UpdateContractValidityPeriod(validFrom, validTo);

            //Assert
            Assert.True(result.IsNone);
            Assert.NotNull(sut.Concluded);
            Assert.Equal(validFrom.Date, sut.Concluded.Value.Date);
            Assert.NotNull(sut.ExpirationDate);
            Assert.Equal(validTo.Date, sut.ExpirationDate.Value.Date);
        }

        [Fact]
        public void Cannot_UpdateContractValidityPeriod_If_ValidTo_Is_Before_ValidFrom()
        {
            //Arrange
            var sut = new ItContract();
            var validFrom = A<DateTime>();
            var validTo = validFrom.AddDays(-1);

            //Act
            var result = sut.UpdateContractValidityPeriod(validFrom, validTo);

            //Assert
            Assert.True(result.HasValue);
            Assert.Contains("ValidTo must equal or proceed ValidFrom", result.Value.Message.GetValueOrEmptyString());
            Assert.Equal(OperationFailure.BadInput, result.Value.FailureType);
        }

        [Fact]
        public void Can_SetAgreementElements()
        {
            //Arrange
            var sut = new ItContract();
            var agreementElement = new AgreementElementType()
            {
                Uuid = A<Guid>(),
                Id = A<int>()
            };

            //Act
            var result = sut.SetAgreementElements(new List<AgreementElementType> { agreementElement });

            //Assert
            Assert.True(result.IsNone);
            var agreementElementSet = Assert.Single(sut.AssociatedAgreementElementTypes);
            Assert.Equal(agreementElement.Uuid, agreementElementSet.AgreementElementType.Uuid);
            Assert.Equal(agreementElement.Id, agreementElementSet.AgreementElementType.Id);
        }

        [Fact]
        public void Cannot_SetAgreementElements_If_Duplicats()
        {
            //Arrange
            var sut = new ItContract();
            var agreementElement = new AgreementElementType()
            {
                Uuid = A<Guid>(),
                Id = A<int>()
            };

            //Act
            var result = sut.SetAgreementElements(new List<AgreementElementType> { agreementElement, agreementElement });

            //Assert
            Assert.True(result.HasValue);
            Assert.Contains("agreement elements must not contain duplicates", result.Value.Message.GetValueOrEmptyString());
            Assert.Equal(OperationFailure.BadInput, result.Value.FailureType);
        }

        [Fact]
        public void Can_ClearParent()
        {
            //Arrange
            var sut = new ItContract()
            {
                Parent = new ItContract()
            };

            //Act
            sut.ClearParent();

            //Assert
            Assert.Null(sut.Parent);
        }

        [Fact]
        public void Can_SetParent()
        {
            //Arrange
            var orgId = A<int>();
            var sut = new ItContract
            {
                Id = A<int>(),
                OrganizationId = orgId
            };
            var parent = new ItContract
            {
                Id = A<int>(),
                OrganizationId = orgId
            };

            //Act
            var result = sut.SetParent(parent);

            //Assert
            Assert.True(result.IsNone);
            Assert.Equal(parent.Id, sut.Parent.Id);
        }

        [Fact]
        public void Cannot_SetParent_If_Not_Same_Organization()
        {
            //Arrange
            var sut = new ItContract
            {
                Id = A<int>(),
                OrganizationId = A<int>()
            };
            var parent = new ItContract
            {
                Id = A<int>(),
                OrganizationId = A<int>()
            };

            //Act
            var result = sut.SetParent(parent);

            //Assert
            Assert.True(result.HasValue);
            Assert.Contains("Parent and child contracts must be in same organization", result.Value.Message.GetValueOrEmptyString());
            Assert.Equal(OperationFailure.BadInput, result.Value.FailureType);
        }

        [Fact]
        public void Can_SetResponsibleOrganizationUnit()
        {
            //Arrange
            var orgUnit = new OrganizationUnit()
            {
                Uuid = A<Guid>()
            };
            var organization = new Organization()
            {
                OrgUnits = new List<OrganizationUnit>()
                {
                    orgUnit
                }
            };
            var sut = new ItContract
            {
                Id = A<int>(),
                Organization = organization
            };

            //Act
            var result = sut.SetResponsibleOrganizationUnit(orgUnit.Uuid);

            //Assert
            Assert.True(result.IsNone);
            Assert.Equal(orgUnit.Uuid, sut.ResponsibleOrganizationUnit.Uuid);
        }

        [Fact]
        public void Cannot_SetResponsibleOrganizationUnit_If_Not_Part_Of_Organizations_OrgUnits()
        {
            //Arrange
            var orgUnit = new OrganizationUnit()
            {
                Uuid = A<Guid>()
            };
            var organization = new Organization()
            {
                OrgUnits = new List<OrganizationUnit>()
            };
            var sut = new ItContract
            {
                Id = A<int>(),
                Organization = organization
            };

            //Act
            var result = sut.SetResponsibleOrganizationUnit(orgUnit.Uuid);

            //Assert
            Assert.True(result.HasValue);
            Assert.Contains("UUID of responsible organization unit does not match an organization unit on this contract's organization", result.Value.Message.GetValueOrEmptyString());
            Assert.Equal(OperationFailure.BadInput, result.Value.FailureType);
        }

        [Fact]
        public void Can_ResetResponsibleOrganizationUnit()
        {
            //Arrange
            var sut = new ItContract()
            {
                ResponsibleOrganizationUnit = new OrganizationUnit()
            };

            //Act
            sut.ResetResponsibleOrganizationUnit();

            //Assert
            Assert.Null(sut.ResponsibleOrganizationUnit);
        }

        [Fact]
        public void Can_ResetProcurementStrategy()
        {
            //Arrange
            var sut = new ItContract()
            {
                ProcurementStrategy = new ProcurementStrategyType()
            };

            //Act
            sut.ResetProcurementStrategy();

            //Assert
            Assert.Null(sut.ProcurementStrategy);
        }

        [Fact]
        public void Can_ResetPurchaseForm()
        {
            //Arrange
            var sut = new ItContract()
            {
                PurchaseForm = new PurchaseFormType()
            };

            //Act
            sut.ResetPurchaseForm();

            //Assert
            Assert.Null(sut.PurchaseForm);
        }

        [Fact]
        public void Can_ResetProcurementPlan()
        {
            //Arrange
            var sut = new ItContract()
            {
                ProcurementPlanQuarter = A<int>(),
                ProcurementPlanYear = A<int>()
            };

            //Act
            sut.ResetProcurementPlan();

            //Assert
            Assert.Null(sut.ProcurementPlanQuarter);
            Assert.Null(sut.ProcurementPlanYear);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void Can_UpdateProcurementPlan(int quarterValue)
        {
            //Arrange
            var sut = new ItContract();
            var quarter = Convert.ToByte(quarterValue);
            var year = A<int>();

            //Act
            var result = sut.UpdateProcurementPlan((quarter, year));

            //Assert
            Assert.True(result.IsNone);
            Assert.NotNull(sut.ProcurementPlanQuarter);
            Assert.Equal(quarterValue, sut.ProcurementPlanQuarter);
            Assert.NotNull(sut.ProcurementPlanYear);
            Assert.Equal(year, sut.ProcurementPlanYear);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(5)]
        public void Cannot_UpdateProcurementPlan_If_Quarter_Not_Between_1_And_4(int halfValue)
        {
            //Arrange
            var sut = new ItContract();
            var half = Convert.ToByte(halfValue);
            var year = A<int>();

            //Act
            var result = sut.UpdateProcurementPlan((half, year));

            //Assert
            Assert.True(result.HasValue);
            Assert.Contains("Quarter Of Year has to be either 1, 2, 3 or 4", result.Value.Message.GetValueOrEmptyString());
            Assert.Equal(OperationFailure.BadInput, result.Value.FailureType);
        }

        [Fact]
        public void Can_AssignSystemUsage()
        {
            //Arrange
            var orgId = A<int>();
            var sut = new ItContract
            {
                OrganizationId = orgId
            };
            var usage = new ItSystemUsage
            {
                Id = A<int>(),
                OrganizationId = orgId
            };

            //Act
            var result = sut.AssignSystemUsage(usage);

            //Assert
            Assert.True(result.IsNone);
            var assignedUsage = Assert.Single(sut.AssociatedSystemUsages);
            Assert.Equal(usage.Id, assignedUsage.ItSystemUsage.Id);
        }

        [Fact]
        public void Cannot_AssignSystemUsage_If_Not_In_Same_Org_As_Contract()
        {
            //Arrange
            var sut = new ItContract
            {
                OrganizationId = A<int>()
            };
            var usage = new ItSystemUsage
            {
                Id = A<int>(),
                OrganizationId = A<int>()
            };

            //Act
            var result = sut.AssignSystemUsage(usage);

            //Assert
            Assert.True(result.HasValue);
            Assert.Contains("Cannot assign It System Usage to Contract within different Organization", result.Value.Message.GetValueOrEmptyString());
            Assert.Equal(OperationFailure.BadInput, result.Value.FailureType);
        }

        [Fact]
        public void Cannot_AssignSystemUsage_If_Already_Assigned()
        {
            //Arrange
            var orgId = A<int>();
            var usage = new ItSystemUsage
            {
                Id = A<int>(),
                OrganizationId = orgId
            };
            var sut = new ItContract
            {
                OrganizationId = orgId,
                AssociatedSystemUsages = new List<ItContractItSystemUsage>()
                {
                    new ItContractItSystemUsage()
                    {
                        ItSystemUsageId = usage.Id
                    }
                }
            };

            //Act
            var result = sut.AssignSystemUsage(usage);

            //Assert
            Assert.True(result.HasValue);
            Assert.Contains($"It System Usage with Id: {usage.Id}, already assigned to Contract", result.Value.Message.GetValueOrEmptyString());
            Assert.Equal(OperationFailure.Conflict, result.Value.FailureType);
        }

        [Fact]
        public void Can_RemoveSystemUsage()
        {
            //Arrange
            var orgId = A<int>();
            var usage = new ItSystemUsage
            {
                Id = A<int>(),
                OrganizationId = orgId
            };
            var sut = new ItContract
            {
                OrganizationId = orgId,
                AssociatedSystemUsages = new List<ItContractItSystemUsage>()
                {
                    new ItContractItSystemUsage()
                    {
                        ItSystemUsageId = usage.Id
                    }
                }
            };

            //Act
            var result = sut.RemoveSystemUsage(usage);

            //Assert
            Assert.True(result.IsNone);
            Assert.Empty(sut.AssociatedSystemUsages);
        }

        [Fact]
        public void Can_AssignDataProcessingRegistration()
        {
            //Arrange
            var orgId = A<int>();
            var sut = new ItContract
            {
                OrganizationId = orgId
            };
            var dpr = new DataProcessingRegistration
            {
                Id = A<int>(),
                OrganizationId = orgId
            };

            //Act
            var result = sut.AssignDataProcessingRegistration(dpr);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(dpr.Id, result.Value.Id);
            var assignedDpr = Assert.Single(sut.DataProcessingRegistrations);
            Assert.Equal(dpr.Id, assignedDpr.Id);
        }

        [Fact]
        public void Cannot_AssignDataProcessingRegistration_If_Not_In_Same_Org()
        {
            //Arrange
            var sut = new ItContract
            {
                OrganizationId = A<int>()
            };
            var dpr = new DataProcessingRegistration
            {
                Id = A<int>(),
                OrganizationId = A<int>()
            };

            //Act
            var result = sut.AssignDataProcessingRegistration(dpr);

            //Assert
            Assert.True(result.Failed);
            Assert.Contains("Cannot assign Data Processing Registration to Contract within different Organization", result.Error.Message.GetValueOrEmptyString());
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }



        [Fact]
        public void Cannot_AssignDataProcessingRegistration_If_Already_Assigned()
        {
            //Arrange
            var orgId = A<int>();
            var dpr = new DataProcessingRegistration
            {
                Id = A<int>(),
                OrganizationId = orgId
            };
            var sut = new ItContract
            {
                OrganizationId = orgId,
                DataProcessingRegistrations = new List<DataProcessingRegistration>()
                {
                    dpr
                }
            };

            //Act
            var result = sut.AssignDataProcessingRegistration(dpr);

            //Assert
            Assert.True(result.Failed);
            Assert.Contains("Data processing registration is already assigned", result.Error.Message.GetValueOrEmptyString());
            Assert.Equal(OperationFailure.Conflict, result.Error.FailureType);
        }

        [Fact]
        public void Can_RemoveDataProcessingRegistration()
        {
            //Arrange
            var orgId = A<int>();
            var dpr = new DataProcessingRegistration
            {
                Id = A<int>(),
                OrganizationId = orgId
            };
            var sut = new ItContract
            {
                OrganizationId = orgId,
                DataProcessingRegistrations = new List<DataProcessingRegistration>()
                {
                    dpr
                }
            };

            //Act
            var result = sut.RemoveDataProcessingRegistration(dpr);

            //Assert
            Assert.True(result.Ok);
            Assert.Empty(sut.DataProcessingRegistrations);
        }

        [Fact]
        public void Cannot_RemoveDataProcessingRegistration_If_Not_Assigned()
        {
            //Arrange
            var orgId = A<int>();
            var dpr = new DataProcessingRegistration
            {
                Id = A<int>(),
                OrganizationId = orgId
            };
            var sut = new ItContract
            {
                OrganizationId = orgId
            };

            //Act
            var result = sut.RemoveDataProcessingRegistration(dpr);

            //Assert
            Assert.True(result.Failed);
            Assert.Contains("Data processing registration not assigned", result.Error.Message.GetValueOrEmptyString());
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void Can_ResetPaymentFrequency()
        {
            //Arrange
            var sut = new ItContract()
            {
                PaymentFreqency = new PaymentFreqencyType()
            };

            //Act
            sut.ResetPaymentFrequency();

            //Assert
            Assert.Null(sut.PaymentFreqency);
        }

        [Fact]
        public void Can_ResetPaymentModel()
        {
            //Arrange
            var sut = new ItContract()
            {
                PaymentModel = new PaymentModelType()
            };

            //Act
            sut.ResetPaymentModel();

            //Assert
            Assert.Null(sut.PaymentModel);
        }

        [Fact]
        public void Can_ResetPriceRegulation()
        {
            //Arrange
            var sut = new ItContract()
            {
                PriceRegulation = new PriceRegulationType()
            };

            //Act
            sut.ResetPriceRegulation();

            //Assert
            Assert.Null(sut.PriceRegulation);
        }

        [Fact]
        public void Can_ResetNoticePeriod()
        {
            //Arrange
            var sut = new ItContract
            {
                TerminationDeadline = new TerminationDeadlineType()
            };

            //Act
            sut.ResetNoticePeriod();

            //Assert
            Assert.Null(sut.TerminationDeadline);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Validate_Returns_Error_If_Start_Date_Has_Not_Passed(bool enforceValid)
        {
            //Arrange
            var now = CreateValidDate();
            var sut = new ItContract()
            {
                Concluded = now.AddDays(1),
                Active = enforceValid
            };

            //Act
            var result = sut.Validate(now);

            //Assert
            Assert.Equal(enforceValid, result.Result);//If not enforced valid we expect the value to be false
            Assert.Equal(enforceValid, result.EnforcedValid);
            var validationError = Assert.Single(result.ValidationErrors);
            Assert.Equal(ItContractValidationError.StartDateNotPassed, validationError);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Validate_Returns_Error_If_End_Date_Has_Passed(bool enforceValid)
        {
            //Arrange
            var now = CreateValidDate();
            var sut = new ItContract()
            {
                ExpirationDate = now.AddDays(-1),
                Active = enforceValid
            };

            //Act
            var result = sut.Validate(now);

            //Assert
            Assert.Equal(enforceValid, result.Result);//If not enforced valid we expect the value to be false
            Assert.Equal(enforceValid, result.EnforcedValid);
            var validationError = Assert.Single(result.ValidationErrors);
            Assert.Equal(ItContractValidationError.EndDatePassed, validationError);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Validate_Returns_Error_If_Termination_Deadline_Has_Passed_With_No_TerminationPeriod(bool enforceValid)
        {
            //Arrange
            var now = CreateValidDate();
            var sut = new ItContract()
            {
                Terminated = now.AddDays(-1),
                Active = enforceValid
            };

            //Act
            var result = sut.Validate(now);

            //Assert
            Assert.Equal(enforceValid, result.Result);//If not enforced valid we expect the value to be false
            Assert.Equal(enforceValid, result.EnforcedValid);
            var validationError = Assert.Single(result.ValidationErrors);
            Assert.Equal(ItContractValidationError.TerminationPeriodExceeded, validationError);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Validate_Returns_Error_If_Termination_Deadline_Has_Passed_With_And_TerminationPeriod_Passed(bool enforceValid)
        {
            //Arrange
            var now = CreateValidDate();
            var terminationDeadline = new Random(A<int>()).Next(1, 12);
            var sut = new ItContract()
            {
                Terminated = now.AddDays(-1).AddMonths(-1 * terminationDeadline),
                TerminationDeadline = new TerminationDeadlineType
                {
                    Name = terminationDeadline.ToString("D")
                },
                Active = enforceValid
            };

            //Act
            var result = sut.Validate(now);

            //Assert
            Assert.Equal(enforceValid, result.Result);//If not enforced valid we expect the value to be false
            Assert.Equal(enforceValid, result.EnforcedValid);
            var validationError = Assert.Single(result.ValidationErrors);
            Assert.Equal(ItContractValidationError.TerminationPeriodExceeded, validationError);
        }

        [Theory]
        [InlineData(true, 0)]
        [InlineData(false, 0)]
        [InlineData(false, 1)]
        public void Validate_Returns_Success_If_Termination_Deadline_Has_Not_Passed(bool enforceValid, int dayOffset)
        {
            //Arrange
            var now = CreateValidDate();
            var sut = new ItContract
            {
                Terminated = now.AddDays(dayOffset),
                Active = enforceValid
            };

            //Act
            var result = sut.Validate(now);

            //Assert
            Assert.True(result.Result);//If not enforced valid we expect the value to be false
            Assert.Equal(enforceValid, result.EnforcedValid);
            Assert.Empty(result.ValidationErrors);
        }

        [Theory]
        [InlineData(true, 0)]
        [InlineData(false, 0)]
        [InlineData(false, 1)]
        public void Validate_Returns_Success_If_Termination_Deadline_Passed_But_TerminationPeriod_Has_Not_Passed(bool enforceValid, int dayOffset)
        {
            //Arrange
            var validDate = CreateValidDate();

            //make sure the day is present in every month, so there is no month "conversion" error
            //e.g. when subtracting a month from 31.10 the result would be 30.09 which would cause an error (notice the day difference)
            var randomDay = new Random(A<int>()).Next(1, 28);
            var now = new DateTime(validDate.Year, validDate.Month, randomDay);
            var terminationDeadline = new Random(A<int>()).Next(1, 12);

            var sut = new ItContract
            {
                Terminated = now.AddMonths(-1 * terminationDeadline).AddDays(dayOffset),
                Active = enforceValid,
                TerminationDeadline = new TerminationDeadlineType
                {
                    Name = terminationDeadline.ToString("D")
                }
            };

            //Act
            var result = sut.Validate(now);

            //Assert
            Assert.True(result.Result);//If not enforced valid we expect the value to be false
            Assert.Equal(enforceValid, result.EnforcedValid);
            Assert.Empty(result.ValidationErrors);
        }

        [Theory]
        [InlineData(true, 0)]
        [InlineData(false, 0)]
        [InlineData(false, -1)]
        public void Validate_Returns_Success_If_Start_Date_Has_Passed(bool enforceValid, int dayOffset)
        {
            //Arrange
            var now = CreateValidDate();
            var sut = new ItContract
            {
                Concluded = now.AddDays(dayOffset),
                ExpirationDate = now,
                Active = enforceValid
            };

            //Act
            var result = sut.Validate(now);

            //Assert
            Assert.True(result.Result);//If not enforced valid we expect the value to be false
            Assert.Equal(enforceValid, result.EnforcedValid);
            Assert.Empty(result.ValidationErrors);
        }

        [Theory]
        [InlineData(true, 0)]
        [InlineData(false, 0)]
        [InlineData(false, 1)]
        public void Validate_Returns_Success_If_End_Date_Has_Not_Passed(bool enforceValid, int dayOffset)
        {
            //Arrange
            var now = CreateValidDate();
            var sut = new ItContract
            {
                ExpirationDate = now.AddDays(dayOffset),
                Active = enforceValid
            };

            //Act
            var result = sut.Validate(now);

            //Assert
            Assert.True(result.Result);//If not enforced valid we expect the value to be false
            Assert.Equal(enforceValid, result.EnforcedValid);
            Assert.Empty(result.ValidationErrors);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Should_Invalidate_Contract_When_Valid_Parent_Is_Required_But_Parent_Is_Invalid(bool enforceValid)
        {
            var now = DateTime.Now;
            var invalidParent = new ItContract { ExpirationDate = now.AddDays(-1)};
            var sut = new ItContract { Parent = invalidParent, RequireValidParent = true, Active = enforceValid};

            var result = sut.Validate(now);

            Assert.Equal(enforceValid, result.Result);
            var error = Assert.Single(result.ValidationErrors);
            Assert.Equal(ItContractValidationError.InvalidParentContract, error);
        }

        [Fact]
        public void Can_Get_All_Payments()
        {
            var externalEconomyId = A<int>();
            var internalEconomyId = A<int>();
            var contract = new ItContract()
            {
                ExternEconomyStreams = new List<EconomyStream>
                {
                    new()
                    {
                        Id = externalEconomyId
                    }
                },
                InternEconomyStreams = new List<EconomyStream>
                {
                    new()
                    {
                        Id = internalEconomyId
                    }
                }
            };
            const int expectedNumberOfPayments = 2;

            var result = contract.GetAllPayments().ToList();

            Assert.Equal(expectedNumberOfPayments, result.Count);
            Assert.Contains(externalEconomyId, result.Select(x => x.Id));
            Assert.Contains(internalEconomyId, result.Select(x => x.Id));
        }

        [Fact]
        public void Can_Get_Internal_Payments_By_UnitId()
        {
            var internalEconomyId = A<int>();
            var unitId = A<int>();
            var contract = new ItContract()
            {
                InternEconomyStreams = new List<EconomyStream>
                {
                    new()
                    {
                        Id = internalEconomyId,
                        OrganizationUnitId = unitId
                    },
                    new()
                    {
                        Id = A<int>(),
                        OrganizationUnitId = A<int>()
                    }
                }
            };

            var result = contract.GetInternalPaymentsForUnit(unitId).ToList();

            Assert.Single(result);
            Assert.Contains(internalEconomyId, result.Select(x => x.Id));
            Assert.Contains(unitId, result.Select(x => x.OrganizationUnitId));
        }

        [Fact]
        public void Can_Get_External_Payments_By_UnitId()
        {
            var externalEconomyId = A<int>();
            var unitId = A<int>();
            var contract = new ItContract()
            {
                ExternEconomyStreams = new List<EconomyStream>
                {
                    new()
                    {
                        Id = externalEconomyId,
                        OrganizationUnitId = unitId
                    },
                    new()
                    {
                        Id = A<int>(),
                        OrganizationUnitId = A<int>()
                    }
                }
            };

            var result = contract.GetExternalPaymentsForUnit(unitId).ToList();

            Assert.Single(result);
            Assert.Contains(externalEconomyId, result.Select(x => x.Id));
            Assert.Contains(unitId, result.Select(x => x.OrganizationUnitId));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Reset_EconomyStream(bool isInternal)
        {
            var id = A<int>();
            var unit = new OrganizationUnit { Id = A<int>() };
            var contract = new ItContract();
            if (isInternal)
            {
                contract.InternEconomyStreams = new List<EconomyStream>
                {
                    new()
                    {
                        Id = id,
                        OrganizationUnit = unit,
                    }
                };
            }
            else
            {
                contract.ExternEconomyStreams = new List<EconomyStream>
                {
                    new()
                    {
                        Id = id,
                        OrganizationUnit = unit,
                    }
                };
            }

            var error = contract.ResetEconomyStreamOrganizationUnit(id, isInternal);

            Assert.True(error.IsNone);
            if (isInternal)
            {
                var intern = contract.InternEconomyStreams.FirstOrDefault();
                Assert.NotNull(intern);
                Assert.Null(intern.OrganizationUnit);
            }
            else
            {
                var external = contract.ExternEconomyStreams.FirstOrDefault();
                Assert.NotNull(external);
                Assert.Null(external.OrganizationUnit);
            }
        }

        [Fact]
        public void Remove_EconomyStream_Returns_NotFound()
        {
            var id = A<int>();
            var contract = new ItContract();
            var expectedErrorMessage = $"EconomyStream with id: {id} was not found";

            var error = contract.ResetEconomyStreamOrganizationUnit(id, A<bool>());

            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.NotFound, error.Value.FailureType);
            Assert.Equal(expectedErrorMessage, error.Value.Message);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Transfer_EconomyStream(bool isInternal)
        {
            var id = A<int>();
            var unit = new OrganizationUnit { Id = A<int>() };
            var targetUnit = new OrganizationUnit { Uuid = A<Guid>() };
            var contract = new ItContract()
            {
                Organization = new Organization()
                {
                    OrgUnits = new List<OrganizationUnit>()
                    {
                        targetUnit
                    }
                }
            };
            var economyStream = new EconomyStream
            {
                Id = id,
                OrganizationUnit = unit,
            };

            if (isInternal)
            {
                contract.InternEconomyStreams = new List<EconomyStream> { economyStream };
            }
            else
            {
                contract.ExternEconomyStreams = new List<EconomyStream> { economyStream };
            }

            var error = contract.TransferEconomyStream(id, targetUnit.Uuid, isInternal);

            Assert.True(error.IsNone);
            if (isInternal)
            {
                var intern = contract.InternEconomyStreams.FirstOrDefault();
                Assert.NotNull(intern);
                Assert.Equal(intern.OrganizationUnit.Uuid, targetUnit.Uuid);
            }
            else
            {
                var external = contract.ExternEconomyStreams.FirstOrDefault();
                Assert.NotNull(external);
                Assert.Equal(external.OrganizationUnit.Uuid, targetUnit.Uuid);
            }
        }


        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Transfer_EconomyStream_Returns_NotFound(bool isInternal)
        {
            var id = A<int>();
            var contract = new ItContract() { Organization = new Organization() };

            var error = contract.TransferEconomyStream(id, A<Guid>(), isInternal);

            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.NotFound, error.Value.FailureType);
        }

        private DateTime CreateValidDate()
        {
            return DateTime.Now.Date.AddMonths(new Random(A<int>()).Next(-30, 30));
        }
    }
}
