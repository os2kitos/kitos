using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.SystemUsage.Write;
using Core.DomainModel.Shared;
using Moq;
using Newtonsoft.Json.Linq;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Request.Generic.ExternalReferences;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;
using Presentation.Web.Models.API.V2.Request.SystemUsage;
using Presentation.Web.Models.API.V2.Types.Shared;
using Presentation.Web.Models.API.V2.Types.SystemUsage;
using Tests.Toolkit.Extensions;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    public class ItSystemUsageWriteModelMapperTest : WriteModelMapperTestBase
    {
        private readonly ItSystemUsageWriteModelMapper _sut;
        private readonly Mock<ICurrentHttpRequest> _currentHttpRequestMock;

        public ItSystemUsageWriteModelMapperTest()
        {
            _currentHttpRequestMock = new Mock<ICurrentHttpRequest>();
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(Enumerable.Empty<string>().AsParameterMatch())).Returns(GetAllInputPropertyNames<CreateItSystemUsageRequestDTO>());
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(Enumerable.Empty<string>().AsParameterMatch())).Returns(GetAllInputPropertyNames<UpdateItSystemUsageRequestDTO>());
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(nameof(UpdateItSystemUsageRequestDTO.General).WrapAsEnumerable().AsParameterMatch())).Returns(GetAllInputPropertyNames<GeneralDataUpdateRequestDTO>());
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(new[] { nameof(UpdateItSystemUsageRequestDTO.General), nameof(UpdateItSystemUsageRequestDTO.General.Validity) }.AsParameterMatch())).Returns(GetAllInputPropertyNames<ItSystemUsageValidityWriteRequestDTO>());
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(nameof(UpdateItSystemUsageRequestDTO.Archiving).WrapAsEnumerable().AsParameterMatch())).Returns(GetAllInputPropertyNames<UpdatedSystemUsageArchivingParameters>());
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(nameof(CreateItSystemUsageRequestDTO.Archiving).WrapAsEnumerable().AsParameterMatch())).Returns(GetAllInputPropertyNames<ArchivingCreationRequestDTO>());
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(nameof(UpdateItSystemUsageRequestDTO.GDPR).WrapAsEnumerable().AsParameterMatch())).Returns(GetAllInputPropertyNames<GDPRWriteRequestDTO>());
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(nameof(UpdateItSystemUsageRequestDTO.LocalKleDeviations).WrapAsEnumerable().AsParameterMatch())).Returns(GetAllInputPropertyNames<LocalKLEDeviationsRequestDTO>());
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(nameof(UpdateItSystemUsageRequestDTO.OrganizationUsage).WrapAsEnumerable().AsParameterMatch())).Returns(GetAllInputPropertyNames<OrganizationUsageWriteRequestDTO>());
            _currentHttpRequestMock.Setup(x => x.GetObject(It.IsAny<IEnumerable<string>>())).Returns(Maybe<JToken>.None);
            _sut = new ItSystemUsageWriteModelMapper(_currentHttpRequestMock.Object);
        }

        [Fact]
        public void Can_Map_Roles()
        {
            //Arrange
            var roles = Many<RoleAssignmentRequestDTO>().OrderBy(x => x.RoleUuid).ToList();

            //Act
            var output = _sut.FromPATCH(new UpdateItSystemUsageRequestDTO() { Roles = roles });

            //Assert
            var roleSection = AssertPropertyContainsDataChange(output.Roles);
            var userRolePairs = AssertPropertyContainsDataChange(roleSection.UserRolePairs).OrderBy(x => x.RoleUuid).ToList();
            Assert.Equal(roles.Count, userRolePairs.Count);
            for (var i = 0; i < userRolePairs.Count; i++)
            {
                var expected = roles[i];
                var actual = userRolePairs[i];
                Assert.Equal(expected.RoleUuid, actual.RoleUuid);
                Assert.Equal(expected.UserUuid, actual.UserUuid);
            }
        }

        [Fact]
        public void Can_Map_Single_ExternalReference()
        {
            //Arrange
            var expected = A<ExternalReferenceDataWriteRequestDTO>();

            //Act
            var actual = _sut.MapExternalReference(expected);

            //Assert
            Assert.Equal(expected.Url, actual.Url);
            Assert.Equal(expected.Title, actual.Title);
            Assert.Equal(expected.DocumentId, actual.DocumentId);
            Assert.Equal(expected.MasterReference, actual.MasterReference);
        }

        [Fact]
        public void Can_Map_ExternalReferences()
        {
            //Arrange
            var references = Many<UpdateExternalReferenceDataWriteRequestDTO>().OrderBy(x => x.Url).ToList();

            //Act
            var mappedReferences = _sut.FromPATCH(new UpdateItSystemUsageRequestDTO() { ExternalReferences = references }).ExternalReferences.Value.OrderBy(x => x.Url).ToList();

            //Assert
            Assert.Equal(mappedReferences.Count, mappedReferences.Count);
            for (var i = 0; i < mappedReferences.Count; i++)
            {
                var expected = references[i];
                var actual = mappedReferences[i];
                Assert.Equal(expected.Uuid, actual.Uuid);
                Assert.Equal(expected.Url, actual.Url);
                Assert.Equal(expected.Title, actual.Title);
                Assert.Equal(expected.DocumentId, actual.DocumentId);
                Assert.Equal(expected.MasterReference, actual.MasterReference);
            }
        }

        [Fact]
        public void Can_Map_KLE()
        {
            //Arrange
            var input = A<LocalKLEDeviationsRequestDTO>();

            //Act
            var output = _sut.FromPATCH(new UpdateItSystemUsageRequestDTO() { LocalKleDeviations = input });

            //Assert
            var mappedKle = AssertPropertyContainsDataChange(output.KLE);
            AssertKLE(input.AddedKLEUuids, mappedKle.AddedKLEUuids);
            AssertKLE(input.RemovedKLEUuids, mappedKle.RemovedKLEUuids);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void Map_KLE_Resets_Property_If_Source_Is_Not_Defined(bool addedNull, bool removedNull)
        {
            //Arrange
            var input = A<LocalKLEDeviationsRequestDTO>();
            if (addedNull) input.AddedKLEUuids = null;
            if (removedNull) input.RemovedKLEUuids = null;

            //Act
            var output = _sut.FromPATCH(new UpdateItSystemUsageRequestDTO() { LocalKleDeviations = input });

            //Assert that null is translated into a reset value (change to "none")
            var mappedKle = AssertPropertyContainsDataChange(output.KLE);
            Assert.Equal(addedNull, mappedKle.AddedKLEUuids.NewValue.IsNone);
            Assert.Equal(removedNull, mappedKle.RemovedKLEUuids.NewValue.IsNone);
        }

        [Fact]
        public void Can_Map_OrganizationUsage()
        {
            //Arrange
            var input = A<OrganizationUsageWriteRequestDTO>();

            //Act
            var output = _sut.FromPATCH(new UpdateItSystemUsageRequestDTO() { OrganizationUsage = input });

            //Assert 
            var mappedOrgUsage = AssertPropertyContainsDataChange(output.OrganizationalUsage);
            var responsible = AssertPropertyContainsDataChange(mappedOrgUsage.ResponsibleOrganizationUnitUuid);
            var usingOrgUnits = AssertPropertyContainsDataChange(mappedOrgUsage.UsingOrganizationUnitUuids);
            Assert.Equal(input.ResponsibleOrganizationUnitUuid, responsible);
            Assert.Equal(input.UsingOrganizationUnitUuids, usingOrgUnits);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void Map_OrganizationUsage_Resets_Property_If_Source_Value_Is_Not_Defined(bool responsibleIsNull, bool unitsAreNull)
        {
            //Arrange
            var input = A<OrganizationUsageWriteRequestDTO>();
            if (responsibleIsNull) input.ResponsibleOrganizationUnitUuid = null;
            if (unitsAreNull) input.UsingOrganizationUnitUuids = null;

            //Act
            var output = _sut.FromPATCH(new UpdateItSystemUsageRequestDTO() { OrganizationUsage = input });

            //Assert 
            var mappedOrgUsage = AssertPropertyContainsDataChange(output.OrganizationalUsage);
            if (responsibleIsNull)
                AssertPropertyContainsResetDataChange(mappedOrgUsage.ResponsibleOrganizationUnitUuid);
            else
                AssertPropertyContainsDataChange(mappedOrgUsage.ResponsibleOrganizationUnitUuid);
            if (unitsAreNull)
                AssertPropertyContainsResetDataChange(mappedOrgUsage.UsingOrganizationUnitUuids);
            else
                AssertPropertyContainsDataChange(mappedOrgUsage.UsingOrganizationUnitUuids);
        }

        [Fact]
        public void Can_Map_General_Data_Properties()
        {
            //Arrange
            var input = A<GeneralDataWriteRequestDTO>();

            //Act
            var output = _sut.FromPOST(new CreateItSystemUsageRequestDTO() { General = input });

            //Assert
            var mappedGeneralSection = AssertPropertyContainsDataChange(output.GeneralProperties);
            AssertCommonGeneralDataWriteProperties(input, mappedGeneralSection);
            Assert.True(mappedGeneralSection.MainContractUuid.IsUnchanged, "The main contract should be untouched as it is not part of the initial write contract");
        }

        [Fact]
        public void Map_General_Data_Properties_Resets_Validity_If_SourceValue_Not_Defined()
        {
            //Arrange
            var input = A<GeneralDataUpdateRequestDTO>();
            input.Validity = null;

            //Act
            var output = _sut.FromPATCH(new UpdateItSystemUsageRequestDTO { General = input });

            //Assert
            var mappedGeneralSection = AssertPropertyContainsDataChange(output.GeneralProperties);

            Assert.True(mappedGeneralSection.LifeCycleStatus.HasChange);
            Assert.Null(mappedGeneralSection.LifeCycleStatus.NewValue);
            AssertPropertyContainsResetDataChange(mappedGeneralSection.ValidFrom);
            AssertPropertyContainsResetDataChange(mappedGeneralSection.ValidTo);
        }

        [Fact]
        public void Map_General_Data_Properties_Resets_NumberOfExpectedUsers_If_SourceValue_Not_Defined()
        {
            //Arrange
            var input = A<GeneralDataUpdateRequestDTO>();
            input.NumberOfExpectedUsers = null;

            //Act
            var output = _sut.FromPATCH(new UpdateItSystemUsageRequestDTO { General = input });

            //Assert
            var mappedGeneralSection = AssertPropertyContainsDataChange(output.GeneralProperties);
            AssertPropertyContainsResetDataChange(mappedGeneralSection.NumberOfExpectedUsersInterval);
        }

        [Fact]
        public void Can_Map_General_Data_Update_Properties()
        {
            //Arrange
            var input = A<GeneralDataUpdateRequestDTO>();

            //Act
            var output = _sut.FromPUT(new UpdateItSystemUsageRequestDTO { General = input });

            //Assert
            var mappedGeneralSection = AssertPropertyContainsDataChange(output.GeneralProperties);
            AssertCommonGeneralDataWriteProperties(input, mappedGeneralSection);
            Assert.Equal(input.MainContractUuid, AssertPropertyContainsDataChange(mappedGeneralSection.MainContractUuid));
        }

        [Fact]
        public void Map_General_Data_Update_Properties_Resets_Main_Contract_Id_If_Undefined()
        {
            //Arrange
            var input = A<GeneralDataUpdateRequestDTO>();
            input.MainContractUuid = null;

            //Act
            var output = _sut.FromPATCH(new UpdateItSystemUsageRequestDTO() { General = input });

            //Assert
            var mappedGeneralSection = AssertPropertyContainsDataChange(output.GeneralProperties);
            AssertCommonGeneralDataWriteProperties(input, mappedGeneralSection);
            AssertPropertyContainsResetDataChange(mappedGeneralSection.MainContractUuid);
        }

        [Fact]
        public void Can_Map_GDPR_Data_Properties()
        {
            //Arrange
            var input = A<GDPRWriteRequestDTO>();

            //Act
            var output = _sut.FromPATCH(new UpdateItSystemUsageRequestDTO() { GDPR = input });

            //Assert
            var mappedGdpr = AssertPropertyContainsDataChange(output.GDPR);
            Assert.Equal(input.Purpose, AssertPropertyContainsDataChange(mappedGdpr.Purpose));
            Assert.Equal(input.BusinessCritical, AssertPropertyContainsDataChange(mappedGdpr.BusinessCritical)?.ToYesNoDontKnowChoice());
            Assert.Equal(input.HostedAt, AssertPropertyContainsDataChange(mappedGdpr.HostedAt)?.ToHostingChoice());
            AssertLinkMapping(input.DirectoryDocumentation, mappedGdpr.DirectoryDocumentation);
            Assert.Equal(input.DataSensitivityLevels.ToList(), AssertPropertyContainsDataChange(mappedGdpr.DataSensitivityLevels).Select(x => x.ToDataSensitivityLevelChoice()));
            Assert.Equal(input.SensitivePersonDataUuids.ToList(), AssertPropertyContainsDataChange(mappedGdpr.SensitivePersonDataUuids));
            Assert.Equal(input.RegisteredDataCategoryUuids.ToList(), AssertPropertyContainsDataChange(mappedGdpr.RegisteredDataCategoryUuids));
            Assert.Equal(input.TechnicalPrecautionsInPlace, AssertPropertyContainsDataChange(mappedGdpr.TechnicalPrecautionsInPlace)?.ToYesNoDontKnowChoice());
            Assert.Equal(input.TechnicalPrecautionsApplied.ToList(), AssertPropertyContainsDataChange(mappedGdpr.TechnicalPrecautionsApplied).Select(x => x.ToTechnicalPrecautionChoice()));
            AssertLinkMapping(input.TechnicalPrecautionsDocumentation, mappedGdpr.TechnicalPrecautionsDocumentation);
            Assert.Equal(input.UserSupervision, AssertPropertyContainsDataChange(mappedGdpr.UserSupervision)?.ToYesNoDontKnowChoice());
            Assert.Equal(input.UserSupervisionDate, AssertPropertyContainsDataChange(mappedGdpr.UserSupervisionDate));
            AssertLinkMapping(input.UserSupervisionDocumentation, mappedGdpr.UserSupervisionDocumentation);
            Assert.Equal(input.RiskAssessmentConducted, AssertPropertyContainsDataChange(mappedGdpr.RiskAssessmentConducted)?.ToYesNoDontKnowChoice());
            Assert.Equal(input.RiskAssessmentNotes, AssertPropertyContainsDataChange(mappedGdpr.RiskAssessmentNotes));
            Assert.Equal(input.RiskAssessmentConductedDate, AssertPropertyContainsDataChange(mappedGdpr.RiskAssessmentConductedDate));
            AssertLinkMapping(input.RiskAssessmentDocumentation, mappedGdpr.RiskAssessmentDocumentation);
            Assert.Equal(input.RiskAssessmentResult, AssertPropertyContainsDataChange(mappedGdpr.RiskAssessmentResult)?.ToRiskLevelChoice());
            Assert.Equal(input.PlannedRiskAssessmentDate, AssertPropertyContainsDataChange(mappedGdpr.PlannedRiskAssessmentDate));
            Assert.Equal(input.DPIAConducted, AssertPropertyContainsDataChange(mappedGdpr.DPIAConducted)?.ToYesNoDontKnowChoice());
            Assert.Equal(input.DPIADate, AssertPropertyContainsDataChange(mappedGdpr.DPIADate));
            Assert.Equal(input.NextDataRetentionEvaluationDate, AssertPropertyContainsDataChange(mappedGdpr.NextDataRetentionEvaluationDate));
            Assert.Equal(input.DataRetentionEvaluationFrequencyInMonths, AssertPropertyContainsDataChange(mappedGdpr.DataRetentionEvaluationFrequencyInMonths));
            AssertLinkMapping(input.DPIADocumentation, mappedGdpr.DPIADocumentation);
            Assert.Equal(input.SpecificPersonalData.ToList(), AssertPropertyContainsDataChange(mappedGdpr.PersonalDataOptions).Select(x => x.ToGDPRPersonalDataChoice()));
        }

        [Fact]
        public void Map_GDPR_Data_Properties_Maps_Reset_If_Directory_Documentation_Is_Null()
        {
            //Arrange
            var input = A<GDPRWriteRequestDTO>();
            input.DirectoryDocumentation = null;

            //Act
            var output = _sut.FromPATCH(new UpdateItSystemUsageRequestDTO() { GDPR = input });

            //Assert
            var mappedGdpr = AssertPropertyContainsDataChange(output.GDPR);
            AssertPropertyContainsResetDataChange(mappedGdpr.DirectoryDocumentation);
        }

        [Fact]
        public void Map_GDPR_Data_Properties_Maps_Reset_If_DataSensitivityLevels_Is_Null()
        {
            //Arrange
            var input = A<GDPRWriteRequestDTO>();
            input.DataSensitivityLevels = null;

            //Act
            var output = _sut.FromPATCH(new UpdateItSystemUsageRequestDTO() { GDPR = input });

            //Assert
            var mappedGdpr = AssertPropertyContainsDataChange(output.GDPR);
            AssertPropertyContainsResetDataChange(mappedGdpr.DataSensitivityLevels);
        }

        [Fact]
        public void Map_GDPR_Data_Properties_Maps_Reset_If_SensitivePersonDataUuids_Is_Null()
        {
            //Arrange
            var input = A<GDPRWriteRequestDTO>();
            input.SensitivePersonDataUuids = null;

            //Act
            var output = _sut.FromPATCH(new UpdateItSystemUsageRequestDTO() { GDPR = input });

            //Assert
            var mappedGdpr = AssertPropertyContainsDataChange(output.GDPR);
            AssertPropertyContainsResetDataChange(mappedGdpr.SensitivePersonDataUuids);
        }

        [Fact]
        public void Map_GDPR_Data_Properties_Maps_Reset_If_RegisteredDataCategoryUuids_Is_Null()
        {
            //Arrange
            var input = A<GDPRWriteRequestDTO>();
            input.RegisteredDataCategoryUuids = null;

            //Act
            var output = _sut.FromPATCH(new UpdateItSystemUsageRequestDTO() { GDPR = input });

            //Assert
            var mappedGdpr = AssertPropertyContainsDataChange(output.GDPR);
            AssertPropertyContainsResetDataChange(mappedGdpr.RegisteredDataCategoryUuids);
        }

        [Fact]
        public void Map_GDPR_Data_Properties_Maps_Reset_If_TechnicalPrecautionsApplied_Is_Null()
        {
            //Arrange
            var input = A<GDPRWriteRequestDTO>();
            input.TechnicalPrecautionsApplied = null;

            //Act
            var output = _sut.FromPATCH(new UpdateItSystemUsageRequestDTO() { GDPR = input });

            //Assert
            var mappedGdpr = AssertPropertyContainsDataChange(output.GDPR);
            AssertPropertyContainsResetDataChange(mappedGdpr.TechnicalPrecautionsApplied);
        }

        [Fact]
        public void Map_GDPR_Data_Properties_Maps_Reset_If_TechnicalPrecautionsDocumentation_Is_Null()
        {
            //Arrange
            var input = A<GDPRWriteRequestDTO>();
            input.TechnicalPrecautionsDocumentation = null;

            //Act
            var output = _sut.FromPATCH(new UpdateItSystemUsageRequestDTO() { GDPR = input });

            //Assert
            var mappedGdpr = AssertPropertyContainsDataChange(output.GDPR);
            AssertPropertyContainsResetDataChange(mappedGdpr.TechnicalPrecautionsDocumentation);
        }

        [Fact]
        public void Map_GDPR_Data_Properties_Maps_Reset_If_UserSupervisionDocumentation_Is_Null()
        {
            //Arrange
            var input = A<GDPRWriteRequestDTO>();
            input.UserSupervisionDocumentation = null;

            //Act
            var output = _sut.FromPATCH(new UpdateItSystemUsageRequestDTO() { GDPR = input });

            //Assert
            var mappedGdpr = AssertPropertyContainsDataChange(output.GDPR);
            AssertPropertyContainsResetDataChange(mappedGdpr.UserSupervisionDocumentation);
        }

        [Fact]
        public void Map_GDPR_Data_Properties_Maps_Reset_If_RiskAssessmentDocumentation_Is_Null()
        {
            //Arrange
            var input = A<GDPRWriteRequestDTO>();
            input.RiskAssessmentDocumentation = null;

            //Act
            var output = _sut.FromPATCH(new UpdateItSystemUsageRequestDTO() { GDPR = input });

            //Assert
            var mappedGdpr = AssertPropertyContainsDataChange(output.GDPR);
            AssertPropertyContainsResetDataChange(mappedGdpr.RiskAssessmentDocumentation);
        }

        [Fact]
        public void Map_GDPR_Data_Properties_Maps_Reset_If_DPIADocumentation_Is_Null()
        {
            //Arrange
            var input = A<GDPRWriteRequestDTO>();
            input.DPIADocumentation = null;

            //Act
            var output = _sut.FromPATCH(new UpdateItSystemUsageRequestDTO() { GDPR = input });

            //Assert
            var mappedGdpr = AssertPropertyContainsDataChange(output.GDPR);
            AssertPropertyContainsResetDataChange(mappedGdpr.DPIADocumentation);
        }

        [Fact]
        public void Map_GDPR_Data_Properties_Maps_Reset_If_PersonalDataOptions_Is_Null()
        {
            //Arrange
            var input = A<GDPRWriteRequestDTO>();
            input.SpecificPersonalData = null;

            //Act
            var output = _sut.FromPATCH(new UpdateItSystemUsageRequestDTO() { GDPR = input });

            //Assert
            var mappedGdpr = AssertPropertyContainsDataChange(output.GDPR);
            AssertPropertyContainsResetDataChange(mappedGdpr.PersonalDataOptions);
        }

        [Fact]
        public void Can_Map_Archiving()
        {
            //Arrange
            var input = A<ArchivingUpdateRequestDTO>();
            input.JournalPeriods = Many<JournalPeriodUpdateRequestDTO>();
            var allJournalPeriods = input.JournalPeriods.ToList();
            allJournalPeriods.RandomItem().Uuid = null; //Make sure one of them has no uuid

            //Act
            var output = _sut.FromPATCH(new UpdateItSystemUsageRequestDTO() { Archiving = input });

            //Assert
            var mappedArchiving = AssertPropertyContainsDataChange(output.Archiving);
            Assert.Equal(input.ArchiveDuty, AssertPropertyContainsDataChange(mappedArchiving.ArchiveDuty)?.ToArchiveDutyChoice());
            Assert.Equal(input.TypeUuid, AssertPropertyContainsDataChange(mappedArchiving.ArchiveTypeUuid));
            Assert.Equal(input.LocationUuid, AssertPropertyContainsDataChange(mappedArchiving.ArchiveLocationUuid));
            Assert.Equal(input.TestLocationUuid, AssertPropertyContainsDataChange(mappedArchiving.ArchiveTestLocationUuid));
            Assert.Equal(input.SupplierOrganizationUuid, AssertPropertyContainsDataChange(mappedArchiving.ArchiveSupplierOrganizationUuid));
            Assert.Equal(input.Active, AssertPropertyContainsDataChange(mappedArchiving.ArchiveActive));
            Assert.Equal(input.Notes, AssertPropertyContainsDataChange(mappedArchiving.ArchiveNotes));
            Assert.Equal(input.FrequencyInMonths, AssertPropertyContainsDataChange(mappedArchiving.ArchiveFrequencyInMonths));
            Assert.Equal(input.DocumentBearing, AssertPropertyContainsDataChange(mappedArchiving.ArchiveDocumentBearing));

            var inputPeriods = allJournalPeriods.OrderBy(_ => _.ArchiveId).ToList();
            var mappedPeriods = AssertPropertyContainsDataChange(mappedArchiving.ArchiveJournalPeriods).OrderBy(_ => _.ArchiveId).ToList();
            Assert.Equal(inputPeriods.Count, mappedPeriods.Count);
            for (var i = 0; i < inputPeriods.Count; i++)
            {
                var inputData = inputPeriods[i];
                var outputData = mappedPeriods[i];
                Assert.Equal(inputData.Approved, outputData.Approved);
                Assert.Equal(inputData.ArchiveId, outputData.ArchiveId);
                Assert.Equal(inputData.StartDate, outputData.StartDate);
                Assert.Equal(inputData.EndDate, outputData.EndDate);
                Assert.Equal(inputData.Uuid, outputData.Uuid);
            }
        }

        [Fact]
        public void Map_Archiving_Resets_JournalPeriods_If_Null()
        {
            //Arrange
            var input = A<ArchivingUpdateRequestDTO>();
            input.JournalPeriods = null;

            //Act
            var output = _sut.FromPATCH(new UpdateItSystemUsageRequestDTO() { Archiving = input });

            //Assert
            var mappedArchiving = AssertPropertyContainsDataChange(output.Archiving);
            AssertPropertyContainsResetDataChange(mappedArchiving.ArchiveJournalPeriods);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedSectionsInput))]
        public void FromPOST_Maps_All_Defined_Sections_And_Undefined_Are_Mapped_As_Unchanged(
            bool generalNull,
            bool rolesNull,
            bool kleNull,
            bool orgUsageNull,
            bool referencesNull,
            bool archivingNull,
            bool gdprNull)
        {
            //Arrange
            var input = new CreateItSystemUsageRequestDTO()
            {
                SystemUuid = A<Guid>(),
                OrganizationUuid = A<Guid>(),
                General = generalNull ? null : A<GeneralDataWriteRequestDTO>(),
                Roles = rolesNull ? null : Many<RoleAssignmentRequestDTO>(),
                LocalKleDeviations = kleNull ? null : A<LocalKLEDeviationsRequestDTO>(),
                OrganizationUsage = orgUsageNull ? null : A<OrganizationUsageWriteRequestDTO>(),
                ExternalReferences = referencesNull ? null : Many<ExternalReferenceDataWriteRequestDTO>(),
                Archiving = archivingNull ? null : A<ArchivingCreationRequestDTO>(),
                GDPR = gdprNull ? null : A<GDPRWriteRequestDTO>(),
            };

            //Act
            var output = _sut.FromPOST(input);

            //Assert that null on creation just turns off changes in affected, optional sections
            Assert.Equal(generalNull, output.GeneralProperties.IsNone);
            Assert.Equal(rolesNull, output.Roles.IsNone);
            Assert.Equal(kleNull, output.KLE.IsNone);
            Assert.Equal(orgUsageNull, output.OrganizationalUsage.IsNone);
            Assert.Equal(referencesNull, output.ExternalReferences.IsNone);
            Assert.Equal(archivingNull, output.Archiving.IsNone);
            Assert.Equal(gdprNull, output.GDPR.IsNone);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedSectionsInput))]
        public void FromPUT_Maps_All_Sections_Including_Null_Sections(
           bool generalNull,
           bool rolesNull,
           bool kleNull,
           bool orgUsageNull,
           bool referencesNull,
           bool archivingNull,
           bool gdprNull)
        {
            //Arrange
            var input = new UpdateItSystemUsageRequestDTO()
            {
                General = generalNull ? null : A<GeneralDataUpdateRequestDTO>(),
                Roles = rolesNull ? null : Many<RoleAssignmentRequestDTO>(),
                LocalKleDeviations = kleNull ? null : A<LocalKLEDeviationsRequestDTO>(),
                OrganizationUsage = orgUsageNull ? null : A<OrganizationUsageWriteRequestDTO>(),
                ExternalReferences = referencesNull ? null : Many<UpdateExternalReferenceDataWriteRequestDTO>(),
                Archiving = archivingNull ? null : A<ArchivingUpdateRequestDTO>(),
                GDPR = gdprNull ? null : A<GDPRWriteRequestDTO>(),
            };

            //Act
            var output = _sut.FromPUT(input);

            //Assert that all sections are mapped as changed
            Assert.False(output.GeneralProperties.IsNone);
            Assert.False(output.Roles.IsNone);
            Assert.False(output.KLE.IsNone);
            Assert.False(output.OrganizationalUsage.IsNone);
            Assert.False(output.ExternalReferences.IsNone);
            Assert.False(output.Archiving.IsNone);
            Assert.False(output.GDPR.IsNone);
        }

        [Fact]
        public void Can_Map_SystemRelationParameters()
        {
            //Arrange
            var input = A<SystemRelationWriteRequestDTO>();

            //Act
            var output = _sut.MapRelation(input);

            //Assert
            Assert.Equal(input.ToSystemUsageUuid, output.ToSystemUsageUuid);
            Assert.Equal(input.AssociatedContractUuid, output.AssociatedContractUuid);
            Assert.Equal(input.RelationFrequencyUuid, output.RelationFrequencyUuid);
            Assert.Equal(input.RelationInterfaceUuid, output.UsingInterfaceUuid);
            Assert.Equal(input.Description, output.Description);
            Assert.Equal(input.UrlReference, output.UrlReference);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedSectionsInput))]
        public void FromPUT_Includedes_Root_Level_Sections_Not_Present_In_The_Request(
            bool noGeneralSection,
            bool noRoles,
            bool noKle,
            bool noOrgUsage,
            bool noReferences,
            bool noArchiving,
            bool noGdpr)
        {
            //Arrange
            var emptyInput = new UpdateItSystemUsageRequestDTO();
            ConfigureRootProperties(noGeneralSection, noRoles, noKle, noOrgUsage, noReferences, noArchiving, noGdpr);

            //Act
            var output = _sut.FromPUT(emptyInput);

            //Assert that all sections are mapped as changed - including undefined sections
            Assert.False(output.GeneralProperties.IsNone);
            Assert.False(output.Roles.IsNone);
            Assert.False(output.KLE.IsNone);
            Assert.False(output.OrganizationalUsage.IsNone);
            Assert.False(output.ExternalReferences.IsNone);
            Assert.False(output.Archiving.IsNone);
            Assert.False(output.GDPR.IsNone);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedSectionsInput))]
        public void FromPATCH_Ignores_Root_Level_Sections_Not_Present_In_The_Request(
           bool noGeneralSection,
           bool noRoles,
           bool noKle,
           bool noOrgUsage,
           bool noReferences,
           bool noArchiving,
           bool noGdpr)
        {
            //Arrange
            var emptyInput = new UpdateItSystemUsageRequestDTO();
            ConfigureRootProperties(noGeneralSection, noRoles, noKle, noOrgUsage, noReferences, noArchiving, noGdpr);

            //Act
            var output = _sut.FromPATCH(emptyInput);

            //Assert that all sections are mapped correctly
            Assert.Equal(output.GeneralProperties.IsNone, noGeneralSection);
            Assert.Equal(output.Roles.IsNone, noRoles);
            Assert.Equal(output.KLE.IsNone, noKle);
            Assert.Equal(output.OrganizationalUsage.IsNone, noOrgUsage);
            Assert.Equal(output.ExternalReferences.IsNone, noReferences);
            Assert.Equal(output.Archiving.IsNone, noArchiving);
            Assert.Equal(output.GDPR.IsNone, noGdpr);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedGeneralSectionsInput))]
        public void FromPATCH_Ignores_Undefined_Properties_In_GeneralSection(
            bool noLocalCallName,
            bool noLocalSystemId,
            bool noDataClassificationUuid,
            bool noNotes,
            bool noSystemVersion,
            bool noNumberOfExpectedUsers,
            bool noLifeCycleStatus,
            bool noValidFrom,
            bool noValidTo,
            bool noMainContractUuid)
        {
            //Arrange
            var emptyInput = new UpdateItSystemUsageRequestDTO();
            ConfigureGeneralDataProperties(
                noLocalCallName,
                noLocalSystemId,
                noDataClassificationUuid,
                noNotes,
                noSystemVersion,
                noNumberOfExpectedUsers,
                noLifeCycleStatus,
                noValidFrom,
                noValidTo,
                noMainContractUuid);

            //Act
            var output = _sut.FromPATCH(emptyInput);

            //Assert that all general properties are mapped correctly
            var generalSection = output.GeneralProperties.Value;
            Assert.Equal(noLocalCallName, generalSection.LocalCallName.IsUnchanged);
            Assert.Equal(noLocalSystemId, generalSection.LocalSystemId.IsUnchanged);
            Assert.Equal(noDataClassificationUuid, generalSection.DataClassificationUuid.IsUnchanged);
            Assert.Equal(noNotes, generalSection.Notes.IsUnchanged);
            Assert.Equal(noSystemVersion, generalSection.SystemVersion.IsUnchanged);
            Assert.Equal(noNumberOfExpectedUsers, generalSection.NumberOfExpectedUsersInterval.IsUnchanged);
            Assert.Equal(noLifeCycleStatus, generalSection.LifeCycleStatus.IsUnchanged);
            Assert.Equal(noValidFrom, generalSection.ValidFrom.IsUnchanged);
            Assert.Equal(noValidTo, generalSection.ValidTo.IsUnchanged);
            Assert.Equal(noMainContractUuid, generalSection.MainContractUuid.IsUnchanged);
        }


        [Theory]
        [MemberData(nameof(GetUndefinedGeneralSectionsInput))]
        public void FromPUT_Enforces_Undefined_Properties_In_GeneralSection(
            bool noLocalCallName,
            bool noLocalSystemId,
            bool noDataClassificationUuid,
            bool noNotes,
            bool noSystemVersion,
            bool noNumberOfExpectedUsers,
            bool noLifeCycleStatus,
            bool noValidFrom,
            bool noValidTo,
            bool noMainContractUuid)
        {
            //Arrange
            var emptyInput = new UpdateItSystemUsageRequestDTO();
            ConfigureGeneralDataProperties(
                noLocalCallName,
                noLocalSystemId,
                noDataClassificationUuid,
                noNotes,
                noSystemVersion,
                noNumberOfExpectedUsers,
                noLifeCycleStatus,
                noValidFrom,
                noValidTo,
                noMainContractUuid);

            //Act
            var output = _sut.FromPUT(emptyInput);

            //Assert that all general properties are mapped correctly
            var generalSection = output.GeneralProperties.Value;
            Assert.True(generalSection.LocalCallName.HasChange);
            Assert.True(generalSection.LocalSystemId.HasChange);
            Assert.True(generalSection.DataClassificationUuid.HasChange);
            Assert.True(generalSection.Notes.HasChange);
            Assert.True(generalSection.SystemVersion.HasChange);
            Assert.True(generalSection.NumberOfExpectedUsersInterval.HasChange);
            Assert.True(generalSection.LifeCycleStatus.HasChange);
            Assert.True(generalSection.ValidFrom.HasChange);
            Assert.True(generalSection.ValidTo.HasChange);
            Assert.True(generalSection.MainContractUuid.HasChange);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedOrganizationUsageSectionsInput))]
        public void FromPATCH_Ignores_Undefined_Properties_In_OrganizationUsageSection(
            bool noResponsibleOrganizationUnitUuid,
            bool noUsingOrganizationUnitUuids)
        {
            //Arrange
            var emptyInput = new UpdateItSystemUsageRequestDTO();
            ConfigureOrganizationUsageDataProperties(
                noResponsibleOrganizationUnitUuid,
                noUsingOrganizationUnitUuids);

            //Act
            var output = _sut.FromPATCH(emptyInput);

            //Assert that all organization usage properties are mapped correctly
            var organizationUsageSection = output.OrganizationalUsage.Value;
            Assert.Equal(noResponsibleOrganizationUnitUuid, organizationUsageSection.ResponsibleOrganizationUnitUuid.IsUnchanged);
            Assert.Equal(noUsingOrganizationUnitUuids, organizationUsageSection.UsingOrganizationUnitUuids.IsUnchanged);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedOrganizationUsageSectionsInput))]
        public void FromPUT_Enforces_Undefined_Properties_In_OrganizationUsageSection(
            bool noResponsibleOrganizationUnitUuid,
            bool noUsingOrganizationUnitUuids)
        {
            //Arrange
            var emptyInput = new UpdateItSystemUsageRequestDTO();
            ConfigureOrganizationUsageDataProperties(
                noResponsibleOrganizationUnitUuid,
                noUsingOrganizationUnitUuids);

            //Act
            var output = _sut.FromPUT(emptyInput);

            //Assert that all organization usage properties are mapped correctly
            var organizationUsageSection = output.OrganizationalUsage.Value;
            Assert.True(organizationUsageSection.ResponsibleOrganizationUnitUuid.HasChange);
            Assert.True(organizationUsageSection.UsingOrganizationUnitUuids.HasChange);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedKLESectionsInput))]
        public void FromPATCH_Ignores_Undefined_Properties_In_KLESection(
            bool noAddedKLEUuids,
            bool noRemovedKLEUuids)
        {
            //Arrange
            var emptyInput = new UpdateItSystemUsageRequestDTO();
            ConfigureKLEDataProperties(
                noAddedKLEUuids,
                noRemovedKLEUuids);

            //Act
            var output = _sut.FromPATCH(emptyInput);

            //Assert that all KLE properties are mapped correctly
            var kleSection = output.KLE.Value;
            Assert.Equal(noAddedKLEUuids, kleSection.AddedKLEUuids.IsUnchanged);
            Assert.Equal(noRemovedKLEUuids, kleSection.RemovedKLEUuids.IsUnchanged);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedKLESectionsInput))]
        public void FromPUT_Enforces_Undefined_Properties_In_KLESection(
            bool noAddedKLEUuids,
            bool noRemovedKLEUuids)
        {
            //Arrange
            var emptyInput = new UpdateItSystemUsageRequestDTO();
            ConfigureKLEDataProperties(
                noAddedKLEUuids,
                noRemovedKLEUuids);

            //Act
            var output = _sut.FromPUT(emptyInput);

            //Assert that all KLE properties are mapped correctly
            var kleSection = output.KLE.Value;
            Assert.True(kleSection.AddedKLEUuids.HasChange);
            Assert.True(kleSection.RemovedKLEUuids.HasChange);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedArchivingSectionsInput))]
        public void FromPATCH_Ignores_Undefined_Properties_In_ArchivingSection(
            bool noArchiveDuty,
            bool noTypeUuid,
            bool noLocationUuid,
            bool noTestLocationUuid,
            bool noSupplierOrganizationUuid,
            bool noActive,
            bool noNotes,
            bool noFrequencyInMonths,
            bool noDocumentBearing,
            bool noJournalPeriods)
        {
            //Arrange
            var emptyInput = new UpdateItSystemUsageRequestDTO();
            ConfigureArchivingDataProperties(
                noArchiveDuty,
                noTypeUuid,
                noLocationUuid,
                noTestLocationUuid,
                noSupplierOrganizationUuid,
                noActive,
                noNotes,
                noFrequencyInMonths,
                noDocumentBearing,
                noJournalPeriods);

            //Act
            var output = _sut.FromPATCH(emptyInput);

            //Assert that all Archiving properties are mapped correctly
            var archivingSection = output.Archiving.Value;
            Assert.Equal(noArchiveDuty, archivingSection.ArchiveDuty.IsUnchanged);
            Assert.Equal(noTypeUuid, archivingSection.ArchiveTypeUuid.IsUnchanged);
            Assert.Equal(noLocationUuid, archivingSection.ArchiveLocationUuid.IsUnchanged);
            Assert.Equal(noTestLocationUuid, archivingSection.ArchiveTestLocationUuid.IsUnchanged);
            Assert.Equal(noSupplierOrganizationUuid, archivingSection.ArchiveSupplierOrganizationUuid.IsUnchanged);
            Assert.Equal(noActive, archivingSection.ArchiveActive.IsUnchanged);
            Assert.Equal(noNotes, archivingSection.ArchiveNotes.IsUnchanged);
            Assert.Equal(noFrequencyInMonths, archivingSection.ArchiveFrequencyInMonths.IsUnchanged);
            Assert.Equal(noDocumentBearing, archivingSection.ArchiveDocumentBearing.IsUnchanged);
            Assert.Equal(noJournalPeriods, archivingSection.ArchiveJournalPeriods.IsUnchanged);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedArchivingSectionsInput))]
        public void FromPUT_Enforces_Undefined_Properties_In_ArchivingSection(
            bool noArchiveDuty,
            bool noTypeUuid,
            bool noLocationUuid,
            bool noTestLocationUuid,
            bool noSupplierOrganizationUuid,
            bool noActive,
            bool noNotes,
            bool noFrequencyInMonths,
            bool noDocumentBearing,
            bool noJournalPeriods)
        {
            //Arrange
            var emptyInput = new UpdateItSystemUsageRequestDTO();
            ConfigureArchivingDataProperties(
                noArchiveDuty,
                noTypeUuid,
                noLocationUuid,
                noTestLocationUuid,
                noSupplierOrganizationUuid,
                noActive,
                noNotes,
                noFrequencyInMonths,
                noDocumentBearing,
                noJournalPeriods);

            //Act
            var output = _sut.FromPUT(emptyInput);

            //Assert that all Archiving properties are mapped correctly
            var archivingSection = output.Archiving.Value;
            Assert.True(archivingSection.ArchiveDuty.HasChange);
            Assert.True(archivingSection.ArchiveTypeUuid.HasChange);
            Assert.True(archivingSection.ArchiveLocationUuid.HasChange);
            Assert.True(archivingSection.ArchiveTestLocationUuid.HasChange);
            Assert.True(archivingSection.ArchiveSupplierOrganizationUuid.HasChange);
            Assert.True(archivingSection.ArchiveActive.HasChange);
            Assert.True(archivingSection.ArchiveNotes.HasChange);
            Assert.True(archivingSection.ArchiveFrequencyInMonths.HasChange);
            Assert.True(archivingSection.ArchiveDocumentBearing.HasChange);
            Assert.True(archivingSection.ArchiveJournalPeriods.HasChange);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedGDPRSectionsInput))]
        public void FromPATCH_Ignores_Undefined_Properties_In_GDPRSection(
            bool noPurpose,
            bool noBusinessCritical,
            bool noHostedAt,
            bool noDirectoryDocumentation,
            bool noDataSensitivityLevels,
            bool noSensitivePersonDataUuids,
            bool noRegisteredDataCategoryUuids,
            bool noTechnicalPrecautionsInPlace,
            bool noTechnicalPrecautionsApplied,
            bool noTechnicalPrecautionsDocumentation,
            bool noUserSupervision,
            bool noUserSupervisionDate,
            bool noUserSupervisionDocumentation,
            bool noRiskAssessmentConducted,
            bool noRiskAssessmentConductedDate,
            bool noRiskAssessmentResult,
            bool noRiskAssessmentDocumentation,
            bool noRiskAssessmentNotes,
            bool noPlannedRiskAssessmentDate,
            bool noDPIAConducted,
            bool noDPIADate,
            bool noDPIADocumentation,
            bool noRetentionPeriodDefined,
            bool noNextDataRetentionEvaluationDate,
            bool noDataRetentionEvaluationFrequencyInMonths,
            bool noPersonalData)
        {
            //Arrange
            var emptyInput = new UpdateItSystemUsageRequestDTO();
            ConfigureGDPRDataProperties(
                noPurpose,
                noBusinessCritical,
                noHostedAt,
                noDirectoryDocumentation,
                noDataSensitivityLevels,
                noSensitivePersonDataUuids,
                noRegisteredDataCategoryUuids,
                noTechnicalPrecautionsInPlace,
                noTechnicalPrecautionsApplied,
                noTechnicalPrecautionsDocumentation,
                noUserSupervision,
                noUserSupervisionDate,
                noUserSupervisionDocumentation,
                noRiskAssessmentConducted,
                noRiskAssessmentConductedDate,
                noRiskAssessmentResult,
                noRiskAssessmentDocumentation,
                noRiskAssessmentNotes,
                noPlannedRiskAssessmentDate,
                noDPIAConducted,
                noDPIADate,
                noDPIADocumentation,
                noRetentionPeriodDefined,
                noNextDataRetentionEvaluationDate,
                noDataRetentionEvaluationFrequencyInMonths,
                noPersonalData);

            //Act
            var output = _sut.FromPATCH(emptyInput);

            //Assert that all GDPR properties are mapped correctly
            var gdprSection = output.GDPR.Value;
            Assert.Equal(noPurpose, gdprSection.Purpose.IsUnchanged);
            Assert.Equal(noBusinessCritical, gdprSection.BusinessCritical.IsUnchanged);
            Assert.Equal(noHostedAt, gdprSection.HostedAt.IsUnchanged);
            Assert.Equal(noDirectoryDocumentation, gdprSection.DirectoryDocumentation.IsUnchanged);
            Assert.Equal(noDataSensitivityLevels, gdprSection.DataSensitivityLevels.IsUnchanged);
            Assert.Equal(noSensitivePersonDataUuids, gdprSection.SensitivePersonDataUuids.IsUnchanged);
            Assert.Equal(noRegisteredDataCategoryUuids, gdprSection.RegisteredDataCategoryUuids.IsUnchanged);
            Assert.Equal(noTechnicalPrecautionsInPlace, gdprSection.TechnicalPrecautionsInPlace.IsUnchanged);
            Assert.Equal(noTechnicalPrecautionsApplied, gdprSection.TechnicalPrecautionsApplied.IsUnchanged);
            Assert.Equal(noTechnicalPrecautionsDocumentation, gdprSection.TechnicalPrecautionsDocumentation.IsUnchanged);
            Assert.Equal(noUserSupervision, gdprSection.UserSupervision.IsUnchanged);
            Assert.Equal(noUserSupervisionDate, gdprSection.UserSupervisionDate.IsUnchanged);
            Assert.Equal(noUserSupervisionDocumentation, gdprSection.UserSupervisionDocumentation.IsUnchanged);
            Assert.Equal(noRiskAssessmentConducted, gdprSection.RiskAssessmentConducted.IsUnchanged);
            Assert.Equal(noRiskAssessmentConductedDate, gdprSection.RiskAssessmentConductedDate.IsUnchanged);
            Assert.Equal(noRiskAssessmentResult, gdprSection.RiskAssessmentResult.IsUnchanged);
            Assert.Equal(noRiskAssessmentDocumentation, gdprSection.RiskAssessmentDocumentation.IsUnchanged);
            Assert.Equal(noRiskAssessmentNotes, gdprSection.RiskAssessmentNotes.IsUnchanged);
            Assert.Equal(noPlannedRiskAssessmentDate, gdprSection.PlannedRiskAssessmentDate.IsUnchanged);
            Assert.Equal(noDPIAConducted, gdprSection.DPIAConducted.IsUnchanged);
            Assert.Equal(noDPIADate, gdprSection.DPIADate.IsUnchanged);
            Assert.Equal(noDPIADocumentation, gdprSection.DPIADocumentation.IsUnchanged);
            Assert.Equal(noRetentionPeriodDefined, gdprSection.RetentionPeriodDefined.IsUnchanged);
            Assert.Equal(noNextDataRetentionEvaluationDate, gdprSection.NextDataRetentionEvaluationDate.IsUnchanged);
            Assert.Equal(noDataRetentionEvaluationFrequencyInMonths, gdprSection.DataRetentionEvaluationFrequencyInMonths.IsUnchanged);
            Assert.Equal(noPersonalData, gdprSection.PersonalDataOptions.IsUnchanged);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedGDPRSectionsInput))]
        public void FromPUT_Enforces_Undefined_Properties_In_GDPRSection(
            bool noPurpose,
            bool noBusinessCritical,
            bool noHostedAt,
            bool noDirectoryDocumentation,
            bool noDataSensitivityLevels,
            bool noSensitivePersonDataUuids,
            bool noRegisteredDataCategoryUuids,
            bool noTechnicalPrecautionsInPlace,
            bool noTechnicalPrecautionsApplied,
            bool noTechnicalPrecautionsDocumentation,
            bool noUserSupervision,
            bool noUserSupervisionDate,
            bool noUserSupervisionDocumentation,
            bool noRiskAssessmentConducted,
            bool noRiskAssessmentConductedDate,
            bool noRiskAssessmentResult,
            bool noRiskAssessmentDocumentation,
            bool noRiskAssessmentNotes,
            bool noPlannedRiskAssessmentDate,
            bool noDPIAConducted,
            bool noDPIADate,
            bool noDPIADocumentation,
            bool noRetentionPeriodDefined,
            bool noNextDataRetentionEvaluationDate,
            bool noDataRetentionEvaluationFrequencyInMonths,
            bool noPersonalData)
        {
            //Arrange
            var emptyInput = new UpdateItSystemUsageRequestDTO();
            ConfigureGDPRDataProperties(
                noPurpose,
                noBusinessCritical,
                noHostedAt,
                noDirectoryDocumentation,
                noDataSensitivityLevels,
                noSensitivePersonDataUuids,
                noRegisteredDataCategoryUuids,
                noTechnicalPrecautionsInPlace,
                noTechnicalPrecautionsApplied,
                noTechnicalPrecautionsDocumentation,
                noUserSupervision,
                noUserSupervisionDate,
                noUserSupervisionDocumentation,
                noRiskAssessmentConducted,
                noRiskAssessmentConductedDate,
                noRiskAssessmentResult,
                noRiskAssessmentDocumentation,
                noRiskAssessmentNotes,
                noPlannedRiskAssessmentDate,
                noDPIAConducted,
                noDPIADate,
                noDPIADocumentation,
                noRetentionPeriodDefined,
                noNextDataRetentionEvaluationDate,
                noDataRetentionEvaluationFrequencyInMonths,
                noPersonalData);

            //Act
            var output = _sut.FromPUT(emptyInput);

            //Assert that all GDPR properties are mapped correctly
            var gdprSection = output.GDPR.Value;
            Assert.True(gdprSection.Purpose.HasChange);
            Assert.True(gdprSection.BusinessCritical.HasChange);
            Assert.True(gdprSection.HostedAt.HasChange);
            Assert.True(gdprSection.DirectoryDocumentation.HasChange);
            Assert.True(gdprSection.DataSensitivityLevels.HasChange);
            Assert.True(gdprSection.SensitivePersonDataUuids.HasChange);
            Assert.True(gdprSection.RegisteredDataCategoryUuids.HasChange);
            Assert.True(gdprSection.TechnicalPrecautionsInPlace.HasChange);
            Assert.True(gdprSection.TechnicalPrecautionsApplied.HasChange);
            Assert.True(gdprSection.TechnicalPrecautionsDocumentation.HasChange);
            Assert.True(gdprSection.UserSupervision.HasChange);
            Assert.True(gdprSection.UserSupervisionDate.HasChange);
            Assert.True(gdprSection.UserSupervisionDocumentation.HasChange);
            Assert.True(gdprSection.RiskAssessmentConducted.HasChange);
            Assert.True(gdprSection.RiskAssessmentConductedDate.HasChange);
            Assert.True(gdprSection.RiskAssessmentResult.HasChange);
            Assert.True(gdprSection.RiskAssessmentDocumentation.HasChange);
            Assert.True(gdprSection.RiskAssessmentNotes.HasChange);
            Assert.True(gdprSection.PlannedRiskAssessmentDate.HasChange);
            Assert.True(gdprSection.DPIAConducted.HasChange);
            Assert.True(gdprSection.DPIADate.HasChange);
            Assert.True(gdprSection.DPIADocumentation.HasChange);
            Assert.True(gdprSection.RetentionPeriodDefined.HasChange);
            Assert.True(gdprSection.NextDataRetentionEvaluationDate.HasChange);
            Assert.True(gdprSection.DataRetentionEvaluationFrequencyInMonths.HasChange);
            Assert.True(gdprSection.PersonalDataOptions.HasChange);
        }

        public static IEnumerable<object[]> GetUndefinedSectionsInput()
        {
            return CreateGetUndefinedSectionsInput(7);
        }

        public static IEnumerable<object[]> GetUndefinedGeneralSectionsInput()
        {
            return CreateGetUndefinedSectionsInput(10);
        }

        public static IEnumerable<object[]> GetUndefinedOrganizationUsageSectionsInput()
        {
            return CreateGetUndefinedSectionsInput(2);
        }

        public static IEnumerable<object[]> GetUndefinedKLESectionsInput()
        {
            return CreateGetUndefinedSectionsInput(2);
        }

        public static IEnumerable<object[]> GetUndefinedArchivingSectionsInput()
        {
            return CreateGetUndefinedSectionsInput(10);
        }

        public static IEnumerable<object[]> GetUndefinedGDPRSectionsInput()
        {
            return CreateGetUndefinedSectionsInput(26);
        }

        private void ConfigureGDPRDataProperties(
            bool noPurpose,
            bool noBusinessCritical,
            bool noHostedAt,
            bool noDirectoryDocumentation,
            bool noDataSensitivityLevels,
            bool noSensitivePersonDataUuids,
            bool noRegisteredDataCategoryUuids,
            bool noTechnicalPrecautionsInPlace,
            bool noTechnicalPrecautionsApplied,
            bool noTechnicalPrecautionsDocumentation,
            bool noUserSupervision,
            bool noUserSupervisionDate,
            bool noUserSupervisionDocumentation,
            bool noRiskAssessmentConducted,
            bool noRiskAssessmentConductedDate,
            bool noRiskAssessmentResult,
            bool noRiskAssessmentDocumentation,
            bool noRiskAssessmentNotes,
            bool noPlannedRiskAssessmentDate,
            bool noDPIAConducted,
            bool noDPIADate,
            bool noDPIADocumentation,
            bool noRetentionPeriodDefined,
            bool noNextDataRetentionEvaluationDate,
            bool noDataRetentionEvaluationFrequencyInMonths,
            bool noPersonalData)
        {
            var GDPRProperties = GetAllInputPropertyNames<GDPRWriteRequestDTO>();
            if (noPurpose) GDPRProperties.Remove(nameof(GDPRWriteRequestDTO.Purpose));
            if (noBusinessCritical) GDPRProperties.Remove(nameof(GDPRWriteRequestDTO.BusinessCritical));
            if (noHostedAt) GDPRProperties.Remove(nameof(GDPRWriteRequestDTO.HostedAt));
            if (noDirectoryDocumentation) GDPRProperties.Remove(nameof(GDPRWriteRequestDTO.DirectoryDocumentation));
            if (noDataSensitivityLevels) GDPRProperties.Remove(nameof(GDPRWriteRequestDTO.DataSensitivityLevels));
            if (noSensitivePersonDataUuids) GDPRProperties.Remove(nameof(GDPRWriteRequestDTO.SensitivePersonDataUuids));
            if (noRegisteredDataCategoryUuids) GDPRProperties.Remove(nameof(GDPRWriteRequestDTO.RegisteredDataCategoryUuids));
            if (noTechnicalPrecautionsInPlace) GDPRProperties.Remove(nameof(GDPRWriteRequestDTO.TechnicalPrecautionsInPlace));
            if (noTechnicalPrecautionsApplied) GDPRProperties.Remove(nameof(GDPRWriteRequestDTO.TechnicalPrecautionsApplied));
            if (noTechnicalPrecautionsDocumentation) GDPRProperties.Remove(nameof(GDPRWriteRequestDTO.TechnicalPrecautionsDocumentation));
            if (noUserSupervision) GDPRProperties.Remove(nameof(GDPRWriteRequestDTO.UserSupervision));
            if (noUserSupervisionDate) GDPRProperties.Remove(nameof(GDPRWriteRequestDTO.UserSupervisionDate));
            if (noUserSupervisionDocumentation) GDPRProperties.Remove(nameof(GDPRWriteRequestDTO.UserSupervisionDocumentation));
            if (noRiskAssessmentConducted) GDPRProperties.Remove(nameof(GDPRWriteRequestDTO.RiskAssessmentConducted));
            if (noRiskAssessmentConductedDate) GDPRProperties.Remove(nameof(GDPRWriteRequestDTO.RiskAssessmentConductedDate));
            if (noRiskAssessmentResult) GDPRProperties.Remove(nameof(GDPRWriteRequestDTO.RiskAssessmentResult));
            if (noRiskAssessmentDocumentation) GDPRProperties.Remove(nameof(GDPRWriteRequestDTO.RiskAssessmentDocumentation));
            if (noRiskAssessmentNotes) GDPRProperties.Remove(nameof(GDPRWriteRequestDTO.RiskAssessmentNotes));
            if (noPlannedRiskAssessmentDate) GDPRProperties.Remove(nameof(GDPRWriteRequestDTO.PlannedRiskAssessmentDate));
            if (noDPIAConducted) GDPRProperties.Remove(nameof(GDPRWriteRequestDTO.DPIAConducted));
            if (noDPIADate) GDPRProperties.Remove(nameof(GDPRWriteRequestDTO.DPIADate));
            if (noDPIADocumentation) GDPRProperties.Remove(nameof(GDPRWriteRequestDTO.DPIADocumentation));
            if (noRetentionPeriodDefined) GDPRProperties.Remove(nameof(GDPRWriteRequestDTO.RetentionPeriodDefined));
            if (noNextDataRetentionEvaluationDate) GDPRProperties.Remove(nameof(GDPRWriteRequestDTO.NextDataRetentionEvaluationDate));
            if (noDataRetentionEvaluationFrequencyInMonths) GDPRProperties.Remove(nameof(GDPRWriteRequestDTO.DataRetentionEvaluationFrequencyInMonths));
            if (noPersonalData) GDPRProperties.Remove(nameof(GDPRWriteRequestDTO.SpecificPersonalData));

            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(nameof(UpdateItSystemUsageRequestDTO.GDPR).WrapAsEnumerable().AsParameterMatch())).Returns(GDPRProperties);
        }

        private void ConfigureArchivingDataProperties(
            bool noArchiveDuty,
            bool noTypeUuid,
            bool noLocationUuid,
            bool noTestLocationUuid,
            bool noSupplierOrganizationUuid,
            bool noActive,
            bool noNotes,
            bool noFrequencyInMonths,
            bool noDocumentBearing,
            bool noJournalPeriods)
        {
            var ArchivingProperties = GetAllInputPropertyNames<ArchivingUpdateRequestDTO>();
            if (noArchiveDuty) ArchivingProperties.Remove(nameof(ArchivingUpdateRequestDTO.ArchiveDuty));
            if (noTypeUuid) ArchivingProperties.Remove(nameof(ArchivingUpdateRequestDTO.TypeUuid));
            if (noLocationUuid) ArchivingProperties.Remove(nameof(ArchivingUpdateRequestDTO.LocationUuid));
            if (noTestLocationUuid) ArchivingProperties.Remove(nameof(ArchivingUpdateRequestDTO.TestLocationUuid));
            if (noSupplierOrganizationUuid) ArchivingProperties.Remove(nameof(ArchivingUpdateRequestDTO.SupplierOrganizationUuid));
            if (noActive) ArchivingProperties.Remove(nameof(ArchivingUpdateRequestDTO.Active));
            if (noNotes) ArchivingProperties.Remove(nameof(ArchivingUpdateRequestDTO.Notes));
            if (noFrequencyInMonths) ArchivingProperties.Remove(nameof(ArchivingUpdateRequestDTO.FrequencyInMonths));
            if (noDocumentBearing) ArchivingProperties.Remove(nameof(ArchivingUpdateRequestDTO.DocumentBearing));
            if (noJournalPeriods) ArchivingProperties.Remove(nameof(IHasJournalPeriods<JournalPeriodDTO>.JournalPeriods));

            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(nameof(UpdateItSystemUsageRequestDTO.Archiving).WrapAsEnumerable().AsParameterMatch())).Returns(ArchivingProperties);
        }

        private void ConfigureKLEDataProperties(
            bool noAddedKLEUuids,
            bool noRemovedKLEUuids)
        {
            var KLEProperties = GetAllInputPropertyNames<LocalKLEDeviationsRequestDTO>();
            if (noAddedKLEUuids) KLEProperties.Remove(nameof(LocalKLEDeviationsRequestDTO.AddedKLEUuids));
            if (noRemovedKLEUuids) KLEProperties.Remove(nameof(LocalKLEDeviationsRequestDTO.RemovedKLEUuids));

            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(nameof(UpdateItSystemUsageRequestDTO.LocalKleDeviations).WrapAsEnumerable().AsParameterMatch())).Returns(KLEProperties);
        }

        private void ConfigureOrganizationUsageDataProperties(
            bool noResponsibleOrganizationUnitUuid,
            bool noUsingOrganizationUnitUuids)
        {
            var organizationUsageProperties = GetAllInputPropertyNames<OrganizationUsageWriteRequestDTO>();
            if (noResponsibleOrganizationUnitUuid) organizationUsageProperties.Remove(nameof(OrganizationUsageWriteRequestDTO.ResponsibleOrganizationUnitUuid));
            if (noUsingOrganizationUnitUuids) organizationUsageProperties.Remove(nameof(OrganizationUsageWriteRequestDTO.UsingOrganizationUnitUuids));

            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(nameof(UpdateItSystemUsageRequestDTO.OrganizationUsage).WrapAsEnumerable().AsParameterMatch())).Returns(organizationUsageProperties);
        }

        private void ConfigureGeneralDataProperties(
            bool noLocalCallName,
            bool noLocalSystemId,
            bool noDataClassificationUuid,
            bool noNotes,
            bool noSystemVersion,
            bool noNumberOfExpectedUsers,
            bool noLifeCycleStatus,
            bool noValidFrom,
            bool noValidTo,
            bool noMainContractUuid)
        {
            var generalProperties = GetAllInputPropertyNames<GeneralDataUpdateRequestDTO>();
            if (noLocalCallName) generalProperties.Remove(nameof(GeneralDataUpdateRequestDTO.LocalCallName));
            if (noLocalSystemId) generalProperties.Remove(nameof(GeneralDataUpdateRequestDTO.LocalSystemId));
            if (noDataClassificationUuid) generalProperties.Remove(nameof(GeneralDataUpdateRequestDTO.DataClassificationUuid));
            if (noNotes) generalProperties.Remove(nameof(GeneralDataUpdateRequestDTO.Notes));
            if (noSystemVersion) generalProperties.Remove(nameof(GeneralDataUpdateRequestDTO.SystemVersion));
            if (noNumberOfExpectedUsers) generalProperties.Remove(nameof(GeneralDataUpdateRequestDTO.NumberOfExpectedUsers));
            if (noMainContractUuid) generalProperties.Remove(nameof(GeneralDataUpdateRequestDTO.MainContractUuid));

            var validityProperties = GetAllInputPropertyNames<ItSystemUsageValidityWriteRequestDTO>();
            if (noLifeCycleStatus) validityProperties.Remove(nameof(ItSystemUsageValidityWriteRequestDTO.LifeCycleStatus));
            if (noValidFrom) validityProperties.Remove(nameof(ItSystemUsageValidityWriteRequestDTO.ValidFrom));
            if (noValidTo) validityProperties.Remove(nameof(ItSystemUsageValidityWriteRequestDTO.ValidTo));

            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(nameof(UpdateItSystemUsageRequestDTO.General).WrapAsEnumerable().AsParameterMatch())).Returns(generalProperties);
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(new[] { nameof(UpdateItSystemUsageRequestDTO.General), nameof(UpdateItSystemUsageRequestDTO.General.Validity) }.AsParameterMatch())).Returns(validityProperties);
        }

        private void ConfigureRootProperties(
            bool noGeneralSection,
            bool noRoles,
            bool noKle,
            bool noOrgUsage,
            bool noReferences,
            bool noArchiving,
            bool noGdpr)
        {
            var properties = GetAllInputPropertyNames<UpdateItSystemUsageRequestDTO>();
            if (noGeneralSection) properties.Remove(nameof(UpdateItSystemUsageRequestDTO.General));
            if (noRoles) properties.Remove(nameof(UpdateItSystemUsageRequestDTO.Roles));
            if (noKle) properties.Remove(nameof(UpdateItSystemUsageRequestDTO.LocalKleDeviations));
            if (noOrgUsage) properties.Remove(nameof(UpdateItSystemUsageRequestDTO.OrganizationUsage));
            if (noReferences) properties.Remove(nameof(UpdateItSystemUsageRequestDTO.ExternalReferences));
            if (noArchiving) properties.Remove(nameof(UpdateItSystemUsageRequestDTO.Archiving));
            if (noGdpr) properties.Remove(nameof(UpdateItSystemUsageRequestDTO.GDPR));
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(Enumerable.Empty<string>().AsParameterMatch())).Returns(properties);
        }

        private static void AssertLinkMapping(SimpleLinkDTO sourceData, OptionalValueChange<Maybe<NamedLink>> actual)
        {
            Assert.Equal(sourceData.Name, AssertPropertyContainsDataChange(actual).Name);
            Assert.Equal(sourceData.Url, AssertPropertyContainsDataChange(actual).Url);
        }

        private static void AssertCommonGeneralDataWriteProperties(GeneralDataWriteRequestDTO input, UpdatedSystemUsageGeneralProperties output)
        {

            Assert.Equal(input.LocalCallName, AssertPropertyContainsDataChange(output.LocalCallName));
            Assert.Equal(input.LocalSystemId, AssertPropertyContainsDataChange(output.LocalSystemId));
            Assert.Equal(input.SystemVersion, AssertPropertyContainsDataChange(output.SystemVersion));
            Assert.Equal(input.DataClassificationUuid, AssertPropertyContainsDataChange(output.DataClassificationUuid));
            Assert.Equal(input.Validity?.LifeCycleStatus, AssertPropertyContainsDataChange(output.LifeCycleStatus)?.ToLifeCycleStatusChoice());
            Assert.Equal(input.Validity?.ValidFrom, AssertPropertyContainsDataChange(output.ValidFrom));
            Assert.Equal(input.Validity?.ValidTo, AssertPropertyContainsDataChange(output.ValidTo));
            Assert.Equal(input.Notes, AssertPropertyContainsDataChange(output.Notes));
            Assert.Equal(input.NumberOfExpectedUsers.LowerBound,
                AssertPropertyContainsDataChange(output.NumberOfExpectedUsersInterval).lower);
            Assert.Equal(input.NumberOfExpectedUsers.UpperBound,
                AssertPropertyContainsDataChange(output.NumberOfExpectedUsersInterval).upperBound);
            AssertContainsAiTechnology(input.ContainsAITechnology, output.ContainsAITechnology);
            Assert.Equal(input.WebAccessibilityCompliance, AssertPropertyContainsDataChange(output.WebAccessibilityCompliance).ToYesNoPartiallyChoice());
            Assert.Equal(input.LastWebAccessibilityCheck, AssertPropertyContainsDataChange(output.LastWebAccessibilityCheck));
            Assert.Equal(input.WebAccessibilityNotes, AssertPropertyContainsDataChange(output.WebAccessibilityNotes));
        }

        private static void AssertContainsAiTechnology(YesNoUndecidedChoice? expected, OptionalValueChange<Maybe<YesNoUndecidedOption>> actual)
        {
            if (!actual.HasChange)
            {
                Assert.Null(expected);
            }
            else
            {
                var maybe = actual.NewValue;
                Assert.True(maybe.HasValue);
                var mappedActual = maybe.Value.ToYesNoUndecidedChoice();
                Assert.Equal(expected, mappedActual);
            }
        }

        private static void AssertKLE(IEnumerable<Guid> expected, OptionalValueChange<Maybe<IEnumerable<Guid>>> actual)
        {
            Assert.True(actual.HasChange);
            Assert.True(actual.NewValue.HasValue);
            var mappedUuids = actual.NewValue.Value;
            Assert.Equal(expected, mappedUuids);
        }

    }
}
