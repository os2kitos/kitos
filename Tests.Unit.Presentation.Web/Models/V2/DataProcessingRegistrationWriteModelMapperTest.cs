using System;
using System.Collections.Generic;
using Core.ApplicationServices.Model.GDPR.Write;
using Core.ApplicationServices.Model.Shared;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared.Write;
using Moq;
using Newtonsoft.Json.Linq;
using Presentation.Web.Controllers.API.V2.External.DataProcessingRegistrations.Mapping;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Request.DataProcessing;
using Presentation.Web.Models.API.V2.Request.Generic.ExternalReferences;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;
using Presentation.Web.Models.API.V2.Types.DataProcessing;
using Tests.Toolkit.Extensions;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    public class DataProcessingRegistrationWriteModelMapperTest : WriteModelMapperTestBase
    {
        private readonly DataProcessingRegistrationWriteModelMapper _sut;
        private readonly Mock<ICurrentHttpRequest> _currentHttpRequestMock;

        public DataProcessingRegistrationWriteModelMapperTest()
        {
            _currentHttpRequestMock = new Mock<ICurrentHttpRequest>();
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(Enumerable.Empty<string>().AsParameterMatch())).Returns(GetAllInputPropertyNames<UpdateDataProcessingRegistrationRequestDTO>());
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(nameof(UpdateDataProcessingRegistrationRequestDTO.General).WrapAsEnumerable().AsParameterMatch())).Returns(GetAllInputPropertyNames<DataProcessingRegistrationGeneralDataWriteRequestDTO>());
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(nameof(UpdateDataProcessingRegistrationRequestDTO.Oversight).WrapAsEnumerable().AsParameterMatch())).Returns(GetAllInputPropertyNames<DataProcessingRegistrationOversightWriteRequestDTO>());
            _currentHttpRequestMock.Setup(x => x.GetObject(It.IsAny<IEnumerable<string>>())).Returns(Maybe<JToken>.None);
            _sut = new DataProcessingRegistrationWriteModelMapper(_currentHttpRequestMock.Object);
        }

        [Fact]
        public void MapGeneral_Returns_UpdatedDataProcessingRegistrationGeneralDataParameters()
        {
            //Arrange
            var input = A<DataProcessingRegistrationGeneralDataWriteRequestDTO>();

            //Act
            var output = _sut.FromPATCH(new UpdateDataProcessingRegistrationRequestDTO() { General = input });

            //Assert
            AssertGeneralData(input, AssertPropertyContainsDataChange(output.General));
        }

        [Fact]
        public void MapGeneral__Resets_InsecureCountries_If_Input_Is_Null()
        {
            //Arrange
            var input = A<DataProcessingRegistrationGeneralDataWriteRequestDTO>();
            input.InsecureCountriesSubjectToDataTransferUuids = null;

            //Act
            var output = _sut.FromPATCH(new UpdateDataProcessingRegistrationRequestDTO() { General = input });

            //Assert
            AssertPropertyContainsResetDataChange(AssertPropertyContainsDataChange(output.General).InsecureCountriesSubjectToDataTransferUuids);
        }

        [Fact]
        public void MapGeneral__Resets_DataProcessors_If_Input_Is_Null()
        {
            //Arrange
            var input = A<DataProcessingRegistrationGeneralDataWriteRequestDTO>();
            input.DataProcessorUuids = null;

            //Act
            var output = _sut.FromPATCH(new UpdateDataProcessingRegistrationRequestDTO() { General = input });

            //Assert
            AssertPropertyContainsResetDataChange(AssertPropertyContainsDataChange(output.General).DataProcessorUuids);
        }

        [Fact]
        public void MapGeneral__Resets_SubDataProcessors_If_Input_Is_Null()
        {
            //Arrange
            var input = A<DataProcessingRegistrationGeneralDataWriteRequestDTO>();
            input.SubDataProcessors = null;

            //Act
            var output = _sut.FromPATCH(new UpdateDataProcessingRegistrationRequestDTO() { General = input });

            //Assert
            AssertPropertyContainsResetDataChange(AssertPropertyContainsDataChange(output.General).SubDataProcessors);
        }

        [Fact]
        public void FromPOST_Maps_All_Sections()
        {
            //Arrange
            var input = A<CreateDataProcessingRegistrationRequestDTO>();

            //Act
            var output = _sut.FromPOST(input);

            //Assert
            Assert.Equal(input.Name, AssertPropertyContainsDataChange(output.Name));
            AssertGeneralData(input.General, output.General.Value);
            AssertOversight(input.Oversight, AssertPropertyContainsDataChange(output.Oversight));
            AssertReferences(input.ExternalReferences.ToList(), AssertPropertyContainsDataChange(output.ExternalReferences).ToList());
            AssertRoles(input.Roles.ToList(), AssertPropertyContainsDataChange(output.Roles));
            Assert.Equal(input.SystemUsageUuids, output.SystemUsageUuids.Value);
        }

        [Fact]
        public void FromPUT_Maps_All_Sections()
        {
            //Arrange
            var input = A<UpdateDataProcessingRegistrationRequestDTO>();

            //Act
            var output = _sut.FromPUT(input);

            //Assert
            Assert.Equal(input.Name, AssertPropertyContainsDataChange(output.Name));
            AssertGeneralData(input.General, output.General.Value);
            AssertOversight(input.Oversight, AssertPropertyContainsDataChange(output.Oversight));
            AssertReferences(input.ExternalReferences.ToList(), AssertPropertyContainsDataChange(output.ExternalReferences).ToList());
            AssertRoles(input.Roles.ToList(), AssertPropertyContainsDataChange(output.Roles));
            Assert.Equal(input.SystemUsageUuids, output.SystemUsageUuids.Value);
        }

        [Fact]
        public void FromPATCH_Maps_All_Sections()
        {
            //Arrange
            var input = A<UpdateDataProcessingRegistrationRequestDTO>();

            //Act
            var output = _sut.FromPATCH(input);

            //Assert
            Assert.Equal(input.Name, AssertPropertyContainsDataChange(output.Name));
            AssertGeneralData(input.General, output.General.Value);
            AssertOversight(input.Oversight, AssertPropertyContainsDataChange(output.Oversight));
            AssertReferences(input.ExternalReferences.ToList(), AssertPropertyContainsDataChange(output.ExternalReferences).ToList());
            AssertRoles(input.Roles.ToList(), AssertPropertyContainsDataChange(output.Roles));
            Assert.Equal(input.SystemUsageUuids, output.SystemUsageUuids.Value);
        }

        public static IEnumerable<object[]> GetUndefinedSectionsInput()
        {
            return CreateGetUndefinedSectionsInput(6);
        }

        public static IEnumerable<object[]> GetUndefinedGeneralDataPropertiesInput()
        {
            return CreateGetUndefinedSectionsInput(13);
        }

        public static IEnumerable<object[]> GetUndefinedOversightDataPropertiesInput()
        {
            return CreateGetUndefinedSectionsInput(8);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedSectionsInput))]
        public void FromPATCH_Ignores_Undefined_Sections(
            bool noName,
            bool noGeneralData,
            bool noSystems,
            bool noOversight,
            bool noRoles,
            bool noReferences)
        {
            //Arrange
            var input = new UpdateDataProcessingRegistrationRequestDTO();
            ConfigureRootProperties(noName, noGeneralData, noSystems, noOversight, noRoles, noReferences);

            //Act
            var output = _sut.FromPATCH(input);

            //Assert that method patched empty values before mapping
            Assert.Equal(noName, output.Name.IsUnchanged);
            Assert.Equal(noGeneralData, output.General.IsNone);
            Assert.Equal(noSystems, output.SystemUsageUuids.IsNone);
            Assert.Equal(noOversight, output.Oversight.IsNone);
            Assert.Equal(noRoles, output.Roles.IsNone);
            Assert.Equal(noReferences, output.ExternalReferences.IsNone);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedSectionsInput))]
        public void FromPUT_Enforces_Undefined_Sections(
            bool noName,
            bool noGeneralData,
            bool noSystems,
            bool noOversight,
            bool noRoles,
            bool noReferences)
        {
            //Arrange
            var input = new UpdateDataProcessingRegistrationRequestDTO();
            ConfigureRootProperties(noName, noGeneralData, noSystems, noOversight, noRoles, noReferences);

            //Act
            var output = _sut.FromPUT(input);

            //Assert that method patched empty values before mapping
            Assert.True(output.Name.HasChange);
            Assert.True(output.General.HasValue);
            Assert.True(output.SystemUsageUuids.HasValue);
            Assert.True(output.Oversight.HasValue);
            Assert.True(output.Roles.HasValue);
            Assert.True(output.ExternalReferences.HasValue);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedSectionsInput))]
        public void FromPOST_Ignores_Undefined_Sections(
           bool noName,
           bool noGeneralData,
           bool noSystems,
           bool noOversight,
           bool noRoles,
           bool noReferences)
        {
            //Arrange
            var input = new CreateDataProcessingRegistrationRequestDTO();
            ConfigureRootProperties(noName, noGeneralData, noSystems, noOversight, noRoles, noReferences);

            //Act
            var output = _sut.FromPOST(input);

            //Assert that method patched empty values before mapping
            Assert.Equal(noName, output.Name.IsUnchanged);
            Assert.Equal(noGeneralData, output.General.IsNone);
            Assert.Equal(noSystems, output.SystemUsageUuids.IsNone);
            Assert.Equal(noOversight, output.Oversight.IsNone);
            Assert.Equal(noRoles, output.Roles.IsNone);
            Assert.Equal(noReferences, output.ExternalReferences.IsNone);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedGeneralDataPropertiesInput))]
        public void FromPATCH_Ignores_Undefined_Properties_In_GeneralSection(
            bool noDataResponsible,
            bool noDataResponsibleRemark,
            bool noAgreementConcluded,
            bool noAgreementConcludedRemark,
            bool noAgreementConcludedAt,
            bool noBasisForTransfer,
            bool noTransferToInsecureCountries,
            bool noInsecureCountries,
            bool noDataProcessors,
            bool noHasSubDataProcessors,
            bool noSubDataProcessors,
            bool noMainContract,
            bool noResponsibleUnit)
        {
            //Arrange
            var input = new UpdateDataProcessingRegistrationRequestDTO();
            ConfigureGeneralDataInputContext(noDataResponsible, noDataResponsibleRemark, noAgreementConcluded, noAgreementConcludedRemark, noAgreementConcludedAt, noBasisForTransfer, noTransferToInsecureCountries, noInsecureCountries, noDataProcessors, noHasSubDataProcessors, noSubDataProcessors, noMainContract, noResponsibleUnit);

            //Act
            var output = _sut.FromPATCH(input).General.Value;

            //Assert that method patched empty values before mapping
            Assert.Equal(noDataResponsible, output.DataResponsibleUuid.IsUnchanged);
            Assert.Equal(noDataResponsibleRemark, output.DataResponsibleRemark.IsUnchanged);
            Assert.Equal(noAgreementConcluded, output.IsAgreementConcluded.IsUnchanged);
            Assert.Equal(noAgreementConcludedRemark, output.IsAgreementConcludedRemark.IsUnchanged);
            Assert.Equal(noAgreementConcludedAt, output.AgreementConcludedAt.IsUnchanged);
            Assert.Equal(noBasisForTransfer, output.BasisForTransferUuid.IsUnchanged);
            Assert.Equal(noTransferToInsecureCountries, output.TransferToInsecureThirdCountries.IsUnchanged);
            Assert.Equal(noInsecureCountries, output.InsecureCountriesSubjectToDataTransferUuids.IsUnchanged);
            Assert.Equal(noDataProcessors, output.DataProcessorUuids.IsUnchanged);
            Assert.Equal(noHasSubDataProcessors, output.HasSubDataProcessors.IsUnchanged);
            Assert.Equal(noSubDataProcessors, output.SubDataProcessors.IsUnchanged);
            Assert.Equal(noResponsibleUnit, output.ResponsibleUnitUuid.IsUnchanged);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedGeneralDataPropertiesInput))]
        public void FromPOST_Ignores_Undefined_Properties_In_GeneralSection(
           bool noDataResponsible,
           bool noDataResponsibleRemark,
           bool noAgreementConcluded,
           bool noAgreementConcludedRemark,
           bool noAgreementConcludedAt,
           bool noBasisForTransfer,
           bool noTransferToInsecureCountries,
           bool noInsecureCountries,
           bool noDataProcessors,
           bool noHasSubDataProcessors,
           bool noSubDataProcessors,
           bool noMainContract,
           bool noResponsibleUnit)
        {
            //Arrange
            var input = new CreateDataProcessingRegistrationRequestDTO();
            ConfigureGeneralDataInputContext(noDataResponsible, noDataResponsibleRemark, noAgreementConcluded, noAgreementConcludedRemark, noAgreementConcludedAt, noBasisForTransfer, noTransferToInsecureCountries, noInsecureCountries, noDataProcessors, noHasSubDataProcessors, noSubDataProcessors, noMainContract, noResponsibleUnit);

            //Act
            var output = _sut.FromPOST(input).General.Value;

            //Assert that method patched empty values before mapping
            Assert.Equal(noDataResponsible, output.DataResponsibleUuid.IsUnchanged);
            Assert.Equal(noDataResponsibleRemark, output.DataResponsibleRemark.IsUnchanged);
            Assert.Equal(noAgreementConcluded, output.IsAgreementConcluded.IsUnchanged);
            Assert.Equal(noAgreementConcludedRemark, output.IsAgreementConcludedRemark.IsUnchanged);
            Assert.Equal(noAgreementConcludedAt, output.AgreementConcludedAt.IsUnchanged);
            Assert.Equal(noBasisForTransfer, output.BasisForTransferUuid.IsUnchanged);
            Assert.Equal(noTransferToInsecureCountries, output.TransferToInsecureThirdCountries.IsUnchanged);
            Assert.Equal(noInsecureCountries, output.InsecureCountriesSubjectToDataTransferUuids.IsUnchanged);
            Assert.Equal(noDataProcessors, output.DataProcessorUuids.IsUnchanged);
            Assert.Equal(noHasSubDataProcessors, output.HasSubDataProcessors.IsUnchanged);
            Assert.Equal(noSubDataProcessors, output.SubDataProcessors.IsUnchanged);
            Assert.Equal(noResponsibleUnit, output.ResponsibleUnitUuid.IsUnchanged);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedGeneralDataPropertiesInput))]
        public void FromPUT_Enforces_Undefined_Properties_In_GeneralSection(
            bool noDataResponsible,
            bool noDataResponsibleRemark,
            bool noAgreementConcluded,
            bool noAgreementConcludedRemark,
            bool noAgreementConcludedAt,
            bool noBasisForTransfer,
            bool noTransferToInsecureCountries,
            bool noInsecureCountries,
            bool noDataProcessors,
            bool noHasSubDataProcessors,
            bool noSubDataProcessors,
            bool noMainContract, 
            bool noResponsibleUnit)
        {
            //Arrange
            var input = new UpdateDataProcessingRegistrationRequestDTO();
            ConfigureGeneralDataInputContext(noDataResponsible, noDataResponsibleRemark, noAgreementConcluded, noAgreementConcludedRemark, noAgreementConcludedAt, noBasisForTransfer, noTransferToInsecureCountries, noInsecureCountries, noDataProcessors, noHasSubDataProcessors, noSubDataProcessors, noMainContract, noResponsibleUnit);

            //Act
            var output = _sut.FromPUT(input).General.Value;

            //Assert that method patched empty values before mapping
            Assert.True(output.DataResponsibleUuid.HasChange);
            Assert.True(output.DataResponsibleRemark.HasChange);
            Assert.True(output.IsAgreementConcluded.HasChange);
            Assert.True(output.IsAgreementConcludedRemark.HasChange);
            Assert.True(output.AgreementConcludedAt.HasChange);
            Assert.True(output.BasisForTransferUuid.HasChange);
            Assert.True(output.TransferToInsecureThirdCountries.HasChange);
            Assert.True(output.InsecureCountriesSubjectToDataTransferUuids.HasChange);
            Assert.True(output.DataProcessorUuids.HasChange);
            Assert.True(output.HasSubDataProcessors.HasChange);
            Assert.True(output.SubDataProcessors.HasChange);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedOversightDataPropertiesInput))]
        public void FromPATCH_Ignores_Undefined_Properties_In_Oversight_Section(
         bool noOversightOptionUuids,
         bool noOversightOptionRemark,
         bool noOversightInterval,
         bool noOversightIntervalRemark,
         bool noIsOversightCompleted,
         bool noOversightCompletedRemark,
         bool noOversightScheduledInspectionDate,
         bool noOversightDates)
        {
            //Arrange
            var input = new UpdateDataProcessingRegistrationRequestDTO();

            ConfigureOversightRequestContext(noOversightOptionUuids, noOversightOptionRemark, noOversightInterval, noOversightIntervalRemark, noIsOversightCompleted, noOversightCompletedRemark, noOversightScheduledInspectionDate, noOversightDates);

            //Act
            var output = _sut.FromPATCH(input).Oversight.Value;

            //Assert that method patched empty values before mapping
            Assert.Equal(noOversightOptionUuids, output.OversightOptionUuids.IsUnchanged);
            Assert.Equal(noOversightOptionRemark, output.OversightOptionsRemark.IsUnchanged);
            Assert.Equal(noOversightInterval, output.OversightInterval.IsUnchanged);
            Assert.Equal(noOversightIntervalRemark, output.OversightIntervalRemark.IsUnchanged);
            Assert.Equal(noIsOversightCompleted, output.IsOversightCompleted.IsUnchanged);
            Assert.Equal(noOversightCompletedRemark, output.OversightCompletedRemark.IsUnchanged);
            Assert.Equal(noOversightDates, output.OversightDates.IsUnchanged);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedOversightDataPropertiesInput))]
        public void FromPOST_Ignores_Undefined_Properties_In_Oversight_Section(
            bool noOversightOptionUuids,
            bool noOversightOptionRemark,
            bool noOversightInterval,
            bool noOversightIntervalRemark,
            bool noIsOversightCompleted,
            bool noOversightCompletedRemark,
            bool noOversightScheduledInspectionDate,
            bool noOversightDates)
        {
            //Arrange
            var input = new CreateDataProcessingRegistrationRequestDTO();

            ConfigureOversightRequestContext(noOversightOptionUuids, noOversightOptionRemark, noOversightInterval, noOversightIntervalRemark, noIsOversightCompleted, noOversightCompletedRemark, noOversightScheduledInspectionDate, noOversightDates);

            //Act
            var output = _sut.FromPOST(input).Oversight.Value;

            //Assert that method patched empty values before mapping
            Assert.Equal(noOversightOptionUuids, output.OversightOptionUuids.IsUnchanged);
            Assert.Equal(noOversightOptionRemark, output.OversightOptionsRemark.IsUnchanged);
            Assert.Equal(noOversightInterval, output.OversightInterval.IsUnchanged);
            Assert.Equal(noOversightIntervalRemark, output.OversightIntervalRemark.IsUnchanged);
            Assert.Equal(noIsOversightCompleted, output.IsOversightCompleted.IsUnchanged);
            Assert.Equal(noOversightCompletedRemark, output.OversightCompletedRemark.IsUnchanged);
            Assert.Equal(noOversightDates, output.OversightDates.IsUnchanged);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedOversightDataPropertiesInput))]
        public void FromPUT_Enforces_Changes_To_Properties_In_Oversight_Section(
            bool noOversightOptionUuids,
            bool noOversightOptionRemark,
            bool noOversightInterval,
            bool noOversightIntervalRemark,
            bool noIsOversightCompleted,
            bool noOversightCompletedRemark,
            bool noOversightScheduledInspectionDate,
            bool noOversightDates)
        {
            //Arrange
            var input = new UpdateDataProcessingRegistrationRequestDTO();

            ConfigureOversightRequestContext(noOversightOptionUuids, noOversightOptionRemark, noOversightInterval, noOversightIntervalRemark, noIsOversightCompleted, noOversightCompletedRemark, noOversightScheduledInspectionDate, noOversightDates);

            //Act
            var output = _sut.FromPUT(input).Oversight.Value;

            //Assert that method patched empty values before mapping
            Assert.True(output.OversightOptionUuids.HasChange);
            Assert.True(output.OversightOptionsRemark.HasChange);
            Assert.True(output.OversightInterval.HasChange);
            Assert.True(output.OversightIntervalRemark.HasChange);
            Assert.True(output.IsOversightCompleted.HasChange);
            Assert.True(output.OversightCompletedRemark.HasChange);
            Assert.True(output.OversightDates.HasChange);
        }

        [Fact]
        public void MapOversight_Returns_UpdatedDataProcessingRegistrationOversightDataParameters()
        {
            //Arrange
            var input = A<DataProcessingRegistrationOversightWriteRequestDTO>();

            //Act
            var output = _sut.FromPATCH(new UpdateDataProcessingRegistrationRequestDTO() { Oversight = input });

            //Assert
            AssertOversight(input, AssertPropertyContainsDataChange(output.Oversight));
        }

        [Fact]
        public void MapOversight_Resets_OversightOptions_If_Input_Is_Null()
        {
            //Arrange
            var input = A<DataProcessingRegistrationOversightWriteRequestDTO>();
            input.OversightOptionUuids = null;

            //Act
            var output = _sut.FromPATCH(new UpdateDataProcessingRegistrationRequestDTO() { Oversight = input });

            //Assert
            AssertPropertyContainsResetDataChange(AssertPropertyContainsDataChange(output.Oversight).OversightOptionUuids);
        }

        [Fact]
        public void MapOversight_Resets_OversightDates_If_Input_Is_Null()
        {
            //Arrange
            var input = A<DataProcessingRegistrationOversightWriteRequestDTO>();
            input.OversightDates = null;

            //Act
            var output = _sut.FromPATCH(new UpdateDataProcessingRegistrationRequestDTO() { Oversight = input });

            //Assert
            AssertPropertyContainsResetDataChange(AssertPropertyContainsDataChange(output.Oversight).OversightDates);
        }

        [Fact]
        public void MapRoles_Returns_UpdatedDataProcessingRegistrationRoles()
        {
            //Arrange
            var roles = Many<RoleAssignmentRequestDTO>().OrderBy(x => x.RoleUuid).ToList();

            //Act
            var output = _sut.FromPATCH(new UpdateDataProcessingRegistrationRequestDTO() { Roles = roles });

            //Assert
            AssertRoles(roles, AssertPropertyContainsDataChange(output.Roles));
        }

        [Fact]
        public void Can_Map_ExternalReferences()
        {
            //Arrange
            var references = Many<UpdateExternalReferenceDataWriteRequestDTO>().OrderBy(x => x.Url).ToList();

            //Act
            var output = _sut.FromPATCH(new UpdateDataProcessingRegistrationRequestDTO() { ExternalReferences = references });

            //Assert
            AssertReferences(references, AssertPropertyContainsDataChange(output.ExternalReferences).ToList());
        }

        private static void AssertReferences<T>(IReadOnlyList<T> references, IReadOnlyList<UpdatedExternalReferenceProperties> mappedReferences) where T : ExternalReferenceDataWriteRequestDTO
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

        private static void AssertGeneralData(DataProcessingRegistrationGeneralDataWriteRequestDTO input, UpdatedDataProcessingRegistrationGeneralDataParameters output)
        {
            Assert.Equal(input.DataResponsibleUuid, AssertPropertyContainsDataChange(output.DataResponsibleUuid));
            Assert.Equal(input.DataResponsibleRemark, AssertPropertyContainsDataChange(output.DataResponsibleRemark));
            Assert.Equal(input.IsAgreementConcluded?.ToYesNoIrrelevantOption(), AssertPropertyContainsDataChange(output.IsAgreementConcluded));
            Assert.Equal(input.IsAgreementConcludedRemark, AssertPropertyContainsDataChange(output.IsAgreementConcludedRemark));
            Assert.Equal(input.AgreementConcludedAt, AssertPropertyContainsDataChange(output.AgreementConcludedAt));
            Assert.Equal(input.BasisForTransferUuid, AssertPropertyContainsDataChange(output.BasisForTransferUuid));
            Assert.Equal(input.TransferToInsecureThirdCountries?.ToYesNoUndecidedOption(), AssertPropertyContainsDataChange(output.TransferToInsecureThirdCountries));
            AssertNullableCollection(input.InsecureCountriesSubjectToDataTransferUuids, output.InsecureCountriesSubjectToDataTransferUuids);
            AssertNullableCollection(input.DataProcessorUuids, output.DataProcessorUuids);
            Assert.Equal(input.HasSubDataProcessors?.ToYesNoUndecidedOption(), AssertPropertyContainsDataChange(output.HasSubDataProcessors));
            AssertNullableSubDataProcessorCollection(input.SubDataProcessors, output.SubDataProcessors);
            Assert.Equal(input.MainContractUuid, AssertPropertyContainsDataChange(output.MainContractUuid));
            Assert.Equal(input.ResponsibleOrganizationUnitUuid, AssertPropertyContainsDataChange(output.ResponsibleUnitUuid));
        }

        private static void AssertNullableSubDataProcessorCollection(IEnumerable<DataProcessorRegistrationSubDataProcessorWriteRequestDTO> inputSubDataProcessors, OptionalValueChange<Maybe<IEnumerable<SubDataProcessorParameter>>> outputSubDataProcessors)
        {
            if (inputSubDataProcessors == null)
                AssertPropertyContainsResetDataChange(outputSubDataProcessors);
            else
            {
                var mappedChanges = AssertPropertyContainsDataChange(outputSubDataProcessors).ToList();
                var inputSdps = inputSubDataProcessors.ToList();
                Assert.Equal(inputSdps.Count, mappedChanges.Count);
                var inputByOrgUuid = inputSdps.ToDictionary(x => x.DataProcessorOrganizationUuid);
                var outputByUuid = mappedChanges.ToDictionary(x => x.OrganizationUuid);
                Assert.Equivalent(inputByOrgUuid.Keys.OrderBy(x => x), outputByUuid.Keys.OrderBy(x => x));
                foreach (var inputSdp in inputByOrgUuid)
                {
                    var expected = inputSdp.Value;
                    var actual = outputByUuid[inputSdp.Key];
                    Assert.Equal(expected.TransferToInsecureThirdCountry?.ToYesNoUndecidedOption(), actual.TransferToInsecureThirdCountry);
                    Assert.Equal(expected.BasisForTransferUuid, actual.BasisForTransferOptionUuid);
                    Assert.Equal(expected.InsecureThirdCountrySubjectToDataProcessingUuid, actual.InsecureCountrySubjectToDataTransferUuid);
                }
            }
        }

        private static void AssertNullableCollection(IEnumerable<Guid> fromCollection, OptionalValueChange<Maybe<IEnumerable<Guid>>> actualCollection)
        {
            if (fromCollection == null)
                AssertPropertyContainsResetDataChange(actualCollection);
            else
                Assert.Equal(fromCollection,
                    AssertPropertyContainsDataChange(actualCollection));
        }

        private static void AssertOversightDates(IEnumerable<OversightDateDTO> expected, IEnumerable<UpdatedDataProcessingRegistrationOversightDate> actual)
        {
            var orderedExpected = expected.OrderBy(x => x.CompletedAt).ToList();
            var orderedActual = actual.OrderBy(x => x.CompletedAt).ToList();

            Assert.Equal(orderedExpected.Count, orderedActual.Count);
            for (var i = 0; i < orderedExpected.Count; i++)
            {
                Assert.Equal(orderedExpected[i].CompletedAt, orderedActual[i].CompletedAt);
                Assert.Equal(orderedExpected[i].Remark, orderedActual[i].Remark);
            }
        }

        private static void AssertOversight(DataProcessingRegistrationOversightWriteRequestDTO input,
            UpdatedDataProcessingRegistrationOversightDataParameters output)
        {
            Assert.Equal(input.OversightOptionUuids, AssertPropertyContainsDataChange(output.OversightOptionUuids));
            Assert.Equal(input.OversightOptionsRemark, AssertPropertyContainsDataChange(output.OversightOptionsRemark));
            Assert.Equal(input.OversightInterval?.ToIntervalOption(),
                AssertPropertyContainsDataChange(output.OversightInterval));
            Assert.Equal(input.OversightIntervalRemark, AssertPropertyContainsDataChange(output.OversightIntervalRemark));
            Assert.Equal(input.IsOversightCompleted?.ToYesNoUndecidedOption(),
                AssertPropertyContainsDataChange(output.IsOversightCompleted));
            Assert.Equal(input.OversightCompletedRemark, AssertPropertyContainsDataChange(output.OversightCompletedRemark));
            Assert.Equal(input.OversightScheduledInspectionDate, AssertPropertyContainsDataChange(output.OversightScheduledInspectionDate));
            AssertOversightDates(input.OversightDates, AssertPropertyContainsDataChange(output.OversightDates));
        }

        private static void AssertRoles(IReadOnlyList<RoleAssignmentRequestDTO> expectedRoles, UpdatedDataProcessingRegistrationRoles actualRoles)
        {
            var inputRoles = expectedRoles.OrderBy(x => x.RoleUuid).ToList();
            var mappedRoles = AssertPropertyContainsDataChange(actualRoles.UserRolePairs).OrderBy(x => x.RoleUuid).ToList();
            Assert.Equal(expectedRoles.Count, mappedRoles.Count);
            for (var i = 0; i < mappedRoles.Count; i++)
            {
                var expected = inputRoles[i];
                var actual = mappedRoles[i];
                Assert.Equal(expected.RoleUuid, actual.RoleUuid);
                Assert.Equal(expected.UserUuid, actual.UserUuid);
            }
        }

        private void ConfigureGeneralDataInputContext(
           bool noDataResponsible,
           bool noDataResponsibleRemark,
           bool noAgreementConcluded,
           bool noAgreementConcludedRemark,
           bool noAgreementConcludedAt,
           bool noBasisForTransfer,
           bool noTransferToInsecureCountries,
           bool noInsecureCountries,
           bool noDataProcessors,
           bool noHasSubDataProcessors,
           bool noSubDataProcessors,
           bool noMainContract,
           bool noResponsibleUnit)
        {
            var sectionProperties = GetAllInputPropertyNames<DataProcessingRegistrationGeneralDataWriteRequestDTO>();

            if (noDataResponsible) sectionProperties.Remove(nameof(DataProcessingRegistrationGeneralDataWriteRequestDTO.DataResponsibleUuid));
            if (noDataResponsibleRemark) sectionProperties.Remove(nameof(DataProcessingRegistrationGeneralDataWriteRequestDTO.DataResponsibleRemark));
            if (noAgreementConcluded) sectionProperties.Remove(nameof(DataProcessingRegistrationGeneralDataWriteRequestDTO.IsAgreementConcluded));
            if (noAgreementConcludedRemark) sectionProperties.Remove(nameof(DataProcessingRegistrationGeneralDataWriteRequestDTO.IsAgreementConcludedRemark));
            if (noAgreementConcludedAt) sectionProperties.Remove(nameof(DataProcessingRegistrationGeneralDataWriteRequestDTO.AgreementConcludedAt));
            if (noBasisForTransfer) sectionProperties.Remove(nameof(DataProcessingRegistrationGeneralDataWriteRequestDTO.BasisForTransferUuid));
            if (noTransferToInsecureCountries) sectionProperties.Remove(nameof(DataProcessingRegistrationGeneralDataWriteRequestDTO.TransferToInsecureThirdCountries));
            if (noInsecureCountries) sectionProperties.Remove(nameof(DataProcessingRegistrationGeneralDataWriteRequestDTO.InsecureCountriesSubjectToDataTransferUuids));
            if (noDataProcessors) sectionProperties.Remove(nameof(DataProcessingRegistrationGeneralDataWriteRequestDTO.DataProcessorUuids));
            if (noHasSubDataProcessors) sectionProperties.Remove(nameof(DataProcessingRegistrationGeneralDataWriteRequestDTO.HasSubDataProcessors));
            if (noSubDataProcessors) sectionProperties.Remove(nameof(DataProcessingRegistrationGeneralDataWriteRequestDTO.SubDataProcessors));
            if (noMainContract) sectionProperties.Remove(nameof(DataProcessingRegistrationGeneralDataWriteRequestDTO.MainContractUuid));
            if (noResponsibleUnit)
                sectionProperties.Remove(nameof(DataProcessingRegistrationGeneralDataWriteRequestDTO
                    .ResponsibleOrganizationUnitUuid));

            _currentHttpRequestMock
                .Setup(x => x.GetDefinedJsonProperties(nameof(UpdateDataProcessingRegistrationRequestDTO.General).WrapAsEnumerable().AsParameterMatch()))
                .Returns(sectionProperties);
        }

        private void ConfigureRootProperties(bool noName, bool noGeneralData, bool noSystems, bool noOversight, bool noRoles, bool noReferences)
        {
            var properties = GetAllInputPropertyNames<UpdateDataProcessingRegistrationRequestDTO>();
            if (noName) properties.Remove(nameof(CreateDataProcessingRegistrationRequestDTO.Name));
            if (noGeneralData) properties.Remove(nameof(DataProcessingRegistrationWriteRequestDTO.General));
            if (noSystems) properties.Remove(nameof(DataProcessingRegistrationWriteRequestDTO.SystemUsageUuids));
            if (noOversight) properties.Remove(nameof(DataProcessingRegistrationWriteRequestDTO.Oversight));
            if (noRoles) properties.Remove(nameof(DataProcessingRegistrationWriteRequestDTO.Roles));
            if (noReferences) properties.Remove(nameof(UpdateDataProcessingRegistrationRequestDTO.ExternalReferences));

            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(Enumerable.Empty<string>().AsParameterMatch())).Returns(properties);
        }

        private void ConfigureOversightRequestContext(
            bool noOversightOptionUuids,
            bool noOversightOptionRemark,
            bool noOversightInterval,
            bool noOversightIntervalRemark,
            bool noIsOversightCompleted,
            bool noOversightCompletedRemark,
            bool noOversightScheduledInspectionDate,
            bool noOversightDates)
        {
            var sectionProperties = GetAllInputPropertyNames<DataProcessingRegistrationOversightWriteRequestDTO>();

            if (noOversightOptionUuids) sectionProperties.Remove(nameof(DataProcessingRegistrationOversightWriteRequestDTO.OversightOptionUuids));
            if (noOversightOptionRemark) sectionProperties.Remove(nameof(DataProcessingRegistrationOversightWriteRequestDTO.OversightOptionsRemark));
            if (noOversightInterval) sectionProperties.Remove(nameof(DataProcessingRegistrationOversightWriteRequestDTO.OversightInterval));
            if (noOversightIntervalRemark) sectionProperties.Remove(nameof(DataProcessingRegistrationOversightWriteRequestDTO.OversightIntervalRemark));
            if (noIsOversightCompleted) sectionProperties.Remove(nameof(DataProcessingRegistrationOversightWriteRequestDTO.IsOversightCompleted));
            if (noOversightCompletedRemark) sectionProperties.Remove(nameof(DataProcessingRegistrationOversightWriteRequestDTO.OversightCompletedRemark));
            if (noOversightScheduledInspectionDate) sectionProperties.Remove(nameof(DataProcessingRegistrationOversightWriteRequestDTO.OversightScheduledInspectionDate));
            if (noOversightDates) sectionProperties.Remove(nameof(DataProcessingRegistrationOversightWriteRequestDTO.OversightDates));

            _currentHttpRequestMock
                .Setup(x => x.GetDefinedJsonProperties(nameof(UpdateDataProcessingRegistrationRequestDTO.Oversight).WrapAsEnumerable().AsParameterMatch()))
                .Returns(sectionProperties);
        }
    }
}
