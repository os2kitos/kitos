using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Contracts.Write;
using Core.ApplicationServices.Model.Shared.Write;
using Moq;
using Newtonsoft.Json.Linq;
using Presentation.Web.Controllers.API.V2.External.ItContracts.Mapping;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Request.Contract;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;
using Presentation.Web.Models.API.V2.Types.Contract;
using Presentation.Web.Models.API.V2.Types.Shared;
using Tests.Toolkit.Extensions;
using Xunit;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Models.API.V2.Request.Generic.ExternalReferences;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    public class ItContractWriteModelMapperTest : WriteModelMapperTestBase
    {
        private readonly ItContractWriteModelMapper _sut;
        private readonly Mock<ICurrentHttpRequest> _currentHttpRequestMock;

        public ItContractWriteModelMapperTest()
        {
            _currentHttpRequestMock = new Mock<ICurrentHttpRequest>();
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(Enumerable.Empty<string>().AsParameterMatch())).Returns(GetAllInputPropertyNames<UpdateContractRequestDTO>());
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(nameof(UpdateContractRequestDTO.General).WrapAsEnumerable().AsParameterMatch())).Returns(GetAllInputPropertyNames<ContractGeneralDataWriteRequestDTO>());
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(nameof(UpdateContractRequestDTO.Responsible).WrapAsEnumerable().AsParameterMatch())).Returns(GetAllInputPropertyNames<ContractResponsibleDataWriteRequestDTO>());
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(nameof(UpdateContractRequestDTO.Procurement).WrapAsEnumerable().AsParameterMatch())).Returns(GetAllInputPropertyNames<ContractProcurementDataWriteRequestDTO>());
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(nameof(UpdateContractRequestDTO.Supplier).WrapAsEnumerable().AsParameterMatch())).Returns(GetAllInputPropertyNames<ContractSupplierDataWriteRequestDTO>());
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(nameof(UpdateContractRequestDTO.AgreementPeriod).WrapAsEnumerable().AsParameterMatch())).Returns(GetAllInputPropertyNames<ContractAgreementPeriodDataWriteRequestDTO>());
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(nameof(UpdateContractRequestDTO.PaymentModel).WrapAsEnumerable().AsParameterMatch())).Returns(GetAllInputPropertyNames<ContractPaymentModelDataWriteRequestDTO>());
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(nameof(UpdateContractRequestDTO.Payments).WrapAsEnumerable().AsParameterMatch())).Returns(GetAllInputPropertyNames<ContractPaymentsDataWriteRequestDTO>());
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(nameof(UpdateContractRequestDTO.Termination).WrapAsEnumerable().AsParameterMatch())).Returns(GetAllInputPropertyNames<ContractTerminationDataWriteRequestDTO>());
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(new[] { nameof(UpdateContractRequestDTO.General), nameof(ContractWriteRequestDTO.General.Validity) }.AsParameterMatch())).Returns(GetAllInputPropertyNames<ContractValidityWriteRequestDTO>());
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(new[] { nameof(UpdateContractRequestDTO.Termination), nameof(ContractWriteRequestDTO.Termination.Terms) }.AsParameterMatch())).Returns(GetAllInputPropertyNames<ContractTerminationTermsRequestDTO>());
            _currentHttpRequestMock.Setup(x => x.GetObject(It.IsAny<IEnumerable<string>>())).Returns(Maybe<JToken>.None);
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

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("test")]
        public void Can_Map_Name_From_Patch(string name)
        {
            //Arrange
            var requestDto = new UpdateContractRequestDTO { Name = name };

            //Act
            var modificationParameters = _sut.FromPATCH(requestDto);

            //Assert
            Assert.Equal(requestDto.Name, AssertPropertyContainsDataChange(modificationParameters.Name));
        }

        public static IEnumerable<object[]> GetUndefinedSectionsInput()
        {
            return CreateGetUndefinedSectionsInput(14);
        }

        public static IEnumerable<object[]> GetUndefinedGeneralDataPropertiesInput()
        {
            return CreateGetUndefinedSectionsInput(10);
        }

        public static IEnumerable<object[]> GetUndefinedProcurementPropertiesInput()
        {
            return CreateGetUndefinedSectionsInput(4);
        }

        public static IEnumerable<object[]> GetUndefinedResponsibleDataPropertiesInput()
        {
            return CreateGetUndefinedSectionsInput(4);
        }

        public static IEnumerable<object[]> GetUndefinedSupplierDataPropertiesInput()
        {
            return CreateGetUndefinedSectionsInput(4);
        }

        public static IEnumerable<object[]> GetUndefinedPaymentModelPropertiesInput()
        {
            return CreateGetUndefinedSectionsInput(4);
        }

        public static IEnumerable<object[]> GetUndefinedAgreementPeriodPropertiesInput()
        {
            return CreateGetUndefinedSectionsInput(6);
        }

        public static IEnumerable<object[]> GetUndefinedTerminationDataPropertiesInput()
        {
            return CreateGetUndefinedSectionsInput(4);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedSectionsInput))]
        public void FromPATCH_Ignores_Undefined_Root_Sections(
            bool noName,
            bool noGeneralData,
            bool noParent,
            bool noResponsible,
            bool noProcurement,
            bool noSupplier,
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
            var emptyInput = ConfigureRequestInput(noName, noGeneralData, noParent, noResponsible, noProcurement, noSupplier, noSystemUsages, noExternalReferences, noDataProcessingRegistrations, noRoles, noPaymentModel, noAgreementPeriod, noPayments, noTermination);

            //Act
            var output = _sut.FromPATCH(emptyInput);

            //Assert
            Assert.Equal(noName, output.Name.IsUnchanged);
            Assert.Equal(noParent, output.ParentContractUuid.IsUnchanged);
            Assert.Equal(noGeneralData, output.General.IsNone);
            Assert.Equal(noResponsible, output.Responsible.IsNone);
            Assert.Equal(noProcurement, output.Procurement.IsNone);
            Assert.Equal(noSupplier, output.Supplier.IsNone);
            Assert.Equal(noExternalReferences, output.ExternalReferences.IsNone);
            Assert.Equal(noSystemUsages, output.SystemUsageUuids.IsNone);
            Assert.Equal(noDataProcessingRegistrations, output.DataProcessingRegistrationUuids.IsNone);
            Assert.Equal(noPaymentModel, output.PaymentModel.IsNone);
            Assert.Equal(noAgreementPeriod, output.AgreementPeriod.IsNone);
            Assert.Equal(noPayments, output.Payments.IsNone);
            Assert.Equal(noTermination, output.Termination.IsNone);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedSectionsInput))]
        public void FromPUT_Enforces_Undefined_Root_Sections(
            bool noName,
            bool noGeneralData,
            bool noParent,
            bool noResponsible,
            bool noProcurement,
            bool noSupplier,
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
            var emptyInput = ConfigureRequestInput(noName, noGeneralData, noParent, noResponsible, noProcurement, noSupplier, noSystemUsages, noExternalReferences, noDataProcessingRegistrations, noRoles, noPaymentModel, noAgreementPeriod, noPayments, noTermination);

            //Act
            var output = _sut.FromPUT(emptyInput);

            //Assert
            Assert.False(output.Name.IsUnchanged);
            Assert.False(output.ParentContractUuid.IsUnchanged);
            Assert.False(output.General.IsNone);
            Assert.False(output.Responsible.IsNone);
            Assert.False(output.Procurement.IsNone);
            Assert.False(output.Supplier.IsNone);
            Assert.False(output.ExternalReferences.IsNone);
            Assert.False(output.SystemUsageUuids.IsNone);
            Assert.False(output.DataProcessingRegistrationUuids.IsNone);
            Assert.False(output.PaymentModel.IsNone);
            Assert.False(output.AgreementPeriod.IsNone);
            Assert.False(output.Payments.IsNone);
            Assert.False(output.Termination.IsNone);
        }


        [Theory]
        [MemberData(nameof(GetUndefinedGeneralDataPropertiesInput))]
        public void FromPOST_Ignores_Undefined_Properties_In_GeneralSection(
            bool noContractId,
            bool noContractTypeUuid,
            bool noContractTemplateUuid,
            bool noAgreementElementUuids,
            bool noNotes,
            bool noEnforceValid,
            bool noValidFrom,
            bool noValidTo,
            bool noCriticalityTypeUuid,
            bool noRequireValidParent)
        {
            //Arrange
            var input = new CreateNewContractRequestDTO();
            ConfigureGeneralDataInputContext(noContractId, noContractTypeUuid, noContractTemplateUuid, noAgreementElementUuids, noNotes, noEnforceValid, noValidFrom, noValidTo, noCriticalityTypeUuid, noRequireValidParent);

            //Act
            var output = _sut.FromPOST(input).General.Value;

            //Assert
            Assert.Equal(noContractId, output.ContractId.IsUnchanged);
            Assert.Equal(noContractTypeUuid, output.ContractTypeUuid.IsUnchanged);
            Assert.Equal(noContractTemplateUuid, output.ContractTemplateUuid.IsUnchanged);
            Assert.Equal(noAgreementElementUuids, output.AgreementElementUuids.IsUnchanged);
            Assert.Equal(noNotes, output.Notes.IsUnchanged);
            Assert.Equal(noEnforceValid, output.EnforceValid.IsUnchanged);
            Assert.Equal(noValidFrom, output.ValidFrom.IsUnchanged);
            Assert.Equal(noValidTo, output.ValidTo.IsUnchanged);
            Assert.Equal(noRequireValidParent, output.RequireValidParent.IsUnchanged);
            Assert.Equal(noCriticalityTypeUuid, output.CriticalityUuid.IsUnchanged);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedGeneralDataPropertiesInput))]
        public void FromPATCH_Ignores_Undefined_Properties_In_GeneralSection(
            bool noContractId,
            bool noContractTypeUuid,
            bool noContractTemplateUuid,
            bool noAgreementElementUuids,
            bool noNotes,
            bool noEnforceValid,
            bool noValidFrom,
            bool noValidTo,
            bool noCriticalityTypeUuid, bool noRequireValidParent)
        {
            //Arrange
            var input = new UpdateContractRequestDTO();
            ConfigureGeneralDataInputContext(noContractId, noContractTypeUuid, noContractTemplateUuid, noAgreementElementUuids, noNotes, noEnforceValid, noValidFrom, noValidTo, noCriticalityTypeUuid, noRequireValidParent);

            //Act
            var output = _sut.FromPATCH(input).General.Value;

            //Assert
            Assert.Equal(noContractId, output.ContractId.IsUnchanged);
            Assert.Equal(noContractTypeUuid, output.ContractTypeUuid.IsUnchanged);
            Assert.Equal(noContractTemplateUuid, output.ContractTemplateUuid.IsUnchanged);
            Assert.Equal(noAgreementElementUuids, output.AgreementElementUuids.IsUnchanged);
            Assert.Equal(noNotes, output.Notes.IsUnchanged);
            Assert.Equal(noEnforceValid, output.EnforceValid.IsUnchanged);
            Assert.Equal(noValidFrom, output.ValidFrom.IsUnchanged);
            Assert.Equal(noValidTo, output.ValidTo.IsUnchanged);
            Assert.Equal(noCriticalityTypeUuid, output.CriticalityUuid.IsUnchanged);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedGeneralDataPropertiesInput))]
        public void FromPUT_Enforces_Undefined_Properties_In_GeneralSection(
            bool noContractId,
            bool noContractTypeUuid,
            bool noContractTemplateUuid,
            bool noAgreementElementUuids,
            bool noNotes,
            bool noEnforceValid,
            bool noValidFrom,
            bool noValidTo,
            bool noCriticalityTypeUuid, bool noRequireValidParent)
        {
            //Arrange
            var input = new UpdateContractRequestDTO();
            ConfigureGeneralDataInputContext(noContractId, noContractTypeUuid, noContractTemplateUuid, noAgreementElementUuids, noNotes, noEnforceValid, noValidFrom, noValidTo, noCriticalityTypeUuid, noRequireValidParent);

            //Act
            var output = _sut.FromPUT(input).General.Value;

            //Assert
            Assert.True(output.ContractId.HasChange);
            Assert.True(output.ContractTypeUuid.HasChange);
            Assert.True(output.ContractTemplateUuid.HasChange);
            Assert.True(output.AgreementElementUuids.HasChange);
            Assert.True(output.Notes.HasChange);
            Assert.True(output.EnforceValid.HasChange);
            Assert.True(output.ValidFrom.HasChange);
            Assert.True(output.ValidTo.HasChange);
            Assert.True(output.CriticalityUuid.HasChange);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedProcurementPropertiesInput))]
        public void FromPOST_Ignores_Undefined_Properties_In_ProcurementSection(
            bool noProcurementStrategyUuid,
            bool noPurchaseTypeUuid,
            bool noProcurementPlan,
            bool noProcurementInitiated)
        {
            //Arrange
            var input = new CreateNewContractRequestDTO();
            ConfigureProcurementInputContext(noProcurementStrategyUuid, noPurchaseTypeUuid, noProcurementPlan, noProcurementInitiated);

            //Act
            var output = _sut.FromPOST(input).Procurement.Value;

            //Assert
            Assert.Equal(noProcurementStrategyUuid, output.ProcurementStrategyUuid.IsUnchanged);
            Assert.Equal(noPurchaseTypeUuid, output.PurchaseTypeUuid.IsUnchanged);
            Assert.Equal(noProcurementPlan, output.ProcurementPlan.IsUnchanged);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedProcurementPropertiesInput))]
        public void FromPATCH_Ignores_Undefined_Properties_In_ProcurementSection(
            bool noProcurementStrategyUuid,
            bool noPurchaseTypeUuid,
            bool noProcurementPlan,
            bool noProcurementInitiated)
        {
            //Arrange
            var input = new UpdateContractRequestDTO();
            ConfigureProcurementInputContext(noProcurementStrategyUuid, noPurchaseTypeUuid, noProcurementPlan, noProcurementInitiated);

            //Act
            var output = _sut.FromPATCH(input).Procurement.Value;

            //Assert
            Assert.Equal(noProcurementStrategyUuid, output.ProcurementStrategyUuid.IsUnchanged);
            Assert.Equal(noPurchaseTypeUuid, output.PurchaseTypeUuid.IsUnchanged);
            Assert.Equal(noProcurementPlan, output.ProcurementPlan.IsUnchanged);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedProcurementPropertiesInput))]
        public void FromPUT_Enforces_Undefined_Properties_In_ProcurementSection(
            bool noProcurementStrategyUuid,
            bool noPurchaseTypeUuid,
            bool noProcurementPlan,
            bool noProcurementInitiated)
        {
            //Arrange
            var input = new UpdateContractRequestDTO();
            ConfigureProcurementInputContext(noProcurementStrategyUuid, noPurchaseTypeUuid, noProcurementPlan, noProcurementInitiated);

            //Act
            var output = _sut.FromPUT(input).Procurement.Value;

            //Assert
            Assert.True(output.ProcurementStrategyUuid.HasChange);
            Assert.True(output.PurchaseTypeUuid.HasChange);
            Assert.True(output.ProcurementPlan.HasChange);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedResponsibleDataPropertiesInput))]
        public void FromPOST_Ignores_Undefined_Properties_In_ResponsibleDataSection(
            bool noOrganizationUnitUuid,
            bool noSigned,
            bool noSignedAt,
            bool noSignedBy)
        {
            //Arrange
            var input = new CreateNewContractRequestDTO();
            ConfigureResponsibleDataInputContext(noOrganizationUnitUuid, noSigned, noSignedAt, noSignedBy);

            //Act
            var output = _sut.FromPOST(input).Responsible.Value;

            //Assert
            Assert.Equal(noOrganizationUnitUuid, output.OrganizationUnitUuid.IsUnchanged);
            Assert.Equal(noSigned, output.Signed.IsUnchanged);
            Assert.Equal(noSignedAt, output.SignedAt.IsUnchanged);
            Assert.Equal(noSignedBy, output.SignedBy.IsUnchanged);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedResponsibleDataPropertiesInput))]
        public void FromPATCH_Ignores_Undefined_Properties_In_ResponsibleDataSection(
            bool noOrganizationUnitUuid,
            bool noSigned,
            bool noSignedAt,
            bool noSignedBy)
        {
            //Arrange
            var input = new UpdateContractRequestDTO();
            ConfigureResponsibleDataInputContext(noOrganizationUnitUuid, noSigned, noSignedAt, noSignedBy);

            //Act
            var output = _sut.FromPATCH(input).Responsible.Value;

            //Assert
            Assert.Equal(noOrganizationUnitUuid, output.OrganizationUnitUuid.IsUnchanged);
            Assert.Equal(noSigned, output.Signed.IsUnchanged);
            Assert.Equal(noSignedAt, output.SignedAt.IsUnchanged);
            Assert.Equal(noSignedBy, output.SignedBy.IsUnchanged);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedResponsibleDataPropertiesInput))]
        public void FromPUT_Enforces_Undefined_Properties_In_ResponsibleDataSection(
            bool noOrganizationUnitUuid,
            bool noSigned,
            bool noSignedAt,
            bool noSignedBy)
        {
            //Arrange
            var input = new UpdateContractRequestDTO();
            ConfigureResponsibleDataInputContext(noOrganizationUnitUuid, noSigned, noSignedAt, noSignedBy);

            //Act
            var output = _sut.FromPUT(input).Responsible.Value;

            //Assert
            Assert.True(output.OrganizationUnitUuid.HasChange);
            Assert.True(output.Signed.HasChange);
            Assert.True(output.SignedAt.HasChange);
            Assert.True(output.SignedBy.HasChange);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedSupplierDataPropertiesInput))]
        public void FromPOST_Ignores_Undefined_Properties_In_SupplierSection(
            bool noOrganizationUnitUuid,
            bool noSigned,
            bool noSignedAt,
            bool noSignedBy)
        {
            //Arrange
            var input = new CreateNewContractRequestDTO();
            ConfigureSupplierDataInputContext(noOrganizationUnitUuid, noSigned, noSignedAt, noSignedBy);

            //Act
            var output = _sut.FromPOST(input).Supplier.Value;

            //Assert
            Assert.Equal(noOrganizationUnitUuid, output.OrganizationUuid.IsUnchanged);
            Assert.Equal(noSigned, output.Signed.IsUnchanged);
            Assert.Equal(noSignedAt, output.SignedAt.IsUnchanged);
            Assert.Equal(noSignedBy, output.SignedBy.IsUnchanged);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedSupplierDataPropertiesInput))]
        public void FromPATCH_Ignores_Undefined_Properties_In_SupplierSection(
            bool noOrganizationUnitUuid,
            bool noSigned,
            bool noSignedAt,
            bool noSignedBy)
        {
            //Arrange
            var input = new UpdateContractRequestDTO();
            ConfigureSupplierDataInputContext(noOrganizationUnitUuid, noSigned, noSignedAt, noSignedBy);

            //Act
            var output = _sut.FromPATCH(input).Supplier.Value;

            //Assert
            Assert.Equal(noOrganizationUnitUuid, output.OrganizationUuid.IsUnchanged);
            Assert.Equal(noSigned, output.Signed.IsUnchanged);
            Assert.Equal(noSignedAt, output.SignedAt.IsUnchanged);
            Assert.Equal(noSignedBy, output.SignedBy.IsUnchanged);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedSupplierDataPropertiesInput))]
        public void FromPUT_Enforces_Undefined_Properties_In_SupplierSection(
            bool noOrganizationUnitUuid,
            bool noSigned,
            bool noSignedAt,
            bool noSignedBy)
        {
            //Arrange
            var input = new UpdateContractRequestDTO();
            ConfigureSupplierDataInputContext(noOrganizationUnitUuid, noSigned, noSignedAt, noSignedBy);

            //Act
            var output = _sut.FromPUT(input).Supplier.Value;

            //Assert
            Assert.True(output.OrganizationUuid.HasChange);
            Assert.True(output.Signed.HasChange);
            Assert.True(output.SignedAt.HasChange);
            Assert.True(output.SignedBy.HasChange);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedPaymentModelPropertiesInput))]
        public void FromPOST_Ignores_Undefined_Properties_In_PaymentModelSection(
            bool noOperationsRemunerationStartedAt,
            bool noPaymentFrequencyUuid,
            bool noPaymentModelUuid,
            bool noPriceRegulationUuid)
        {
            //Arrange
            var input = new CreateNewContractRequestDTO();
            ConfigurePaymentModelInputContext(noOperationsRemunerationStartedAt, noPaymentFrequencyUuid, noPaymentModelUuid, noPriceRegulationUuid);

            //Act
            var output = _sut.FromPOST(input).PaymentModel.Value;

            //Assert
            Assert.Equal(noOperationsRemunerationStartedAt, output.OperationsRemunerationStartedAt.IsUnchanged);
            Assert.Equal(noPaymentFrequencyUuid, output.PaymentFrequencyUuid.IsUnchanged);
            Assert.Equal(noPaymentModelUuid, output.PaymentModelUuid.IsUnchanged);
            Assert.Equal(noPriceRegulationUuid, output.PriceRegulationUuid.IsUnchanged);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedPaymentModelPropertiesInput))]
        public void FromPATCH_Ignores_Undefined_Properties_In_PaymentModelSection(
            bool noOperationsRemunerationStartedAt,
            bool noPaymentFrequencyUuid,
            bool noPaymentModelUuid,
            bool noPriceRegulationUuid)
        {
            //Arrange
            var input = new UpdateContractRequestDTO();
            ConfigurePaymentModelInputContext(noOperationsRemunerationStartedAt, noPaymentFrequencyUuid, noPaymentModelUuid, noPriceRegulationUuid);

            //Act
            var output = _sut.FromPATCH(input).PaymentModel.Value;

            //Assert
            Assert.Equal(noOperationsRemunerationStartedAt, output.OperationsRemunerationStartedAt.IsUnchanged);
            Assert.Equal(noPaymentFrequencyUuid, output.PaymentFrequencyUuid.IsUnchanged);
            Assert.Equal(noPaymentModelUuid, output.PaymentModelUuid.IsUnchanged);
            Assert.Equal(noPriceRegulationUuid, output.PriceRegulationUuid.IsUnchanged);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedPaymentModelPropertiesInput))]
        public void FromPUT_Enforces_Undefined_Properties_In_PaymentSection(
            bool noOperationsRemunerationStartedAt,
            bool noPaymentFrequencyUuid,
            bool noPaymentModelUuid,
            bool noPriceRegulationUuid)
        {
            //Arrange
            var input = new UpdateContractRequestDTO();
            ConfigurePaymentModelInputContext(noOperationsRemunerationStartedAt, noPaymentFrequencyUuid, noPaymentModelUuid, noPriceRegulationUuid);

            //Act
            var output = _sut.FromPUT(input).PaymentModel.Value;

            //Assert
            Assert.True(output.OperationsRemunerationStartedAt.HasChange);
            Assert.True(output.PaymentFrequencyUuid.HasChange);
            Assert.True(output.PaymentModelUuid.HasChange);
            Assert.True(output.PriceRegulationUuid.HasChange);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedAgreementPeriodPropertiesInput))]
        public void FromPOST_Ignores_Undefined_Properties_In_AgreementPeriodSection(
            bool noDurationMonths,
            bool noDurationYears,
            bool noExtensionOptionsUsed,
            bool noExtensionOptionsUuid,
            bool noIrrevocableUntil,
            bool noIsContinuous)
        {
            //Arrange
            var input = new CreateNewContractRequestDTO();
            ConfigureAgreementPeriodInputContext(noDurationMonths, noDurationYears, noExtensionOptionsUsed, noExtensionOptionsUuid, noIrrevocableUntil, noIsContinuous);

            //Act
            var output = _sut.FromPOST(input).AgreementPeriod.Value;

            //Assert
            Assert.Equal(noDurationMonths, output.DurationMonths.IsUnchanged);
            Assert.Equal(noDurationYears, output.DurationYears.IsUnchanged);
            Assert.Equal(noExtensionOptionsUsed, output.ExtensionOptionsUsed.IsUnchanged);
            Assert.Equal(noExtensionOptionsUuid, output.ExtensionOptionsUuid.IsUnchanged);
            Assert.Equal(noIrrevocableUntil, output.IrrevocableUntil.IsUnchanged);
            Assert.Equal(noIsContinuous, output.IsContinuous.IsUnchanged);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedAgreementPeriodPropertiesInput))]
        public void FromPATCH_Ignores_Undefined_Properties_In_AgreementPeriodSection(
            bool noDurationMonths,
            bool noDurationYears,
            bool noExtensionOptionsUsed,
            bool noExtensionOptionsUuid,
            bool noIrrevocableUntil,
            bool noIsContinuous)
        {
            //Arrange
            var input = new UpdateContractRequestDTO();
            ConfigureAgreementPeriodInputContext(noDurationMonths, noDurationYears, noExtensionOptionsUsed, noExtensionOptionsUuid, noIrrevocableUntil, noIsContinuous);

            //Act
            var output = _sut.FromPATCH(input).AgreementPeriod.Value;

            //Assert
            Assert.Equal(noDurationMonths, output.DurationMonths.IsUnchanged);
            Assert.Equal(noDurationYears, output.DurationYears.IsUnchanged);
            Assert.Equal(noExtensionOptionsUsed, output.ExtensionOptionsUsed.IsUnchanged);
            Assert.Equal(noExtensionOptionsUuid, output.ExtensionOptionsUuid.IsUnchanged);
            Assert.Equal(noIrrevocableUntil, output.IrrevocableUntil.IsUnchanged);
            Assert.Equal(noIsContinuous, output.IsContinuous.IsUnchanged);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedAgreementPeriodPropertiesInput))]
        public void FromPUT_Enforces_Undefined_Properties_In_AgreementPeriodSection(
            bool noDurationMonths,
            bool noDurationYears,
            bool noExtensionOptionsUsed,
            bool noExtensionOptionsUuid,
            bool noIrrevocableUntil,
            bool noIsContinuous)
        {
            //Arrange
            var input = new UpdateContractRequestDTO();
            ConfigureAgreementPeriodInputContext(noDurationMonths, noDurationYears, noExtensionOptionsUsed, noExtensionOptionsUuid, noIrrevocableUntil, noIsContinuous);

            //Act
            var output = _sut.FromPUT(input).AgreementPeriod.Value;

            //Assert
            Assert.True(output.DurationMonths.HasChange);
            Assert.True(output.DurationYears.HasChange);
            Assert.True(output.ExtensionOptionsUsed.HasChange);
            Assert.True(output.ExtensionOptionsUuid.HasChange);
            Assert.True(output.IrrevocableUntil.HasChange);
            Assert.True(output.IsContinuous.HasChange);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedTerminationDataPropertiesInput))]
        public void FromPOST_Ignores_Undefined_Properties_In_TerminationDataSection(
            bool noTerminatedAt,
            bool noNoticePeriodMonthsUuid,
            bool noNoticePeriodExtendsCurrent,
            bool noNoticeByEndOf)
        {
            //Arrange
            var input = new CreateNewContractRequestDTO();
            ConfigureTerminationDataInputContext(noTerminatedAt, noNoticePeriodMonthsUuid, noNoticePeriodExtendsCurrent, noNoticeByEndOf);

            //Act
            var output = _sut.FromPOST(input).Termination.Value;

            //Assert
            Assert.Equal(noTerminatedAt, output.TerminatedAt.IsUnchanged);
            Assert.Equal(noNoticePeriodMonthsUuid, output.NoticePeriodMonthsUuid.IsUnchanged);
            Assert.Equal(noNoticePeriodExtendsCurrent, output.NoticePeriodExtendsCurrent.IsUnchanged);
            Assert.Equal(noNoticeByEndOf, output.NoticeByEndOf.IsUnchanged);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedTerminationDataPropertiesInput))]
        public void FromPATCH_Ignores_Undefined_Properties_In_TerminationDataSection(
            bool noTerminatedAt,
            bool noNoticePeriodMonthsUuid,
            bool noNoticePeriodExtendsCurrent,
            bool noNoticeByEndOf)
        {
            //Arrange
            var input = new UpdateContractRequestDTO();
            ConfigureTerminationDataInputContext(noTerminatedAt, noNoticePeriodMonthsUuid, noNoticePeriodExtendsCurrent, noNoticeByEndOf);

            //Act
            var output = _sut.FromPATCH(input).Termination.Value;

            //Assert
            Assert.Equal(noTerminatedAt, output.TerminatedAt.IsUnchanged);
            Assert.Equal(noNoticePeriodMonthsUuid, output.NoticePeriodMonthsUuid.IsUnchanged);
            Assert.Equal(noNoticePeriodExtendsCurrent, output.NoticePeriodExtendsCurrent.IsUnchanged);
            Assert.Equal(noNoticeByEndOf, output.NoticeByEndOf.IsUnchanged);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedTerminationDataPropertiesInput))]
        public void FromPUT_Enforces_Undefined_Properties_In_TerminationDataSection(
            bool noTerminatedAt,
            bool noNoticePeriodMonthsUuid,
            bool noNoticePeriodExtendsCurrent,
            bool noNoticeByEndOf)
        {
            //Arrange
            var input = new UpdateContractRequestDTO();
            ConfigureTerminationDataInputContext(noTerminatedAt, noNoticePeriodMonthsUuid, noNoticePeriodExtendsCurrent, noNoticeByEndOf);

            //Act
            var output = _sut.FromPUT(input).Termination.Value;

            //Assert
            Assert.True(output.TerminatedAt.HasChange);
            Assert.True(output.NoticePeriodMonthsUuid.HasChange);
            Assert.True(output.NoticePeriodExtendsCurrent.HasChange);
            Assert.True(output.NoticeByEndOf.HasChange);
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

        [Fact]
        public void FromPatch_Maps_General()
        {
            //Arrange
            var input = new UpdateContractRequestDTO()
            {
                General = A<ContractGeneralDataWriteRequestDTO>()
            };

            //Act
            var output = _sut.FromPATCH(input).General;

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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Map_Parent_From_Patch(bool hasParentUuid)
        {
            //Arrange
            var parentUuid = hasParentUuid ? A<Guid?>() : null;
            var requestDto = new UpdateContractRequestDTO { ParentContractUuid = parentUuid };

            //Act
            var modificationParameters = _sut.FromPATCH(requestDto);

            //Assert
            Assert.Equal(requestDto.ParentContractUuid, AssertPropertyContainsDataChange(modificationParameters.ParentContractUuid));
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
        public void Can_Map_Responsible_FromPATCH()
        {
            //Arrange
            var input = A<ContractResponsibleDataWriteRequestDTO>();

            //Act
            var output = _sut.FromPATCH(new UpdateContractRequestDTO { Responsible = input });

            //Assert
            AssertResponsible(input, output.Responsible.Value);
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
        public void Can_Map_Supplier_FromPATCH()
        {
            //Arrange
            var input = A<ContractSupplierDataWriteRequestDTO>();

            //Act
            var output = _sut.FromPATCH(new UpdateContractRequestDTO { Supplier = input });

            //Assert
            AssertSupplier(input, output.Supplier.Value);
        }

        private UpdateContractRequestDTO ConfigureRequestInput(bool noName, bool noGeneralData, bool noParent,
           bool noResponsible, bool noProcurement, bool noSupplier, bool noSystemUsages,
           bool noExternalReferences, bool noDataProcessingRegistrations, bool noRoles, bool noPaymentModel,
           bool noAgreementPeriod, bool noPayments, bool noTermination)
        {
            var rootProperties = GetRootProperties();

            if (noName) rootProperties.Remove(nameof(UpdateContractRequestDTO.Name));
            if (noGeneralData) rootProperties.Remove(nameof(UpdateContractRequestDTO.General));
            if (noParent) rootProperties.Remove(nameof(UpdateContractRequestDTO.ParentContractUuid));
            if (noResponsible) rootProperties.Remove(nameof(UpdateContractRequestDTO.Responsible));
            if (noProcurement) rootProperties.Remove(nameof(UpdateContractRequestDTO.Procurement));
            if (noSupplier) rootProperties.Remove(nameof(UpdateContractRequestDTO.Supplier));
            if (noExternalReferences) rootProperties.Remove(nameof(UpdateContractRequestDTO.ExternalReferences));
            if (noSystemUsages) rootProperties.Remove(nameof(UpdateContractRequestDTO.SystemUsageUuids));
            if (noDataProcessingRegistrations)
                rootProperties.Remove(nameof(UpdateContractRequestDTO.DataProcessingRegistrationUuids));
            if (noRoles) rootProperties.Remove(nameof(UpdateContractRequestDTO.Roles));
            if (noPaymentModel) rootProperties.Remove(nameof(UpdateContractRequestDTO.PaymentModel));
            if (noAgreementPeriod) rootProperties.Remove(nameof(UpdateContractRequestDTO.AgreementPeriod));
            if (noPayments) rootProperties.Remove(nameof(UpdateContractRequestDTO.Payments));
            if (noTermination) rootProperties.Remove(nameof(UpdateContractRequestDTO.Termination));
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(Enumerable.Empty<string>().AsParameterMatch())).Returns(rootProperties);
            var emptyInput = new UpdateContractRequestDTO();
            return emptyInput;
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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Map_Procurement_From_Patch(bool hasValues)
        {
            //Arrange
            var procurement = CreateProcurementRequest(hasValues);
            var requestDto = new UpdateContractRequestDTO { Procurement = procurement };

            //Act
            var modificationParameters = _sut.FromPATCH(requestDto);

            //Assert
            Assert.True(modificationParameters.Procurement.HasValue);
            var procurementDto = modificationParameters.Procurement.Value;
            AssertProcurement(hasValues, procurement, procurementDto);
        }

        [Fact]
        public void Can_Map_ExternalReferences_FromPUT()
        {
            //Arrange
            var references = Many<UpdateExternalReferenceDataWriteRequestDTO>().OrderBy(x => x.Url).ToList();

            //Act
            var mappedReferences = _sut.FromPUT(new UpdateContractRequestDTO { ExternalReferences = references }).ExternalReferences.Value.OrderBy(x => x.Url).ToList();

            //Assert
            AssertExternalReferences(mappedReferences, references);
        }

        [Fact]
        public void Can_Map_ExternalReferences_FromPATCH()
        {
            //Arrange
            var references = Many<UpdateExternalReferenceDataWriteRequestDTO>().OrderBy(x => x.Url).ToList();

            //Act
            var mappedReferences = _sut.FromPATCH(new UpdateContractRequestDTO { ExternalReferences = references }).ExternalReferences.Value.OrderBy(x => x.Url).ToList();

            //Assert
            AssertExternalReferences(mappedReferences, references);
        }

        [Fact]
        public void Can_Map_ExternalReferences_FromPOST()
        {
            //Arrange
            var references = Many<ExternalReferenceDataWriteRequestDTO>().OrderBy(x => x.Url).ToList();

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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Map_SystemUsages_From_Patch(bool hasValues)
        {
            //Arrange
            var systemUsageUuids = hasValues ? new[] { A<Guid>(), A<Guid>() } : new Guid[0];
            var requestDto = new UpdateContractRequestDTO { SystemUsageUuids = systemUsageUuids };

            //Act
            var modificationParameters = _sut.FromPATCH(requestDto);

            //Assert
            Assert.True(modificationParameters.SystemUsageUuids.HasValue);
            var modifiedUuids = modificationParameters.SystemUsageUuids.Value;
            AssertUuids(systemUsageUuids, modifiedUuids);
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
        public void Can_Map_Roles_FromPATCH()
        {
            //Arrange
            var roles = Many<RoleAssignmentRequestDTO>().OrderBy(x => x.RoleUuid).ToList();

            //Act
            var result = _sut.FromPATCH(new UpdateContractRequestDTO() { Roles = roles });

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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Map_DataProcessingRegistrations_From_Patch(bool hasValues)
        {
            //Arrange
            var dataProcessingRegistrationUuids = hasValues ? new[] { A<Guid>(), A<Guid>() } : new Guid[0];
            var requestDto = new UpdateContractRequestDTO { DataProcessingRegistrationUuids = dataProcessingRegistrationUuids };

            //Act
            var modificationParameters = _sut.FromPATCH(requestDto);

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
        public void Can_Map_PaymentModel_FromPOST(bool hasValues)
        {
            //Arrange
            var input = new ContractPaymentModelDataWriteRequestDTO()
            {
                OperationsRemunerationStartedAt = hasValues ? A<DateTime>() : null,
                PaymentFrequencyUuid = hasValues ? A<Guid>() : null,
                PaymentModelUuid = hasValues ? A<Guid>() : null,
                PriceRegulationUuid = hasValues ? A<Guid>() : null
            };

            //Act
            var output = _sut.FromPOST(new CreateNewContractRequestDTO() { PaymentModel = input });

            //Assert
            AssertPaymentModel(input, output.PaymentModel.Value);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Map_PaymentModel_FromPUT(bool hasValues)
        {
            //Arrange
            var input = new ContractPaymentModelDataWriteRequestDTO()
            {
                OperationsRemunerationStartedAt = hasValues ? A<DateTime>() : null,
                PaymentFrequencyUuid = hasValues ? A<Guid>() : null,
                PaymentModelUuid = hasValues ? A<Guid>() : null,
                PriceRegulationUuid = hasValues ? A<Guid>() : null
            };

            //Act
            var output = _sut.FromPUT(new UpdateContractRequestDTO { PaymentModel = input });

            //Assert
            AssertPaymentModel(input, output.PaymentModel.Value);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Map_PaymentModel_FromPATCH(bool hasValues)
        {
            //Arrange
            var input = new ContractPaymentModelDataWriteRequestDTO()
            {
                OperationsRemunerationStartedAt = hasValues ? A<DateTime>() : null,
                PaymentFrequencyUuid = hasValues ? A<Guid>() : null,
                PaymentModelUuid = hasValues ? A<Guid>() : null,
                PriceRegulationUuid = hasValues ? A<Guid>() : null
            };

            //Act
            var output = _sut.FromPATCH(new UpdateContractRequestDTO { PaymentModel = input });

            //Assert
            AssertPaymentModel(input, output.PaymentModel.Value);
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
        public void Can_Map_Agreement_Period_FromPATCH()
        {
            //Arrange
            var input = A<ContractAgreementPeriodDataWriteRequestDTO>();

            //Act
            var output = _sut.FromPATCH(new UpdateContractRequestDTO() { AgreementPeriod = input });

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
        public void Can_Map_Payments_FromPATCH()
        {
            //Arrange
            var input = A<ContractPaymentsDataWriteRequestDTO>();

            //Act
            var output = _sut.FromPATCH(new UpdateContractRequestDTO { Payments = input }).Payments.Value;

            //Assert
            AssertPayments(input, output);
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
        public void Can_Map_Termination_FromPATCH()
        {
            //Arrange
            var input = A<ContractTerminationDataWriteRequestDTO>();

            //Act
            var output = _sut.FromPATCH(new UpdateContractRequestDTO() { Termination = input });

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

        private static void AssertPaymentModel(ContractPaymentModelDataWriteRequestDTO input, ItContractPaymentModelModificationParameters output)
        {
            Assert.Equal(input.PaymentFrequencyUuid, output.PaymentFrequencyUuid.NewValue);
            Assert.Equal(input.PaymentModelUuid, output.PaymentModelUuid.NewValue);
            Assert.Equal(input.PriceRegulationUuid, output.PriceRegulationUuid.NewValue);
            Assert.Equal(input.OperationsRemunerationStartedAt, output.OperationsRemunerationStartedAt.NewValue.Match(val => val, () => (DateTime?)null));
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

        private static void AssertExternalReferences<T>(IReadOnlyList<UpdatedExternalReferenceProperties> mappedReferences, IReadOnlyList<T> references) where T : ExternalReferenceDataWriteRequestDTO
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

                if (expected is UpdateExternalReferenceDataWriteRequestDTO expectedUpdateReference)
                {
                    Assert.Equal(expectedUpdateReference.Uuid, actual.Uuid);
                }
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
            Assert.Equal(input.Validity.RequireValidParent, AssertPropertyContainsDataChange(output.RequireValidParent));
            Assert.Equal(input.CriticalityUuid, AssertPropertyContainsDataChange(output.CriticalityUuid));
        }

        private static void AssertProcurement(bool hasValues, ContractProcurementDataWriteRequestDTO expected, ItContractProcurementModificationParameters actual)
        {
            Assert.Equal(expected.ProcurementStrategyUuid, AssertPropertyContainsDataChange(actual.ProcurementStrategyUuid));
            Assert.Equal(expected.PurchaseTypeUuid, AssertPropertyContainsDataChange(actual.PurchaseTypeUuid));

            if (hasValues)
            {
                var (half, year) = AssertPropertyContainsDataChange(actual.ProcurementPlan);
                Assert.Equal(expected.ProcurementPlan.QuarterOfYear, half);
                Assert.Equal(expected.ProcurementPlan.Year, year);
                Assert.Equal(expected.ProcurementInitiated, AssertPropertyContainsDataChange(actual.ProcurementInitiated).ToYesNoUndecidedChoice());
            }
            else
            {
                AssertPropertyContainsResetDataChange(actual.ProcurementPlan);
                AssertPropertyContainsResetDataChange(actual.ProcurementInitiated);
            }
        }

        private ContractProcurementDataWriteRequestDTO CreateProcurementRequest(bool hasValues)
        {
            return new ContractProcurementDataWriteRequestDTO
            {
                ProcurementStrategyUuid = hasValues ? A<Guid>() : null,
                PurchaseTypeUuid = hasValues ? A<Guid>() : null,
                ProcurementInitiated = hasValues ? A<YesNoUndecidedChoice>() : null,
                ProcurementPlan = hasValues
                    ? new ProcurementPlanDTO
                    {
                        QuarterOfYear = Convert.ToByte(A<int>() % 1 + 1),
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
        private void ConfigureGeneralDataInputContext(
            bool noContractId,
            bool noContractTypeUuid,
            bool noContractTemplateUuid,
            bool noAgreementElementUuids,
            bool noNotes,
            bool noEnforceValid,
            bool noValidFrom,
            bool noValidTo,
            bool noCriticalityTypeUuid,
            bool noRequireValidParent)
        {
            var sectionProperties = GetAllInputPropertyNames<ContractGeneralDataWriteRequestDTO>();
            var validitySectionProperties = GetAllInputPropertyNames<ContractValidityWriteRequestDTO>();

            if (noContractId) sectionProperties.Remove(nameof(ContractGeneralDataWriteRequestDTO.ContractId));
            if (noContractTypeUuid) sectionProperties.Remove(nameof(ContractGeneralDataWriteRequestDTO.ContractTypeUuid));
            if (noContractTemplateUuid) sectionProperties.Remove(nameof(ContractGeneralDataWriteRequestDTO.ContractTemplateUuid));
            if (noAgreementElementUuids) sectionProperties.Remove(nameof(ContractGeneralDataWriteRequestDTO.AgreementElementUuids));
            if (noNotes) sectionProperties.Remove(nameof(ContractGeneralDataWriteRequestDTO.Notes));
            if (noCriticalityTypeUuid) sectionProperties.Remove(nameof(ContractGeneralDataWriteRequestDTO.CriticalityUuid));

            if (noEnforceValid) validitySectionProperties.Remove(nameof(ContractValidityWriteRequestDTO.EnforcedValid));
            if (noValidFrom) validitySectionProperties.Remove(nameof(ContractValidityWriteRequestDTO.ValidFrom));
            if (noValidTo) validitySectionProperties.Remove(nameof(ContractValidityWriteRequestDTO.ValidTo));
            if (noRequireValidParent) validitySectionProperties.Remove(nameof(ContractValidityWriteRequestDTO.RequireValidParent));

            _currentHttpRequestMock
                .Setup(x => x.GetDefinedJsonProperties(nameof(UpdateContractRequestDTO.General).WrapAsEnumerable().AsParameterMatch()))
                .Returns(sectionProperties);
            _currentHttpRequestMock
                .Setup(x => x.GetDefinedJsonProperties(new[] { nameof(UpdateContractRequestDTO.General), nameof(ContractWriteRequestDTO.General.Validity) }.AsParameterMatch()))
                .Returns(validitySectionProperties);
        }

        private void ConfigureProcurementInputContext(
            bool noProcurementStrategyUuid,
            bool noPurchaseTypeUuid,
            bool noProcurementPlan,
            bool noProcurementInitiated)
        {
            var sectionProperties = GetAllInputPropertyNames<ContractProcurementDataWriteRequestDTO>();

            if (noProcurementStrategyUuid) sectionProperties.Remove(nameof(ContractProcurementDataWriteRequestDTO.ProcurementStrategyUuid));
            if (noPurchaseTypeUuid) sectionProperties.Remove(nameof(ContractProcurementDataWriteRequestDTO.PurchaseTypeUuid));
            if (noProcurementPlan) sectionProperties.Remove(nameof(ContractProcurementDataWriteRequestDTO.ProcurementPlan));
            if (noProcurementInitiated) sectionProperties.Remove(nameof(ContractProcurementDataWriteRequestDTO.ProcurementInitiated));

            _currentHttpRequestMock
                .Setup(x => x.GetDefinedJsonProperties(nameof(UpdateContractRequestDTO.Procurement).WrapAsEnumerable().AsParameterMatch()))
                .Returns(sectionProperties);
        }

        private void ConfigureResponsibleDataInputContext(
            bool noOrganizationUnitUuid,
            bool noSigned,
            bool noSignedAt,
            bool noSignedBy)
        {
            var sectionProperties = GetAllInputPropertyNames<ContractResponsibleDataWriteRequestDTO>();

            if (noOrganizationUnitUuid) sectionProperties.Remove(nameof(ContractResponsibleDataWriteRequestDTO.OrganizationUnitUuid));
            if (noSigned) sectionProperties.Remove(nameof(ContractResponsibleDataWriteRequestDTO.Signed));
            if (noSignedAt) sectionProperties.Remove(nameof(ContractResponsibleDataWriteRequestDTO.SignedAt));
            if (noSignedBy) sectionProperties.Remove(nameof(ContractResponsibleDataWriteRequestDTO.SignedBy));

            _currentHttpRequestMock
                .Setup(x => x.GetDefinedJsonProperties(nameof(UpdateContractRequestDTO.Responsible).WrapAsEnumerable().AsParameterMatch()))
                .Returns(sectionProperties);
        }

        private void ConfigureSupplierDataInputContext(
            bool noOrganizationUuid,
            bool noSigned,
            bool noSignedAt,
            bool noSignedBy)
        {
            var sectionProperties = GetAllInputPropertyNames<ContractSupplierDataWriteRequestDTO>();

            if (noOrganizationUuid) sectionProperties.Remove(nameof(ContractSupplierDataWriteRequestDTO.OrganizationUuid));
            if (noSigned) sectionProperties.Remove(nameof(ContractSupplierDataWriteRequestDTO.Signed));
            if (noSignedAt) sectionProperties.Remove(nameof(ContractSupplierDataWriteRequestDTO.SignedAt));
            if (noSignedBy) sectionProperties.Remove(nameof(ContractSupplierDataWriteRequestDTO.SignedBy));

            _currentHttpRequestMock
                .Setup(x => x.GetDefinedJsonProperties(nameof(UpdateContractRequestDTO.Supplier).WrapAsEnumerable().AsParameterMatch()))
                .Returns(sectionProperties);
        }

        private void ConfigurePaymentModelInputContext(
            bool noOperationsRemunerationStartedAt,
            bool noPaymentFrequencyUuid,
            bool noPaymentModelUuid,
            bool noPriceRegulationUuid)
        {
            var sectionProperties = GetAllInputPropertyNames<ContractPaymentModelDataWriteRequestDTO>();

            if (noOperationsRemunerationStartedAt) sectionProperties.Remove(nameof(ContractPaymentModelDataWriteRequestDTO.OperationsRemunerationStartedAt));
            if (noPaymentFrequencyUuid) sectionProperties.Remove(nameof(ContractPaymentModelDataWriteRequestDTO.PaymentFrequencyUuid));
            if (noPaymentModelUuid) sectionProperties.Remove(nameof(ContractPaymentModelDataWriteRequestDTO.PaymentModelUuid));
            if (noPriceRegulationUuid) sectionProperties.Remove(nameof(ContractPaymentModelDataWriteRequestDTO.PriceRegulationUuid));

            _currentHttpRequestMock
                .Setup(x => x.GetDefinedJsonProperties(nameof(UpdateContractRequestDTO.PaymentModel).WrapAsEnumerable().AsParameterMatch()))
                .Returns(sectionProperties);
        }

        private void ConfigureAgreementPeriodInputContext(
            bool noDurationMonths,
            bool noDurationYears,
            bool noExtensionOptionsUsed,
            bool noExtensionOptionsUuid,
            bool noIrrevocableUntil,
            bool noIsContinuous)
        {
            var sectionProperties = GetAllInputPropertyNames<ContractAgreementPeriodDataWriteRequestDTO>();

            if (noDurationMonths) sectionProperties.Remove(nameof(ContractAgreementPeriodDataWriteRequestDTO.DurationMonths));
            if (noDurationYears) sectionProperties.Remove(nameof(ContractAgreementPeriodDataWriteRequestDTO.DurationYears));
            if (noExtensionOptionsUsed) sectionProperties.Remove(nameof(ContractAgreementPeriodDataWriteRequestDTO.ExtensionOptionsUsed));
            if (noExtensionOptionsUuid) sectionProperties.Remove(nameof(ContractAgreementPeriodDataWriteRequestDTO.ExtensionOptionsUuid));
            if (noIrrevocableUntil) sectionProperties.Remove(nameof(ContractAgreementPeriodDataWriteRequestDTO.IrrevocableUntil));
            if (noIsContinuous) sectionProperties.Remove(nameof(ContractAgreementPeriodDataWriteRequestDTO.IsContinuous));

            _currentHttpRequestMock
                .Setup(x => x.GetDefinedJsonProperties(nameof(UpdateContractRequestDTO.AgreementPeriod).WrapAsEnumerable().AsParameterMatch()))
                .Returns(sectionProperties);
        }

        private void ConfigureTerminationDataInputContext(
            bool noTerminatedAt,
            bool noNoticePeriodMonthsUuid,
            bool noNoticePeriodExtendsCurrent,
            bool noNoticeByEndOf)
        {
            var sectionProperties = GetAllInputPropertyNames<ContractTerminationDataWriteRequestDTO>();
            var termsSectionProperties = GetAllInputPropertyNames<ContractTerminationTermsRequestDTO>();

            if (noTerminatedAt) sectionProperties.Remove(nameof(ContractTerminationDataWriteRequestDTO.TerminatedAt));

            if (noNoticePeriodMonthsUuid) termsSectionProperties.Remove(nameof(ContractTerminationTermsRequestDTO.NoticePeriodMonthsUuid));
            if (noNoticePeriodExtendsCurrent) termsSectionProperties.Remove(nameof(ContractTerminationTermsRequestDTO.NoticePeriodExtendsCurrent));
            if (noNoticeByEndOf) termsSectionProperties.Remove(nameof(ContractTerminationTermsRequestDTO.NoticeByEndOf));

            _currentHttpRequestMock
                .Setup(x => x.GetDefinedJsonProperties(nameof(UpdateContractRequestDTO.Termination).WrapAsEnumerable().AsParameterMatch()))
                .Returns(sectionProperties);
            _currentHttpRequestMock
                .Setup(x => x.GetDefinedJsonProperties(new[] { nameof(UpdateContractRequestDTO.Termination), nameof(UpdateContractRequestDTO.Termination.Terms) }.AsParameterMatch()))
                .Returns(termsSectionProperties);
        }
    }
}
