using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.ApplicationServices.Model.Contracts.Write;
using Core.ApplicationServices.Model.Shared.Write;
using Moq;
using Presentation.Web.Controllers.API.V2.External.ItContracts.Mapping;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Request.Contract;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;
using Presentation.Web.Models.API.V2.Types.Contract;
using Presentation.Web.Models.API.V2.Types.Shared;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    public class ItContractWriteModelMapperTest : WriteModelMapperTestBase
    {
        private readonly ItContractWriteModelMapper _sut;
        private readonly Mock<ICurrentHttpRequest> _currentHttpRequestMock;

        public ItContractWriteModelMapperTest()
        {
            _currentHttpRequestMock = new Mock<ICurrentHttpRequest>();
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonRootProperties())
                .Returns(GetRootProperties());
            _sut = new ItContractWriteModelMapper(_currentHttpRequestMock.Object);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("test")]
        public void Can_Map_Name_From_Post(string name)
        {
            //Arrange
            var requestDto = new CreateNewContractRequestDTO { Name = name };

            //Act
            var modificationParameters = _sut.FromPOST(requestDto);

            //Assert
            Assert.Equal(requestDto.Name, AssertPropertyContainsDataChange(modificationParameters.Name));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("test")]
        public void Can_Map_Name_From_Put(string name)
        {
            //Arrange
            var requestDto = new UpdateContractRequestDTO { Name = name };

            //Act
            var modificationParameters = _sut.FromPUT(requestDto);

            //Assert
            Assert.Equal(requestDto.Name, AssertPropertyContainsDataChange(modificationParameters.Name));
        }

        public static IEnumerable<object[]> GetUndefinedSectionsInput()
        {
            return CreateGetUndefinedSectionsInput(15);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedSectionsInput))]
        public void FromPUT_Ignores_Undefined_Root_Sections(
            bool noName,
            bool noGeneralData,
            bool noParent,
            bool noResponsible,
            bool noProcurement,
            bool noSupplier,
            bool noHandoverTrials,
            bool noSystemUsages,
            bool noExternalReferences,
            bool noDataProcessingRegistrations,
            bool noRoles,
            bool noPaymentModel,
            bool noAgreementPeriod,
            bool noPayments,
            bool noTermination)
        {
            //Arrange
            var rootProperties = GetRootProperties();

            if (noName) rootProperties.Remove(nameof(UpdateContractRequestDTO.Name));
            if (noGeneralData) rootProperties.Remove(nameof(UpdateContractRequestDTO.General));
            if (noParent) rootProperties.Remove(nameof(UpdateContractRequestDTO.ParentContractUuid));
            if (noResponsible) rootProperties.Remove(nameof(UpdateContractRequestDTO.Responsible));
            if (noProcurement) rootProperties.Remove(nameof(UpdateContractRequestDTO.Procurement));
            if (noSupplier) rootProperties.Remove(nameof(UpdateContractRequestDTO.Supplier));
            if (noHandoverTrials) rootProperties.Remove(nameof(UpdateContractRequestDTO.HandoverTrials));
            if (noExternalReferences) rootProperties.Remove(nameof(UpdateContractRequestDTO.ExternalReferences));
            if (noSystemUsages) rootProperties.Remove(nameof(UpdateContractRequestDTO.SystemUsageUuids));
            if (noDataProcessingRegistrations) rootProperties.Remove(nameof(UpdateContractRequestDTO.DataProcessingRegistrationUuids));
            if (noRoles) rootProperties.Remove(nameof(UpdateContractRequestDTO.Roles));
            if (noPaymentModel) rootProperties.Remove(nameof(UpdateContractRequestDTO.PaymentModel));
            if (noAgreementPeriod) rootProperties.Remove(nameof(UpdateContractRequestDTO.AgreementPeriod));
            if (noPayments) rootProperties.Remove(nameof(UpdateContractRequestDTO.Payments));
            if (noTermination) rootProperties.Remove(nameof(UpdateContractRequestDTO.Termination));
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonRootProperties()).Returns(rootProperties);
            var emptyInput = new UpdateContractRequestDTO();

            //Act
            var output = _sut.FromPUT(emptyInput);

            //Assert
            Assert.Equal(noName, output.Name.IsUnchanged);
            Assert.Equal(noParent, output.ParentContractUuid.IsUnchanged);
            Assert.Equal(noGeneralData, output.General.IsNone);
            Assert.Equal(noResponsible, output.Responsible.IsNone);
            Assert.Equal(noProcurement, output.Procurement.IsNone);
            Assert.Equal(noSupplier, output.Supplier.IsNone);
            Assert.Equal(noHandoverTrials, output.HandoverTrials.IsNone);
            Assert.Equal(noExternalReferences, output.ExternalReferences.IsNone);
            Assert.Equal(noSystemUsages, output.SystemUsageUuids.IsNone);
            Assert.Equal(noDataProcessingRegistrations, output.DataProcessingRegistrationUuids.IsNone);
            Assert.Equal(noPaymentModel, output.PaymentModel.IsNone);
            Assert.Equal(noAgreementPeriod, output.AgreementPeriod.IsNone);
            Assert.Equal(noPayments, output.Payments.IsNone);
            Assert.Equal(noTermination, output.Termination.IsNone);
        }

        [Fact]
        public void Can_Map_General()
        {
            //Arrange
            var input = A<ContractGeneralDataWriteRequestDTO>();

            //Act
            var output = _sut.MapGeneralData(input);

            //Assert
            AssertGeneralData(input, output);
        }

        [Fact]
        public void FromPost_Maps_General()
        {
            //Arrange
            var input = new CreateNewContractRequestDTO()
            {
                General = A<ContractGeneralDataWriteRequestDTO>()
            };

            //Act
            var output = _sut.FromPOST(input).General;

            //Assert
            AssertGeneralData(input.General, AssertPropertyContainsDataChange(output));
        }

        [Fact]
        public void FromPut_Maps_General()
        {
            //Arrange
            var input = new UpdateContractRequestDTO()
            {
                General = A<ContractGeneralDataWriteRequestDTO>()
            };

            //Act
            var output = _sut.FromPUT(input).General;

            //Assert
            AssertGeneralData(input.General, AssertPropertyContainsDataChange(output));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Map_Parent_From_Post(bool hasParentUuid)
        {
            //Arrange
            var parentUuid = hasParentUuid ? A<Guid?>() : null;
            var requestDto = new CreateNewContractRequestDTO { ParentContractUuid = parentUuid };

            //Act
            var modificationParameters = _sut.FromPOST(requestDto);

            //Assert
            Assert.Equal(requestDto.ParentContractUuid, AssertPropertyContainsDataChange(modificationParameters.ParentContractUuid));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Map_Parent_From_Put(bool hasParentUuid)
        {
            //Arrange
            var parentUuid = hasParentUuid ? A<Guid?>() : null;
            var requestDto = new UpdateContractRequestDTO { ParentContractUuid = parentUuid };

            //Act
            var modificationParameters = _sut.FromPUT(requestDto);

            //Assert
            Assert.Equal(requestDto.ParentContractUuid, AssertPropertyContainsDataChange(modificationParameters.ParentContractUuid));
        }

        [Fact]
        public void Can_Map_Responsible()
        {
            //Arrange
            var input = A<ContractResponsibleDataWriteRequestDTO>();

            //Act
            var output = _sut.MapResponsible(input);

            //Assert
            AssertResponsible(input, output);
        }

        [Fact]
        public void Can_Map_Responsible_FromPOST()
        {
            //Arrange
            var input = A<ContractResponsibleDataWriteRequestDTO>();

            //Act
            var output = _sut.FromPOST(new CreateNewContractRequestDTO() { Responsible = input });

            //Assert
            AssertResponsible(input, output.Responsible.Value);
        }

        [Fact]
        public void Can_Map_Responsible_FromPUT()
        {
            //Arrange
            var input = A<ContractResponsibleDataWriteRequestDTO>();

            //Act
            var output = _sut.FromPUT(new UpdateContractRequestDTO { Responsible = input });

            //Assert
            AssertResponsible(input, output.Responsible.Value);
        }

        [Fact]
        public void Can_Map_Supplier()
        {
            //Arrange
            var input = A<ContractSupplierDataWriteRequestDTO>();

            //Act
            var output = _sut.MapSupplier(input);

            //Assert
            AssertSupplier(input, output);
        }

        [Fact]
        public void Can_Map_Supplier_FromPOST()
        {
            //Arrange
            var input = A<ContractSupplierDataWriteRequestDTO>();

            //Act
            var output = _sut.FromPOST(new CreateNewContractRequestDTO() { Supplier = input });

            //Assert
            AssertSupplier(input, output.Supplier.Value);
        }

        [Fact]
        public void Can_Map_Supplier_FromPUT()
        {
            //Arrange
            var input = A<ContractSupplierDataWriteRequestDTO>();

            //Act
            var output = _sut.FromPUT(new UpdateContractRequestDTO { Supplier = input });

            //Assert
            AssertSupplier(input, output.Supplier.Value);
        }

        [Fact]
        public void Can_Map_HandoverTrials()
        {
            //Arrange
            var input = Many<HandoverTrialRequestDTO>().ToList();

            //Act
            var output = _sut.MapHandOverTrials(input);

            //Assert
            AssertHandoverTrials(input, output);
        }

        [Fact]
        public void Can_Map_HandoverTrials_FromPOST()
        {
            //Arrange
            var input = Many<HandoverTrialRequestDTO>().ToList();

            //Act
            var output = _sut.FromPOST(new CreateNewContractRequestDTO() { HandoverTrials = input });

            //Assert
            AssertHandoverTrials(input, output.HandoverTrials.Value);
        }

        [Fact]
        public void Can_Map_HandoverTrials_FromPUT()
        {
            //Arrange
            var input = Many<HandoverTrialRequestDTO>().ToList();

            //Act
            var output = _sut.FromPUT(new UpdateContractRequestDTO { HandoverTrials = input });

            //Assert
            AssertHandoverTrials(input, output.HandoverTrials.Value);
        }

        private static void AssertHandoverTrials(List<HandoverTrialRequestDTO> input, IEnumerable<ItContractHandoverTrialUpdate> output)
        {
            var expected = input.OrderBy(x => x.HandoverTrialTypeUuid).ToList();
            var actual = output.OrderBy(x => x.HandoverTrialTypeUuid).ToList();
            Assert.Equal(expected.Count, actual.Count);
            foreach (var pair in expected.Zip(actual, (e, a) => new { inputDTO = e, mappedChange = a }))
            {
                Assert.Equal(pair.inputDTO.ApprovedAt, pair.mappedChange.ApprovedAt);
                Assert.Equal(pair.inputDTO.ExpectedAt, pair.mappedChange.ExpectedAt);
                Assert.Equal(pair.inputDTO.HandoverTrialTypeUuid, pair.mappedChange.HandoverTrialTypeUuid);
            }
        }

        private static void AssertSupplier(ContractSupplierDataWriteRequestDTO input, ItContractSupplierModificationParameters output)
        {
            Assert.Equal(input.OrganizationUuid, AssertPropertyContainsDataChange(output.OrganizationUuid));
            Assert.Equal(input.Signed, AssertPropertyContainsDataChange(output.Signed));
            Assert.Equal(input.SignedAt, AssertPropertyContainsDataChange(output.SignedAt));
            Assert.Equal(input.SignedBy, AssertPropertyContainsDataChange(output.SignedBy));
        }

        private static void AssertResponsible(ContractResponsibleDataWriteRequestDTO input,
            ItContractResponsibleDataModificationParameters output)
        {
            Assert.Equal(input.OrganizationUnitUuid, AssertPropertyContainsDataChange(output.OrganizationUnitUuid));
            Assert.Equal(input.Signed, AssertPropertyContainsDataChange(output.Signed));
            Assert.Equal(input.SignedAt, AssertPropertyContainsDataChange(output.SignedAt));
            Assert.Equal(input.SignedBy, AssertPropertyContainsDataChange(output.SignedBy));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Map_Procurement_From_Post(bool hasValues)
        {
            //Arrange
            var procurement = CreateProcurementRequest(hasValues);
            var requestDto = new CreateNewContractRequestDTO { Procurement = procurement };

            //Act
            var modificationParameters = _sut.FromPOST(requestDto);

            //Assert
            Assert.True(modificationParameters.Procurement.HasValue);
            var procurementDto = modificationParameters.Procurement.Value;
            AssertProcurement(hasValues, procurement, procurementDto);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Map_Procurement_From_Put(bool hasValues)
        {
            //Arrange
            var procurement = CreateProcurementRequest(hasValues);
            var requestDto = new UpdateContractRequestDTO { Procurement = procurement };

            //Act
            var modificationParameters = _sut.FromPUT(requestDto);

            //Assert
            Assert.True(modificationParameters.Procurement.HasValue);
            var procurementDto = modificationParameters.Procurement.Value;
            AssertProcurement(hasValues, procurement, procurementDto);
        }

        [Fact]
        public void Can_Map_Procurement()
        {
            //Arrange
            var input = A<ContractProcurementDataWriteRequestDTO>();

            //Act
            var output = _sut.MapProcurement(input);

            //Assert
            AssertProcurement(true, input, output);
        }

        [Fact]
        public void Can_Map_ExternalReferences()
        {
            //Arrange
            var references = Many<ExternalReferenceDataDTO>().OrderBy(x => x.Url).ToList();

            //Act
            var mappedReferences = _sut.MapReferences(references).OrderBy(x => x.Url).ToList();

            //Assert
            AssertExternalReferences(mappedReferences, references);
        }

        [Fact]
        public void Can_Map_ExternalReferences_FromPUT()
        {
            //Arrange
            var references = Many<ExternalReferenceDataDTO>().OrderBy(x => x.Url).ToList();

            //Act
            var mappedReferences = _sut.FromPUT(new UpdateContractRequestDTO { ExternalReferences = references }).ExternalReferences.Value.OrderBy(x => x.Url).ToList();

            //Assert
            AssertExternalReferences(mappedReferences, references);
        }

        [Fact]
        public void Can_Map_ExternalReferences_FromPOST()
        {
            //Arrange
            var references = Many<ExternalReferenceDataDTO>().OrderBy(x => x.Url).ToList();

            //Act
            var mappedReferences = _sut.FromPOST(new CreateNewContractRequestDTO { ExternalReferences = references }).ExternalReferences.Value.OrderBy(x => x.Url).ToList();

            //Assert
            AssertExternalReferences(mappedReferences, references);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Map_SystemUsages_From_Post(bool hasValues)
        {
            //Arrange
            var systemUsageUuids = hasValues ? new[] { A<Guid>(), A<Guid>() } : new Guid[0];
            var requestDto = new CreateNewContractRequestDTO { SystemUsageUuids = systemUsageUuids };

            //Act
            var modificationParameters = _sut.FromPOST(requestDto);

            //Assert
            Assert.True(modificationParameters.SystemUsageUuids.HasValue);
            var modifiedUuids = modificationParameters.SystemUsageUuids.Value;
            AssertUuids(systemUsageUuids, modifiedUuids);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Map_SystemUsages_From_Put(bool hasValues)
        {
            //Arrange
            var systemUsageUuids = hasValues ? new[] { A<Guid>(), A<Guid>() } : new Guid[0];
            var requestDto = new UpdateContractRequestDTO { SystemUsageUuids = systemUsageUuids };

            //Act
            var modificationParameters = _sut.FromPUT(requestDto);

            //Assert
            Assert.True(modificationParameters.SystemUsageUuids.HasValue);
            var modifiedUuids = modificationParameters.SystemUsageUuids.Value;
            AssertUuids(systemUsageUuids, modifiedUuids);
        }

        [Fact]
        public void Can_Map_Roles()
        {
            //Arrange
            var roles = Many<RoleAssignmentRequestDTO>().OrderBy(x => x.RoleUuid).ToList();

            //Act
            var rolePairs = _sut.MapRoles(roles).ToList();

            //Assert
            AssertRoles(roles, rolePairs);
        }

        [Fact]
        public void Can_Map_Roles_FromPUT()
        {
            //Arrange
            var roles = Many<RoleAssignmentRequestDTO>().OrderBy(x => x.RoleUuid).ToList();

            //Act
            var result = _sut.FromPUT(new UpdateContractRequestDTO() { Roles = roles });

            //Assert
            AssertRoles(roles, AssertPropertyContainsDataChange(result.Roles).ToList());
        }

        [Fact]
        public void Can_Map_Roles_FromPOST()
        {
            //Arrange
            var roles = Many<RoleAssignmentRequestDTO>().OrderBy(x => x.RoleUuid).ToList();

            //Act
            var result = _sut.FromPOST(new CreateNewContractRequestDTO { Roles = roles });

            //Assert
            AssertRoles(roles, AssertPropertyContainsDataChange(result.Roles).ToList());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Map_DataProcessingRegistrations_From_Post(bool hasValues)
        {
            //Arrange
            var dataProcessingRegistrationUuids = hasValues ? new[] { A<Guid>(), A<Guid>() } : new Guid[0];
            var requestDto = new CreateNewContractRequestDTO { DataProcessingRegistrationUuids = dataProcessingRegistrationUuids };

            //Act
            var modificationParameters = _sut.FromPOST(requestDto);

            //Assert
            Assert.True(modificationParameters.DataProcessingRegistrationUuids.HasValue);
            var modifiedUuids = modificationParameters.DataProcessingRegistrationUuids.Value;
            AssertUuids(dataProcessingRegistrationUuids, modifiedUuids);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Map_DataProcessingRegistrations_From_Put(bool hasValues)
        {
            //Arrange
            var dataProcessingRegistrationUuids = hasValues ? new[] { A<Guid>(), A<Guid>() } : new Guid[0];
            var requestDto = new UpdateContractRequestDTO { DataProcessingRegistrationUuids = dataProcessingRegistrationUuids };

            //Act
            var modificationParameters = _sut.FromPUT(requestDto);

            //Assert
            Assert.True(modificationParameters.DataProcessingRegistrationUuids.HasValue);
            var modifiedUuids = modificationParameters.DataProcessingRegistrationUuids.Value;
            AssertUuids(dataProcessingRegistrationUuids, modifiedUuids);
        }

        private static void AssertRoles(List<RoleAssignmentRequestDTO> roles, List<UserRolePair> rolePairs)
        {
            Assert.Equal(roles.Count, rolePairs.Count);
            for (var i = 0; i < rolePairs.Count; i++)
            {
                var expected = roles[i];
                var actual = rolePairs[i];
                Assert.Equal(expected.RoleUuid, actual.RoleUuid);
                Assert.Equal(expected.UserUuid, actual.UserUuid);
            }
        }


        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Map_PaymentModel(bool hasValues)
        {
            //Arrange
            var milestones = hasValues ? Many<PaymentMileStoneDTO>().ToList() : null;
            var input = new ContractPaymentModelDataWriteRequestDTO()
            {
                OperationsRemunerationStartedAt = hasValues ? A<DateTime>() : null,
                PaymentFrequencyUuid = hasValues ? A<Guid>() : null,
                PaymentModelUuid = hasValues ? A<Guid>() : null,
                PriceRegulationUuid = hasValues ? A<Guid>() : null,
                PaymentMileStones = milestones
            };

            //Act
            var output = _sut.MapPaymentModel(input);

            //Assert
            AssertPaymentModel(input, output, hasValues);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Map_PaymentModel_FromPOST(bool hasValues)
        {
            //Arrange
            var milestones = hasValues ? Many<PaymentMileStoneDTO>().ToList() : null;
            var input = new ContractPaymentModelDataWriteRequestDTO()
            {
                OperationsRemunerationStartedAt = hasValues ? A<DateTime>() : null,
                PaymentFrequencyUuid = hasValues ? A<Guid>() : null,
                PaymentModelUuid = hasValues ? A<Guid>() : null,
                PriceRegulationUuid = hasValues ? A<Guid>() : null,
                PaymentMileStones = milestones
            };

            //Act
            var output = _sut.FromPOST(new CreateNewContractRequestDTO() { PaymentModel = input });

            //Assert
            AssertPaymentModel(input, output.PaymentModel.Value, hasValues);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Map_PaymentModel_FromPUT(bool hasValues)
        {
            //Arrange
            var milestones = hasValues ? Many<PaymentMileStoneDTO>().ToList() : null;
            var input = new ContractPaymentModelDataWriteRequestDTO()
            {
                OperationsRemunerationStartedAt = hasValues ? A<DateTime>() : null,
                PaymentFrequencyUuid = hasValues ? A<Guid>() : null,
                PaymentModelUuid = hasValues ? A<Guid>() : null,
                PriceRegulationUuid = hasValues ? A<Guid>() : null,
                PaymentMileStones = milestones
            };

            //Act
            var output = _sut.FromPUT(new UpdateContractRequestDTO { PaymentModel = input });

            //Assert
            AssertPaymentModel(input, output.PaymentModel.Value, hasValues);
        }

        [Fact]
        public void Can_Map_Agreement_Period()
        {
            //Arrange
            var input = A<ContractAgreementPeriodDataWriteRequestDTO>();

            //Act
            var output = _sut.MapAgreementPeriod(input);

            //Assert
            AssertAgreementPeriod(input, output);
        }

        [Fact]
        public void Can_Map_Agreement_Period_FromPUT()
        {
            //Arrange
            var input = A<ContractAgreementPeriodDataWriteRequestDTO>();

            //Act
            var output = _sut.FromPUT(new UpdateContractRequestDTO() { AgreementPeriod = input });

            //Assert
            AssertAgreementPeriod(input, AssertPropertyContainsDataChange(output.AgreementPeriod));
        }

        [Fact]
        public void Can_Map_Agreement_Period_FromPOST()
        {
            //Arrange
            var input = A<ContractAgreementPeriodDataWriteRequestDTO>();

            //Act
            var output = _sut.FromPOST(new CreateNewContractRequestDTO { AgreementPeriod = input });

            //Assert
            AssertAgreementPeriod(input, AssertPropertyContainsDataChange(output.AgreementPeriod));
        }

        [Fact]
        public void Can_Map_Payments()
        {
            //Arrange
            var input = A<ContractPaymentsDataWriteRequestDTO>();

            //Act
            var output = _sut.MapPayments(input);

            //Assert
            AssertPayments(input, output);
        }

        [Fact]
        public void Can_Map_Payments_FromPOST()
        {
            //Arrange
            var input = A<ContractPaymentsDataWriteRequestDTO>();

            //Act
            var output = _sut.FromPOST(new CreateNewContractRequestDTO { Payments = input }).Payments.Value;

            //Assert
            AssertPayments(input, output);
        }

        [Fact]
        public void Can_Map_Payments_FromPUT()
        {
            //Arrange
            var input = A<ContractPaymentsDataWriteRequestDTO>();

            //Act
            var output = _sut.FromPUT(new UpdateContractRequestDTO { Payments = input }).Payments.Value;

            //Assert
            AssertPayments(input, output);
        }
		
		[Fact]
        public void Can_Map_Termination()
        {
            //Arrange
            var input = A<ContractTerminationDataWriteRequestDTO>();

            //Act
            var output = _sut.MapTermination(input);

            //Assert
            AssertTermination(input, output);
        }

        [Fact]
        public void Can_Map_Termination_FromPUT()
        {
            //Arrange
            var input = A<ContractTerminationDataWriteRequestDTO>();

            //Act
            var output = _sut.FromPUT(new UpdateContractRequestDTO() { Termination = input });

            //Assert
            AssertTermination(input, AssertPropertyContainsDataChange(output.Termination));
        }

        [Fact]
        public void Can_Map_Termination_FromPOST()
        {
            //Arrange
            var input = A<ContractTerminationDataWriteRequestDTO>();

            //Act
            var output = _sut.FromPOST(new CreateNewContractRequestDTO() { Termination = input });

            //Assert
            AssertTermination(input, AssertPropertyContainsDataChange(output.Termination));
        }

        private static void AssertTermination(ContractTerminationDataWriteRequestDTO input, ItContractTerminationParameters output)
        {
            Assert.Equal(input.TerminatedAt, AssertPropertyContainsDataChange(output.TerminatedAt));
            Assert.Equal(input.Terms.NoticePeriodMonthsUuid, AssertPropertyContainsDataChange(output.NoticePeriodMonthsUuid));
            Assert.Equal(input.Terms.NoticePeriodExtendsCurrent?.ToYearSegmentOption(), AssertPropertyContainsDataChange(output.NoticePeriodExtendsCurrent));
            Assert.Equal(input.Terms.NoticeByEndOf?.ToYearSegmentOption(), AssertPropertyContainsDataChange(output.NoticeByEndOf));
        }

        private static void AssertAgreementPeriod(ContractAgreementPeriodDataWriteRequestDTO input,
            ItContractAgreementPeriodModificationParameters output)
        {
            Assert.Equal(input.DurationMonths, AssertPropertyContainsDataChange(output.DurationMonths));
            Assert.Equal(input.DurationYears, AssertPropertyContainsDataChange(output.DurationYears));
            Assert.Equal(input.ExtensionOptionsUsed, AssertPropertyContainsDataChange(output.ExtensionOptionsUsed));
            Assert.Equal(input.ExtensionOptionsUuid, AssertPropertyContainsDataChange(output.ExtensionOptionsUuid));
            Assert.Equal(input.IrrevocableUntil, AssertPropertyContainsDataChange(output.IrrevocableUntil));
            Assert.Equal(input.IsContinuous, AssertPropertyContainsDataChange(output.IsContinuous));
        }

        private static void AssertPaymentModel(ContractPaymentModelDataWriteRequestDTO input, ItContractPaymentModelModificationParameters output, bool hasValues)
        {
            Assert.Equal(input.PaymentFrequencyUuid, output.PaymentFrequencyUuid.NewValue);
            Assert.Equal(input.PaymentModelUuid, output.PaymentModelUuid.NewValue);
            Assert.Equal(input.PriceRegulationUuid, output.PriceRegulationUuid.NewValue);
            Assert.Equal(input.OperationsRemunerationStartedAt, output.OperationsRemunerationStartedAt.NewValue.Match(val => val, () => (DateTime?)null));
            AssertPaymentMilestones(
                input.PaymentMileStones.FromNullable().Match(val => val, () => new List<PaymentMileStoneDTO>()),
                output.PaymentMileStones.Match(val => val, () => new List<ItContractPaymentMilestone>()));
        }

        private static void AssertPaymentMilestones(IEnumerable<PaymentMileStoneDTO> input, IEnumerable<ItContractPaymentMilestone> output)
        {
            var expected = input.OrderBy(x => x.Title).ToList();
            var actual = output.OrderBy(x => x.Title).ToList();
            Assert.Equal(expected.Count, actual.Count);
            foreach (var pair in expected.Zip(actual, (e, a) => new { inputDTO = e, mappedChange = a }))
            {
                Assert.Equal(pair.inputDTO.Approved, pair.mappedChange.Approved);
                Assert.Equal(pair.inputDTO.Expected, pair.mappedChange.Expected);
                Assert.Equal(pair.inputDTO.Title, pair.mappedChange.Title);
            }
        }

        private static void AssertUuids(IEnumerable<Guid> expected, IEnumerable<Guid> actual)
        {
            var orderedExpected = expected.OrderBy(x => x).ToList();
            var orderedActual = actual.OrderBy(x => x).ToList();

            Assert.Equal(orderedExpected.Count, orderedActual.Count);
            for (var i = 0; i < orderedExpected.Count; i++)
            {
                Assert.Equal(orderedExpected[i], orderedActual[i]);
            }
        }

        private static void AssertExternalReferences(List<UpdatedExternalReferenceProperties> mappedReferences, List<ExternalReferenceDataDTO> references)
        {
            Assert.Equal(mappedReferences.Count, mappedReferences.Count);
            for (var i = 0; i < mappedReferences.Count; i++)
            {
                var expected = references[i];
                var actual = mappedReferences[i];
                Assert.Equal(expected.Url, actual.Url);
                Assert.Equal(expected.Title, actual.Title);
                Assert.Equal(expected.DocumentId, actual.DocumentId);
                Assert.Equal(expected.MasterReference, actual.MasterReference);
            }
        }

        private static void AssertGeneralData(ContractGeneralDataWriteRequestDTO input,
            ItContractGeneralDataModificationParameters output)
        {
            Assert.Equal(input.ContractId, AssertPropertyContainsDataChange(output.ContractId));
            Assert.Equal(input.ContractTypeUuid, AssertPropertyContainsDataChange(output.ContractTypeUuid));
            Assert.Equal(input.ContractTemplateUuid, AssertPropertyContainsDataChange(output.ContractTemplateUuid));
            Assert.Equal(input.AgreementElementUuids, AssertPropertyContainsDataChange(output.AgreementElementUuids));
            Assert.Equal(input.Notes, AssertPropertyContainsDataChange(output.Notes));
            Assert.Equal(input.Validity.ValidFrom, AssertPropertyContainsDataChange(output.ValidFrom));
            Assert.Equal(input.Validity.ValidTo, AssertPropertyContainsDataChange(output.ValidTo));
            Assert.Equal(input.Validity.EnforcedValid, AssertPropertyContainsDataChange(output.EnforceValid));
        }

        private static void AssertProcurement(bool hasValues, ContractProcurementDataWriteRequestDTO expected, ItContractProcurementModificationParameters actual)
        {
            Assert.Equal(expected.ProcurementStrategyUuid, AssertPropertyContainsDataChange(actual.ProcurementStrategyUuid));
            Assert.Equal(expected.PurchaseTypeUuid, AssertPropertyContainsDataChange(actual.PurchaseTypeUuid));
            if (hasValues)
            {
                var (half, year) = AssertPropertyContainsDataChange(actual.ProcurementPlan);
                Assert.Equal(expected.ProcurementPlan.HalfOfYear, half);
                Assert.Equal(expected.ProcurementPlan.Year, year);
            }
            else
            {
                AssertPropertyContainsResetDataChange(actual.ProcurementPlan);
            }
        }

        private ContractProcurementDataWriteRequestDTO CreateProcurementRequest(bool hasValues)
        {
            return new ContractProcurementDataWriteRequestDTO
            {
                ProcurementStrategyUuid = hasValues ? A<Guid>() : null,
                PurchaseTypeUuid = hasValues ? A<Guid>() : null,
                ProcurementPlan = hasValues
                    ? new ProcurementPlanDTO()
                    {
                        HalfOfYear = Convert.ToByte(A<int>() % 1 + 1),
                        Year = A<int>()
                    }
                    : null
            };
        }

        private static HashSet<string> GetRootProperties()
        {
            return typeof(CreateNewContractRequestDTO).GetProperties().Select(x => x.Name).ToHashSet();
        }
		
		private static void AssertPayments(ContractPaymentsDataWriteRequestDTO input, ItContractPaymentDataModificationParameters output)
        {
            AssertPaymentCollection(input.Internal, AssertPropertyContainsDataChange(output.InternalPayments));
            AssertPaymentCollection(input.External, AssertPropertyContainsDataChange(output.ExternalPayments));
        }

        private static void AssertPaymentCollection(IEnumerable<PaymentRequestDTO> expectedPayments, IEnumerable<ItContractPayment> outputPayments)
        {
            var expected = expectedPayments.ToList();
            var actual = outputPayments.ToList();
            Assert.Equal(expected.Count, actual.Count);
            for (var i = 0; i < expected.Count; i++)
            {
                var exp = expected[i];
                var act = actual[i];
                Assert.Equal(exp.Note, act.Note);
                Assert.Equal(exp.AccountingEntry, act.AccountingEntry);
                Assert.Equal(exp.Acquisition, act.Acquisition);
                Assert.Equal(exp.AuditDate, act.AuditDate);
                Assert.Equal(exp.AuditStatus.ToTrafficLight(), act.AuditStatus);
                Assert.Equal(exp.Operation, act.Operation);
                Assert.Equal(exp.OrganizationUnitUuid, act.OrganizationUnitUuid);
                Assert.Equal(exp.Other, act.Other);
            }
        }
    }
}
