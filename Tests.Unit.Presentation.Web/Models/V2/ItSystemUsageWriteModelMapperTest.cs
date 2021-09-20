using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.SystemUsage.Write;
using Moq;
using Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;
using Presentation.Web.Models.API.V2.Request.SystemUsage;
using Presentation.Web.Models.API.V2.Types.Shared;
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
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonRootProperties())
                .Returns(GetAllInputPropertyNames<UpdateItSystemUsageRequestDTO>());
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
        public void Can_Map_ExternalReferences()
        {
            //Arrange
            var references = Many<ExternalReferenceDataDTO>().OrderBy(x => x.Url).ToList();

            //Act
            var mappedReferences = _sut.FromPATCH(new UpdateItSystemUsageRequestDTO() { ExternalReferences = references }).ExternalReferences.Value.OrderBy(x => x.Url).ToList();

            //Assert
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
            var input = A<GeneralDataWriteRequestDTO>();
            input.Validity = null;

            //Act
            var output = _sut.FromPOST(new CreateItSystemUsageRequestDTO() { General = input });

            //Assert
            var mappedGeneralSection = AssertPropertyContainsDataChange(output.GeneralProperties);
            AssertPropertyContainsResetDataChange(mappedGeneralSection.EnforceActive);
            AssertPropertyContainsResetDataChange(mappedGeneralSection.ValidFrom);
            AssertPropertyContainsResetDataChange(mappedGeneralSection.ValidTo);
        }

        [Fact]
        public void Map_General_Data_Properties_Resets_NumberOfExpectedUsers_If_SourceValue_Not_Defined()
        {
            //Arrange
            var input = A<GeneralDataWriteRequestDTO>();
            input.NumberOfExpectedUsers = null;

            //Act
            var output = _sut.FromPOST(new CreateItSystemUsageRequestDTO() { General = input });

            //Assert
            var mappedGeneralSection = AssertPropertyContainsDataChange(output.GeneralProperties);
            AssertPropertyContainsResetDataChange(mappedGeneralSection.NumberOfExpectedUsersInterval);
        }

        [Fact]
        public void Map_General_Data_Properties_Resets_AssociatedProjects_If_SourceValue_Not_Defined()
        {
            //Arrange
            var input = A<GeneralDataWriteRequestDTO>();
            input.AssociatedProjectUuids = null;

            //Act
            var output = _sut.FromPOST(new CreateItSystemUsageRequestDTO() { General = input });

            //Assert
            var mappedGeneralSection = AssertPropertyContainsDataChange(output.GeneralProperties);
            AssertPropertyContainsResetDataChange(mappedGeneralSection.AssociatedProjectUuids);
        }

        [Fact]
        public void Can_Map_General_Data_Update_Properties()
        {
            //Arrange
            var input = A<GeneralDataUpdateRequestDTO>();

            //Act
            var output = _sut.FromPOST(new CreateItSystemUsageRequestDTO() { General = input });

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
            var output = _sut.FromPATCH(new UpdateItSystemUsageRequestDTO(){General = input});

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
            var output = _sut.FromPATCH(new UpdateItSystemUsageRequestDTO(){GDPR = input});

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
            Assert.Equal(input.DPIAConducted, AssertPropertyContainsDataChange(mappedGdpr.DPIAConducted)?.ToYesNoDontKnowChoice());
            Assert.Equal(input.DPIADate, AssertPropertyContainsDataChange(mappedGdpr.DPIADate));
            Assert.Equal(input.NextDataRetentionEvaluationDate, AssertPropertyContainsDataChange(mappedGdpr.NextDataRetentionEvaluationDate));
            Assert.Equal(input.DataRetentionEvaluationFrequencyInMonths, AssertPropertyContainsDataChange(mappedGdpr.DataRetentionEvaluationFrequencyInMonths));
            AssertLinkMapping(input.DPIADocumentation, mappedGdpr.DPIADocumentation);
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
        public void Can_Map_Archiving()
        {
            //Arrange
            var input = A<ArchivingWriteRequestDTO>();

            //Act
            var output = _sut.FromPATCH(new UpdateItSystemUsageRequestDTO(){Archiving = input});

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

            var inputPeriods = input.JournalPeriods.OrderBy(_ => _.ArchiveId).ToList();
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
            }
        }

        [Fact]
        public void Map_Archiving_Resets_JournalPeriods_If_Null()
        {
            //Arrange
            var input = A<ArchivingWriteRequestDTO>();
            input.JournalPeriods = null;

            //Act
            var output = _sut.FromPATCH(new UpdateItSystemUsageRequestDTO() { Archiving = input });

            //Assert
            var mappedArchiving = AssertPropertyContainsDataChange(output.Archiving);
            AssertPropertyContainsResetDataChange(mappedArchiving.ArchiveJournalPeriods);
        }

        public static IEnumerable<object[]> GetUndefinedSectionsInput()
        {
            return CreateGetUndefinedSectionsInput(7);
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
                ExternalReferences = referencesNull ? null : Many<ExternalReferenceDataDTO>(),
                Archiving = archivingNull ? null : A<ArchivingWriteRequestDTO>(),
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
                ExternalReferences = referencesNull ? null : Many<ExternalReferenceDataDTO>(),
                Archiving = archivingNull ? null : A<ArchivingWriteRequestDTO>(),
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
        public void FromPut_Includedes_Root_Level_Sections_Not_Present_In_The_Request(
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
            var definedProperties = GetAllInputPropertyNames<UpdateItSystemUsageRequestDTO>();
            if (noGeneralSection) definedProperties.Remove(nameof(UpdateItSystemUsageRequestDTO.General));
            if (noRoles) definedProperties.Remove(nameof(UpdateItSystemUsageRequestDTO.Roles));
            if (noKle) definedProperties.Remove(nameof(UpdateItSystemUsageRequestDTO.LocalKleDeviations));
            if (noOrgUsage) definedProperties.Remove(nameof(UpdateItSystemUsageRequestDTO.OrganizationUsage));
            if (noReferences) definedProperties.Remove(nameof(UpdateItSystemUsageRequestDTO.ExternalReferences));
            if (noArchiving) definedProperties.Remove(nameof(UpdateItSystemUsageRequestDTO.Archiving));
            if (noGdpr) definedProperties.Remove(nameof(UpdateItSystemUsageRequestDTO.GDPR));

            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonRootProperties()).Returns(definedProperties);

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
        public void FromPatch_Ignores_Root_Level_Sections_Not_Present_In_The_Request(
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
            var definedProperties = GetAllInputPropertyNames<UpdateItSystemUsageRequestDTO>();
            if (noGeneralSection) definedProperties.Remove(nameof(UpdateItSystemUsageRequestDTO.General));
            if (noRoles) definedProperties.Remove(nameof(UpdateItSystemUsageRequestDTO.Roles));
            if (noKle) definedProperties.Remove(nameof(UpdateItSystemUsageRequestDTO.LocalKleDeviations));
            if (noOrgUsage) definedProperties.Remove(nameof(UpdateItSystemUsageRequestDTO.OrganizationUsage));
            if (noReferences) definedProperties.Remove(nameof(UpdateItSystemUsageRequestDTO.ExternalReferences));
            if (noArchiving) definedProperties.Remove(nameof(UpdateItSystemUsageRequestDTO.Archiving));
            if (noGdpr) definedProperties.Remove(nameof(UpdateItSystemUsageRequestDTO.GDPR));

            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonRootProperties()).Returns(definedProperties);

            //Act
            var output = _sut.FromPATCH(emptyInput);

            //Assert that all sections are mapped as changed - including undefined sections
            Assert.Equal(output.GeneralProperties.IsNone, noGeneralSection);
            Assert.Equal(output.Roles.IsNone, noRoles);
            Assert.Equal(output.KLE.IsNone, noKle);
            Assert.Equal(output.OrganizationalUsage.IsNone, noOrgUsage);
            Assert.Equal(output.ExternalReferences.IsNone, noReferences);
            Assert.Equal(output.Archiving.IsNone, noArchiving);
            Assert.Equal(output.GDPR.IsNone, noGdpr);
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
            Assert.Equal(input.AssociatedProjectUuids, AssertPropertyContainsDataChange(output.AssociatedProjectUuids));
            Assert.Equal(input.DataClassificationUuid, AssertPropertyContainsDataChange(output.DataClassificationUuid));
            Assert.Equal(input.Validity?.EnforcedValid, AssertPropertyContainsDataChange(output.EnforceActive));
            Assert.Equal(input.Validity?.ValidFrom, AssertPropertyContainsDataChange(output.ValidFrom));
            Assert.Equal(input.Validity?.ValidTo, AssertPropertyContainsDataChange(output.ValidTo));
            Assert.Equal(input.Notes, AssertPropertyContainsDataChange(output.Notes));
            Assert.Equal(input.NumberOfExpectedUsers.LowerBound,
                AssertPropertyContainsDataChange(output.NumberOfExpectedUsersInterval).lower);
            Assert.Equal(input.NumberOfExpectedUsers.UpperBound,
                AssertPropertyContainsDataChange(output.NumberOfExpectedUsersInterval).upperBound);
        }

        private static void AssertKLE(IEnumerable<Guid> expected, OptionalValueChange<Maybe<IEnumerable<Guid>>> actual)
        {
            Assert.True(actual.HasChange);
            Assert.True(actual.NewValue.HasValue);
            var mappedUuids = actual.NewValue.Value;
            Assert.Equal(expected, mappedUuids);
        }

        private static HashSet<string> GetAllInputPropertyNames<T>()
        {
            return typeof(T).GetProperties().Select(x => x.Name).ToHashSet();
        }
    }
}
