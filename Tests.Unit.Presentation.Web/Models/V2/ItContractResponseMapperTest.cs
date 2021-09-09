using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Presentation.Web.Controllers.API.V2.External.ItContracts.Mapping;
using Presentation.Web.Models.API.V2.Response.Contract;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.Types.Contract;
using Presentation.Web.Models.API.V2.Types.Shared;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    public class ItContractResponseMapperTest : WithAutoFixture
    {
        private readonly ItContractResponseMapper _sut;

        public ItContractResponseMapperTest()
        {
            _sut = new ItContractResponseMapper();
        }

        [Fact]
        public void MapContractDTO_Maps_Root_Properties()
        {
            //Arrange
            var contract = CreateContract();
            AssignBasicProperties(contract);

            //Act
            var dto = _sut.MapContractDTO(contract);

            //Assert
            Assert.Equal(contract.Name, dto.Name);
            Assert.Equal(contract.Uuid, dto.Uuid);
            Assert.Equal(contract.LastChanged, dto.LastModified);
            AssertIdentity(contract.Parent, dto.ParentContract);
            AssertUser(contract.ObjectOwner, dto.CreatedBy);
            AssertUser(contract.LastChangedByUser, dto.LastModifiedBy);
            AssertOrganization(contract.Organization, dto.OrganizationContext);
        }

        [Fact]
        public void MapContractDTO_Maps_No_Properties()
        {
            //Arrange
            var contract = new ItContract();

            //Act
            var dto = _sut.MapContractDTO(contract);

            //Assert
            Assert.Equal(contract.Uuid, dto.Uuid);
            Assert.Equal(contract.LastChanged, dto.LastModified);

            Assert.Null(dto.CreatedBy);
            Assert.Null(dto.LastModifiedBy);
            Assert.Null(dto.Name);
            Assert.Null(dto.OrganizationContext);
            Assert.Null(dto.ParentContract);
            Assert.Empty(dto.ExternalReferences);
            Assert.Empty(dto.SystemUsages);
            Assert.Empty(dto.Roles);

            var general = dto.General;
            Assert.Null(general.ContractId);
            Assert.Null(general.Notes);
            Assert.Null(general.ContractType);
            Assert.Null(general.ContractTemplate);
            Assert.Empty(general.AgreementElements);
            var validity = general.Validity;
            Assert.Equal(contract.IsActive, validity.Valid);
            Assert.False(validity.EnforcedValid);
            Assert.Null(validity.ValidFrom);
            Assert.Null(validity.ValidTo);

            var procurement = dto.Procurement;
            Assert.Null(procurement.ProcurementStrategy);
            Assert.Null(procurement.PurchaseType);
            Assert.Null(procurement.ProcurementPlan);

            var supplier = dto.Supplier;
            Assert.False(supplier.Signed);
            Assert.Null(supplier.SignedBy);
            Assert.Null(supplier.SignedAt);
            Assert.Null(supplier.SupplierOrganization);

            var responsible = dto.Responsible;
            Assert.False(responsible.Signed);
            Assert.Null(responsible.SignedBy);
            Assert.Null(responsible.SignedAt);
            Assert.Null(responsible.OrganizationUnit);

            Assert.Empty(dto.SystemUsages);

            Assert.Empty(dto.DataProcessingRegistrations);

            Assert.Empty(dto.HandoverTrials);

            var paymentModel = dto.PaymentModel;
            Assert.Null(paymentModel.OperationsRemunerationStartedAt);
            Assert.Null(paymentModel.PaymentModel);
            Assert.Null(paymentModel.PaymentFrequency);
            Assert.Null(paymentModel.PriceRegulation);
            Assert.Empty(paymentModel.PaymentMileStones);

            var agreementPeriod = dto.AgreementPeriod;
            Assert.Null(agreementPeriod.DurationMonths);
            Assert.Null(agreementPeriod.DurationYears);
            Assert.False(agreementPeriod.IsContinuous);
            Assert.Null(agreementPeriod.ExtensionOptions);
            Assert.Equal(0, agreementPeriod.ExtensionOptionsUsed);
            Assert.Null(agreementPeriod.IrrevocableUntil);

            var termination = dto.Termination;
            Assert.Null(termination.TerminatedAt);
            var terms = termination.Terms;
            Assert.Null(terms.NoticePeriodMonths);
            Assert.Null(terms.NoticeByEndOf);
            Assert.Null(terms.NoticePeriodExtendsCurrent);

            var payments = dto.Payments;
            Assert.Empty(payments.External);
            Assert.Empty(payments.Internal);

            Assert.Empty(dto.Roles);

            Assert.Empty(dto.ExternalReferences);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MapContractDTO_Maps_General_Properties(bool withOptionalCrossReferences)
        {
            //Arrange
            var contract = CreateContract();
            AssignBasicProperties(contract);
            AssignGeneralPropertiesSection(contract, withOptionalCrossReferences);

            //Act
            var dto = _sut.MapContractDTO(contract);

            //Arrange
            Assert.Equal(contract.ItContractId, dto.General.ContractId);
            Assert.Equal(contract.Note, dto.General.Notes);
            AssertOptionalIdentity(contract.ContractTemplate, dto.General.ContractTemplate);
            AssertOptionalIdentity(contract.ContractType, dto.General.ContractType);
            AssertAgreementElements(contract.AssociatedAgreementElementTypes, dto.General.AgreementElements);

            Assert.Equal(contract.Concluded, dto.General.Validity.ValidFrom);
            Assert.Equal(contract.ExpirationDate, dto.General.Validity.ValidTo);
            Assert.Equal(contract.Active, dto.General.Validity.EnforcedValid);
            Assert.Equal(contract.IsActive, dto.General.Validity.Valid);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MapContractDTO_Maps_Procurement_Properties(bool withOptionalCrossReferences)
        {
            //Arrange
            var contract = CreateContract();
            AssignBasicProperties(contract);
            AssignProcurementPropertiesSection(contract, withOptionalCrossReferences);

            //Act
            var dto = _sut.MapContractDTO(contract);

            //Arrange
            AssertOptionalIdentity(contract.ProcurementStrategy, dto.Procurement.ProcurementStrategy);
            AssertOptionalIdentity(contract.PurchaseForm, dto.Procurement.PurchaseType);
            if (withOptionalCrossReferences)
            {
                Assert.Equal(Convert.ToByte(contract.ProcurementPlanHalf.Value), dto.Procurement.ProcurementPlan.HalfOfYear);
                Assert.Equal(contract.ProcurementPlanYear.Value, dto.Procurement.ProcurementPlan.Year);
            }
            else
            {
                Assert.Null(dto.Procurement.ProcurementPlan);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MapContractDTO_Maps_Supplier_Properties(bool withOptionalCrossReferences)
        {
            //Arrange
            var contract = CreateContract();
            AssignBasicProperties(contract);
            AssignSupplierPropertiesSection(contract, withOptionalCrossReferences);

            //Act
            var dto = _sut.MapContractDTO(contract);

            //Arrange
            Assert.Equal(contract.SupplierContractSigner, dto.Supplier.SignedBy);
            Assert.Equal(contract.HasSupplierSigned, dto.Supplier.Signed);
            Assert.Equal(contract.SupplierSignedDate, dto.Supplier.SignedAt);
            if (withOptionalCrossReferences)
            {
                AssertOrganization(contract.Supplier, dto.Supplier.SupplierOrganization);
            }
            else
            {
                Assert.Null(dto.Supplier.SupplierOrganization);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MapContractDTO_Maps_Responsible_Properties(bool withOptionalCrossReferences)
        {
            //Arrange
            var contract = CreateContract();
            AssignBasicProperties(contract);
            AssignResponsiblePropertiesSection(contract, withOptionalCrossReferences);

            //Act
            var dto = _sut.MapContractDTO(contract);

            //Arrange
            Assert.Equal(contract.ContractSigner, dto.Responsible.SignedBy);
            Assert.Equal(contract.IsSigned, dto.Responsible.Signed);
            Assert.Equal(contract.SignedDate, dto.Responsible.SignedAt);
            if (withOptionalCrossReferences)
            {
                AssertIdentity(contract.ResponsibleOrganizationUnit, dto.Responsible.OrganizationUnit);
            }
            else
            {
                Assert.Null(dto.Responsible.OrganizationUnit);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MapContractDTO_Maps_SystemUsageUuids_Section(bool withSystemUsages)
        {
            //Arrange
            var contract = CreateContract();
            AssignBasicProperties(contract);
            if (withSystemUsages)
            {
                var usage1 = CreateSystemUsage();
                var usage2 = CreateSystemUsage();
                AssignSystemUsages(contract, new[] { usage1, usage2 });
            }
            else
            {
                AssignSystemUsages(contract, new ItSystemUsage[0]);
            }

            //Act
            var dto = _sut.MapContractDTO(contract);

            //Assert
            if (withSystemUsages)
            {
                AssertSystemUsages(contract, dto.SystemUsages.ToList());
            }
            else
            {
                Assert.Empty(dto.SystemUsages);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MapContractDTO_Maps_DataProcessingRegistrationUuids_Section(bool withDpr)
        {
            //Arrange
            var contract = CreateContract();
            AssignBasicProperties(contract);
            if (withDpr)
            {
                var dpr1 = CreateDPR();
                var dpr2 = CreateDPR();
                AssignDPRs(contract, new[] { dpr1, dpr2 });
            }
            else
            {
                AssignDPRs(contract, new DataProcessingRegistration[0]);
            }

            //Act
            var dto = _sut.MapContractDTO(contract);

            //Assert
            if (withDpr)
            {
                AssertIdentities(contract.DataProcessingRegistrations, dto.DataProcessingRegistrations.ToList());
            }
            else
            {
                Assert.Empty(dto.DataProcessingRegistrations);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MapContractDTO_Maps_HandoverTrials_Section(bool withHandoverTrials)
        {
            //Arrange
            var contract = CreateContract();
            AssignBasicProperties(contract);
            AssignHandoverTrialProperties(contract, withHandoverTrials);

            //Act
            var dto = _sut.MapContractDTO(contract);

            //Assert
            if (withHandoverTrials)
            {
                AssertHandoverTrials(contract, dto.HandoverTrials.ToList());
            }
            else
            {
                Assert.Empty(dto.HandoverTrials);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MapContractDTO_Maps_PaymentModels_Section(bool withOptionalCrossReferences)
        {
            //Arrange
            var contract = CreateContract();
            AssignBasicProperties(contract);
            AssignPaymentModelProperties(contract, withOptionalCrossReferences);

            //Act
            var dto = _sut.MapContractDTO(contract);

            //Assert
            Assert.Equal(contract.OperationRemunerationBegun, dto.PaymentModel.OperationsRemunerationStartedAt);
            AssertOptionalIdentity(contract.PaymentModel, dto.PaymentModel.PaymentModel);
            AssertOptionalIdentity(contract.PaymentFreqency, dto.PaymentModel.PaymentFrequency);
            AssertOptionalIdentity(contract.PriceRegulation, dto.PaymentModel.PriceRegulation);
            if (withOptionalCrossReferences)
            {
                AssertPaymentMilestones(contract, dto.PaymentModel.PaymentMileStones.ToList());
            }
            else
            {
                Assert.Empty(dto.PaymentModel.PaymentMileStones);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MapContractDTO_Maps_AgreementPeriods_Section(bool withOptionalCrossReferences)
        {
            //Arrange
            var contract = CreateContract();
            AssignBasicProperties(contract);
            AssignAgreementPeriodProperties(contract, withOptionalCrossReferences);

            //Act
            var dto = _sut.MapContractDTO(contract);

            //Assert
            Assert.Equal(contract.DurationYears, dto.AgreementPeriod.DurationYears);
            Assert.Equal(contract.DurationMonths, dto.AgreementPeriod.DurationMonths);
            Assert.Equal(contract.DurationOngoing, dto.AgreementPeriod.IsContinuous);
            AssertOptionalIdentity(contract.OptionExtend, dto.AgreementPeriod.ExtensionOptions);
            Assert.Equal(contract.ExtendMultiplier, dto.AgreementPeriod.ExtensionOptionsUsed);
            Assert.Equal(contract.IrrevocableTo, dto.AgreementPeriod.IrrevocableUntil);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MapContractDTO_Maps_Termination_Section(bool withOptionalCrossReferences)
        {
            //Arrange
            var contract = CreateContract();
            AssignBasicProperties(contract);
            AssignTerminationProperties(contract, withOptionalCrossReferences);

            //Act
            var dto = _sut.MapContractDTO(contract);

            //Assert
            Assert.Equal(contract.Terminated, dto.Termination.TerminatedAt);
            AssertOptionalIdentity(contract.TerminationDeadline, dto.Termination.Terms.NoticePeriodMonths);
            AssertYearSegment(contract.Running, dto.Termination.Terms.NoticeByEndOf);
            AssertYearSegment(contract.ByEnding, dto.Termination.Terms.NoticePeriodExtendsCurrent);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MapContractDTO_Maps_Payments_Section(bool withPayments)
        {
            //Arrange
            var contract = CreateContract();
            AssignBasicProperties(contract);
            AssignPaymentProperties(contract, withPayments);

            //Act
            var dto = _sut.MapContractDTO(contract);

            //Assert
            if (withPayments)
            {
                AssertPayments(contract.ExternEconomyStreams, dto.Payments.External.ToList());
                AssertPayments(contract.InternEconomyStreams, dto.Payments.Internal.ToList());
            }
            else
            {
                Assert.Empty(dto.Payments.External);
                Assert.Empty(dto.Payments.Internal);
            }
        }

        [Fact]
        public void MapContractDTO_Maps_Role_Assignment_Section()
        {
            //Arrange
            var contract = CreateContract();
            AssignBasicProperties(contract);
            AssignRoles(contract);

            //Act
            var dto = _sut.MapContractDTO(contract);

            //Assert
            var expected = contract.Rights.Select(right => new
            {
                roleId = right.Role.Uuid,
                roleName = right.Role.Name,
                userId = right.User.Uuid,
                userName = right.User.GetFullName()
            }).ToList();
            var actual = dto.Roles.Select(roleAssignment => new
            {
                roleId = roleAssignment.Role.Uuid,
                roleName = roleAssignment.Role.Name,
                userId = roleAssignment.User.Uuid,
                userName = roleAssignment.User.Name
            }).ToList();
            Assert.Equal(expected.Count, actual.Count);
            foreach (var comparison in expected.Zip(actual, (expectedEntry, actualEntry) => new { expectedEntry, actualEntry }).ToList())
            {
                Assert.Equal(comparison.expectedEntry.roleId, comparison.actualEntry.roleId);
                Assert.Equal(comparison.expectedEntry.roleName, comparison.actualEntry.roleName);
                Assert.Equal(comparison.expectedEntry.userId, comparison.actualEntry.userId);
                Assert.Equal(comparison.expectedEntry.userName, comparison.actualEntry.userName);
            }
        }

        [Fact]
        public void MapContractDTO_Maps_ExternalReferences_Properties_Section()
        {
            //Arrange
            var contract = CreateContract();
            AssignBasicProperties(contract);
            AssignExternalReferences(contract);

            //Act
            var dto = _sut.MapContractDTO(contract);

            //Assert
            AssertExternalReferences(contract, dto.ExternalReferences.ToList());
        }

        #region Creaters

        private DataProcessingRegistration CreateDPR()
        {
            return new()
            {
                Name = A<string>(),
                Uuid = A<Guid>()
            };
        }

        private ItSystemUsage CreateSystemUsage()
        {
            var systemId = A<int>();
            return new ItSystemUsage
            {
                Id = A<int>(),
                Uuid = A<Guid>(),
                ItSystemId = systemId,
                ItSystem = new ItSystem()
                {
                    Id = systemId,
                    Uuid = A<Guid>(),
                    Name = A<string>()
                }
            };
        }

        private User CreateUser()
        {
            return new User
            {
                Name = A<string>(),
                LastName = A<string>(),
                Uuid = A<Guid>()
            };
        }

        private OrganizationUnit CreateOrganizationUnit()
        {
            return new OrganizationUnit { Name = A<string>(), Uuid = A<Guid>() };
        }

        private Organization CreateOrganization()
        {
            return new Organization { Name = A<string>(), Cvr = A<string>(), Uuid = A<Guid>() };
        }

        private ItContract CreateContract()
        {
            return new()
            {
                Id = A<int>(),
                Name = A<string>(),
                Uuid = A<Guid>()
            };
        }

        private EconomyStream CreateEconomyStream()
        {
            return new ()
            {
                AccountingEntry = A<string>(),
                Acquisition = A<int>(),
                AuditDate = A<DateTime>(),
                AuditStatus = A<TrafficLight>(),
                Note = A<string>(),
                Operation = A<int>(),
                OrganizationUnit = CreateOrganizationUnit(),
                Other = A<int>()
            };
        }

        #endregion

        #region Assigners

        private void AssignRoles(ItContract contract)
        {
            var rights = Many<Guid>().Select(id => new ItContractRight()
            {
                User = CreateUser(),
                Role = new ItContractRole() { Name = A<string>(), Uuid = id }
            }).ToList();
            contract.Rights = rights;
        }

        private void AssignExternalReferences(ItContract contract)
        {
            contract.ExternalReferences = Many<string>().Select((title, i) => new ExternalReference
            {
                Title = title,
                URL = A<string>(),
                ExternalReferenceId = A<string>(),
                Id = i
            }).ToList();
            contract.Reference = contract.ExternalReferences.OrderBy(x => A<int>()).First();
        }

        private void AssignPaymentProperties(ItContract contract, bool withPayments)
        {
            contract.ExternEconomyStreams = withPayments
                ? new List<EconomyStream>()
                {
                    CreateEconomyStream()
                }
                : null;
            contract.InternEconomyStreams = withPayments
                ? new List<EconomyStream>()
                {
                    CreateEconomyStream()
                }
                : null;
        }

        private void AssignTerminationProperties(ItContract contract, bool withOptionalCrossReferences)
        {
            contract.Terminated = A<DateTime>();
            contract.TerminationDeadline = withOptionalCrossReferences
                ? new TerminationDeadlineType() { Uuid = A<Guid>(), Name = A<string>() }
                : null;
            contract.Running = A<YearSegmentOption>();
            contract.ByEnding = A<YearSegmentOption>();
        }

        private void AssignAgreementPeriodProperties(ItContract contract, bool withOptionalCrossReferences)
        {
            contract.DurationYears = A<int>();
            contract.DurationMonths = A<int>() % 11;
            contract.DurationOngoing = A<bool>();
            contract.OptionExtend = withOptionalCrossReferences 
                ? new OptionExtendType() { Uuid = A<Guid>(), Name = A<string>() }
                : null;
            contract.ExtendMultiplier = A<int>();
            contract.IrrevocableTo = A<DateTime>();
        }

        private void AssignPaymentModelProperties(ItContract contract, bool withOptionalCrossReferences)
        {
            contract.OperationRemunerationBegun = A<DateTime>();
            contract.PaymentModel = withOptionalCrossReferences
                ? new PaymentModelType() { Uuid = A<Guid>(), Name = A<string>() }
                : null;
            contract.PaymentFreqency = withOptionalCrossReferences
                ? new PaymentFreqencyType() { Uuid = A<Guid>(), Name = A<string>() }
                : null;
            contract.PriceRegulation = withOptionalCrossReferences
                ? new PriceRegulationType() { Uuid = A<Guid>(), Name = A<string>() }
                : null;
            contract.PaymentMilestones = withOptionalCrossReferences
                ? new List<PaymentMilestone>()
                {
                    new PaymentMilestone()
                    {
                        Title = A<string>(),
                        Approved = A<DateTime>(),
                        Expected = A<DateTime>()
                    }
                }
                : null;
        }

        private void AssignHandoverTrialProperties(ItContract contract, bool withHandoverTrials)
        {
            contract.HandoverTrials = withHandoverTrials
                ? new List<HandoverTrial>()
                {
                    new HandoverTrial()
                    {
                        HandoverTrialType = new HandoverTrialType(){ Uuid = A<Guid>(), Name = A<string>() },
                        Approved = A<DateTime>(),
                        Expected = A<DateTime>(),
                        ItContract = contract
                    }
                }
                : new List<HandoverTrial>(0);
        }

        private void AssignDPRs(ItContract contract, DataProcessingRegistration[] dprs)
        {
            contract.DataProcessingRegistrations = dprs;
        }

        private void AssignSystemUsages(ItContract contract, ItSystemUsage[] usages)
        {
            contract.AssociatedSystemUsages = usages.Select(usage => new ItContractItSystemUsage() {ItContract = contract, ItSystemUsage = usage} ).ToList();
        }

        private void AssignResponsiblePropertiesSection(ItContract contract, bool withOptionalCrossReferences)
        {
            contract.ContractSigner = A<string>();
            contract.IsSigned = A<bool>();
            contract.SignedDate = A<DateTime>();
            contract.ResponsibleOrganizationUnit = withOptionalCrossReferences
                ? CreateOrganizationUnit()
                : null;
        }

        private void AssignSupplierPropertiesSection(ItContract contract, bool withOptionalCrossReferences)
        {
            contract.SupplierContractSigner = A<string>();
            contract.HasSupplierSigned = A<bool>();
            contract.SupplierSignedDate = A<DateTime>();
            contract.Supplier = withOptionalCrossReferences 
                ? CreateOrganization()
                : null;
        }

        private void AssignProcurementPropertiesSection(ItContract contract, bool withOptionalCrossReferences)
        {
            contract.ProcurementStrategy = withOptionalCrossReferences
                ? new ProcurementStrategyType() { Uuid = A<Guid>(), Name = A<string>() }
                : null;
            contract.PurchaseForm = withOptionalCrossReferences
                ? new PurchaseFormType() { Uuid = A<Guid>(), Name = A<string>() }
                : null;
            contract.ProcurementPlanHalf = withOptionalCrossReferences 
                ? A<int>() % 2 
                : null;
            contract.ProcurementPlanYear = withOptionalCrossReferences 
                ? A<int>() 
                : null;
        }

        private void AssignGeneralPropertiesSection(ItContract contract, bool withOptionalCrossReferences)
        {
            contract.ItContractId = A<string>();
            contract.Note = A<string>();
            contract.ContractTemplate = withOptionalCrossReferences
                ? new ItContractTemplateType() { Uuid = A<Guid>(), Name = A<string>() }
                : null;
            contract.ContractType = withOptionalCrossReferences
                ? new ItContractType() { Uuid = A<Guid>(), Name = A<string>() }
                : null;
            contract.AssociatedAgreementElementTypes = withOptionalCrossReferences
                ? new List<ItContractAgreementElementTypes>()
                {
                    new ItContractAgreementElementTypes()
                    {
                        ItContract = contract,
                        AgreementElementType = new AgreementElementType() { Uuid = A<Guid>(), Name = A<string>() }
                    }
                }
                : null;
            contract.Active = A<bool>();
            contract.Concluded = A<DateTime>();
            contract.ExpirationDate = A<DateTime>();
        }

        private void AssignBasicProperties(ItContract contract)
        {
            contract.LastChanged = A<DateTime>();
            contract.ObjectOwner = CreateUser();
            contract.LastChangedByUser = CreateUser();
            contract.Organization = CreateOrganization();
            contract.Parent = CreateContract();
        }

        #endregion

        #region Asserters

        private static void AssertExternalReferences(ItContract contract, List<ExternalReferenceDataDTO> dtoExternalReferences)
        {
            var actualMaster = Assert.Single(dtoExternalReferences, reference => reference.MasterReference);
            AssertExternalReference(contract.Reference, actualMaster);
            Assert.Equal(contract.ExternalReferences.Count, dtoExternalReferences.Count);

            foreach (var comparison in contract.ExternalReferences.OrderBy(x => x.Title)
                .Zip(dtoExternalReferences.OrderBy(x => x.Title), (expected, actual) => new { expected, actual })
                .ToList())
            {
                AssertExternalReference(comparison.expected, comparison.actual);
            }
        }

        private static void AssertExternalReference(ExternalReference reference, ExternalReferenceDataDTO actualMaster)
        {
            Assert.Equal(reference.Title, actualMaster.Title);
            Assert.Equal(reference.URL, actualMaster.Url);
            Assert.Equal(reference.ExternalReferenceId, actualMaster.DocumentId);
        }

        private void AssertPayments(ICollection<EconomyStream> expecteds, List<PaymentResponseDTO> actuals)
        {
            Assert.Equal(expecteds.Count, actuals.Count);

            foreach (var comparison in expecteds.OrderBy(x => x.AccountingEntry)
                .Zip(actuals.OrderBy(x => x.AccountingEntry), (expected, actual) => new { expected, actual })
                .ToList())
            {
                AssertPayment(comparison.expected, comparison.actual);
            }
        }

        private void AssertPayment(EconomyStream expected, PaymentResponseDTO actual)
        {
            Assert.Equal(expected.AccountingEntry, actual.AccountingEntry);
            Assert.Equal(expected.Note, actual.Note);
            Assert.Equal(expected.Acquisition, actual.Acquisition);
            Assert.Equal(expected.AuditDate, actual.AuditDate);
            Assert.Equal(expected.Operation, actual.Operation);
            Assert.Equal(expected.Other, actual.Other);
            AssertIdentity(expected.OrganizationUnit, actual.OrganizationUnit);
            AssertAuditStatus(expected.AuditStatus, actual.AuditStatus);
        }

        private static void AssertAuditStatus(TrafficLight? expectedFromSource, PaymentAuditStatus? actual)
        {
            PaymentAuditStatus? expected = expectedFromSource switch
            {
                TrafficLight.White => PaymentAuditStatus.White,
                TrafficLight.Red => PaymentAuditStatus.Red,
                TrafficLight.Yellow => PaymentAuditStatus.Yellow,
                TrafficLight.Green => PaymentAuditStatus.Green,
                null => null,
                _ => throw new ArgumentOutOfRangeException(nameof(expectedFromSource), expectedFromSource, null)
            };
            Assert.Equal(expected, actual);
        }

        private static void AssertYearSegment(YearSegmentOption? expectedFromSource, YearSegmentChoice? actual)
        {
            YearSegmentChoice? expected = expectedFromSource switch
            {
                YearSegmentOption.EndOfCalendarYear => YearSegmentChoice.EndOfCalendarYear,
                YearSegmentOption.EndOfQuarter => YearSegmentChoice.EndOfQuarter,
                YearSegmentOption.EndOfMonth => YearSegmentChoice.EndOfMonth,
                null => null,
                _ => throw new ArgumentOutOfRangeException(nameof(expectedFromSource), expectedFromSource, null)
            };
            Assert.Equal(expected, actual);
        }

        private void AssertPaymentMilestones(ItContract contract, List<PaymentMileStoneDTO> handoverTrials)
        {
            Assert.Equal(contract.PaymentMilestones.Count, handoverTrials.Count);

            foreach (var comparison in contract.PaymentMilestones.OrderBy(x => x.Title)
                .Zip(handoverTrials.OrderBy(x => x.Title), (expected, actual) => new { expected, actual })
                .ToList())
            {
                Assert.Equal(comparison.expected.Title, comparison.actual.Title);
                Assert.Equal(comparison.expected.Approved, comparison.actual.Approved);
                Assert.Equal(comparison.expected.Expected, comparison.actual.Expected);
            }
        }

        private void AssertHandoverTrials(ItContract contract, List<HandoverTrialResponseDTO> handoverTrials)
        {
            Assert.Equal(contract.HandoverTrials.Count, handoverTrials.Count);

            foreach (var comparison in contract.HandoverTrials.OrderBy(x => x.HandoverTrialType.Uuid)
                .Zip(handoverTrials.OrderBy(x => x.HandoverTrialType.Uuid), (expected, actual) => new { expected, actual })
                .ToList())
            {
                AssertOptionalIdentity(comparison.expected.HandoverTrialType, comparison.actual.HandoverTrialType);
                Assert.Equal(comparison.expected.Approved, comparison.actual.ApprovedAt);
                Assert.Equal(comparison.expected.Expected, comparison.actual.ExpectedAt);
            }
        }

        private void AssertSystemUsages(ItContract contract, List<IdentityNamePairResponseDTO> systemUsages)
        {
            Assert.Equal(contract.AssociatedSystemUsages.Count, systemUsages.Count);

            foreach (var comparison in contract.AssociatedSystemUsages.Select(x => x.ItSystemUsage).OrderBy(x => x.Uuid)
                .Zip(systemUsages.OrderBy(x => x.Uuid), (expected, actual) => new { expected, actual })
                .ToList())
            {
                AssertSystemUsage(comparison.expected, comparison.actual);
            }
        }

        private void AssertSystemUsage(ItSystemUsage expected, IdentityNamePairResponseDTO actual)
        {
            Assert.Equal(expected.Uuid, actual.Uuid);
            Assert.Equal(expected.ItSystem.Name, actual.Name);
        }

        private static void AssertAgreementElements(IEnumerable<ItContractAgreementElementTypes> agreementElements, IEnumerable<IdentityNamePairResponseDTO> actualIdentities)
        {
            if (agreementElements == null)
            {
                Assert.Empty(actualIdentities);
            }
            else
            {
                AssertOptionalIdentities(agreementElements.Select(x => x.AgreementElementType), actualIdentities);
            }
        }

        private static void AssertOptionalIdentities<T>(IEnumerable<T> optionalExpectedIdentities, IEnumerable<IdentityNamePairResponseDTO> actualIdentities) where T : IHasUuid, IHasName
        {
            if (optionalExpectedIdentities == null)
            {
                Assert.Null(actualIdentities);
            }
            else
            {
                var orderedOptionalExpectedIdentities = optionalExpectedIdentities.OrderBy(x => x.Uuid).ToList();
                var orderedActualIdentities = actualIdentities.OrderBy(x => x.Uuid).ToList();

                Assert.Equal(orderedOptionalExpectedIdentities.Count, orderedActualIdentities.Count);

                foreach (var comparison in orderedOptionalExpectedIdentities
                    .Zip(orderedActualIdentities, (expected, actual) => new { expected, actual })
                    .ToList())
                {
                    AssertOptionalIdentity(comparison.expected, comparison.actual);
                }
            }
        }

        private static void AssertOptionalIdentity<T>(T optionalExpectedIdentity, IdentityNamePairResponseDTO actualIdentity) where T : IHasUuid, IHasName
        {
            if (optionalExpectedIdentity == null)
                Assert.Null(actualIdentity);
            else
                AssertIdentity(optionalExpectedIdentity, actualIdentity);
        }

        private static void AssertUser(User user, IdentityNamePairResponseDTO dtoValue)
        {
            Assert.Equal((user.GetFullName(), user.Uuid), (dtoValue.Name, dtoValue.Uuid));
        }

        private static void AssertOrganization(Organization organization, ShallowOrganizationResponseDTO shallowOrganizationDTO)
        {
            AssertIdentity(organization, shallowOrganizationDTO);
            Assert.Equal(organization.Cvr, shallowOrganizationDTO.Cvr);
        }

        private static void AssertIdentities<T>(IEnumerable<T> sourceIdentities, IEnumerable<IdentityNamePairResponseDTO> dto) where T : IHasUuid, IHasName
        {
            var orderedOptionalExpectedIdentities = sourceIdentities.OrderBy(x => x.Uuid).ToList();
            var orderedActualIdentities = dto.OrderBy(x => x.Uuid).ToList();

            Assert.Equal(orderedOptionalExpectedIdentities.Count, orderedActualIdentities.Count);

            foreach (var comparison in orderedOptionalExpectedIdentities
                .Zip(orderedActualIdentities, (expected, actual) => new { expected, actual })
                .ToList())
            {
                AssertIdentity(comparison.expected, comparison.actual);
            }
        }

        private static void AssertIdentity<T>(T sourceIdentity, IdentityNamePairResponseDTO dto) where T : IHasUuid, IHasName
        {
            Assert.Equal(sourceIdentity.Name, dto.Name);
            Assert.Equal(sourceIdentity.Uuid, dto.Uuid);
        }

        #endregion
    }
}
