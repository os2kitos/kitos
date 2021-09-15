using System;
using System.Collections.Generic;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
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
            var result = sut.SetAgreementElements(new List<AgreementElementType>{agreementElement});

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
                ProcurementPlanHalf = A<int>(),
                ProcurementPlanYear = A<int>()
            };

            //Act
            sut.ResetProcurementPlan();

            //Assert
            Assert.Null(sut.ProcurementPlanHalf);
            Assert.Null(sut.ProcurementPlanYear);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void Can_UpdateProcurementPlan(int halfValue)
        {
            //Arrange
            var sut = new ItContract();
            var half = Convert.ToByte(halfValue);
            var year = A<int>();

            //Act
            var result = sut.UpdateProcurementPlan((half, year));

            //Assert
            Assert.True(result.IsNone);
            Assert.NotNull(sut.ProcurementPlanHalf);
            Assert.Equal(halfValue, sut.ProcurementPlanHalf);
            Assert.NotNull(sut.ProcurementPlanYear);
            Assert.Equal(year, sut.ProcurementPlanYear);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(3)]
        public void Cannot_UpdateProcurementPlan_If_Half_Not_1_Or_2(int halfValue)
        {
            //Arrange
            var sut = new ItContract();
            var half = Convert.ToByte(halfValue);
            var year = A<int>();

            //Act
            var result = sut.UpdateProcurementPlan((half, year));

            //Assert
            Assert.True(result.HasValue);
            Assert.Contains("Half Of Year has to be either 1 or 2", result.Value.Message.GetValueOrEmptyString());
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
    }
}
