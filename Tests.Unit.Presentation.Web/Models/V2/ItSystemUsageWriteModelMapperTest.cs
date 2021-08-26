using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.SystemUsage.Write;
using Infrastructure.Services.Types;
using Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;
using Presentation.Web.Models.API.V2.Request.SystemUsage;
using Presentation.Web.Models.API.V2.Types.Shared;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    public class ItSystemUsageWriteModelMapperTest : WithAutoFixture
    {
        private readonly ItSystemUsageWriteModelMapper _sut;

        public ItSystemUsageWriteModelMapperTest()
        {
            _sut = new ItSystemUsageWriteModelMapper();
        }

        [Fact]
        public void Can_Map_Roles()
        {
            //Arrange
            var roles = Many<RoleAssignmentRequestDTO>().OrderBy(x => x.RoleUuid).ToList();

            //Act
            var systemUsageRoles = _sut.MapRoles(roles);

            //Assert
            var userRolePairs = AssertPropertyContainsDataChange(systemUsageRoles.UserRolePairs).OrderBy(x => x.RoleUuid).ToList();
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
            var mappedReferences = _sut.MapReferences(references).OrderBy(x => x.Url).ToList();

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
            var output = _sut.MapKle(input);

            //Assert
            AssertKLE(input.AddedKLEUuids, output.AddedKLEUuids);
            AssertKLE(input.RemovedKLEUuids, output.RemovedKLEUuids);
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
            var output = _sut.MapKle(input);

            //Assert that null is translated into a reset value (change to "none")
            Assert.Equal(addedNull, output.AddedKLEUuids.NewValue.IsNone);
            Assert.Equal(removedNull, output.RemovedKLEUuids.NewValue.IsNone);
        }

        [Fact]
        public void Can_Map_OrganizationUsage()
        {
            //Arrange
            var input = A<OrganizationUsageWriteRequestDTO>();

            //Act
            var output = _sut.MapOrganizationalUsage(input);

            //Assert 
            var responsible = AssertPropertyContainsDataChange(output.ResponsibleOrganizationUnitUuid);
            var usingOrgUnits = AssertPropertyContainsDataChange(output.UsingOrganizationUnitUuids);
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
            var output = _sut.MapOrganizationalUsage(input);

            //Assert 
            if (responsibleIsNull)
                AssertPropertyContainsResetDataChange(output.ResponsibleOrganizationUnitUuid);
            else
                AssertPropertyContainsDataChange(output.ResponsibleOrganizationUnitUuid);
            if (unitsAreNull)
                AssertPropertyContainsResetDataChange(output.UsingOrganizationUnitUuids);
            else
                AssertPropertyContainsDataChange(output.UsingOrganizationUnitUuids);
        }

        [Fact]
        public void Can_Map_General_Data_Properties()
        {
            //Arrange
            var input = A<GeneralDataWriteRequestDTO>();

            //Act
            var output = _sut.MapGeneralData(input);

            //Assert
            AssertCommonGeneralDataWriteProperties(input, output);
            Assert.True(output.MainContractUuid.IsUnchanged, "The main contract should be untouched as it is not part of the initial write contract");
        }

        [Fact]
        public void Map_General_Data_Properties_Resets_Validity_If_SourceValue_Not_Defined()
        {
            //Arrange
            var input = A<GeneralDataWriteRequestDTO>();
            input.Validity = null;

            //Act
            var output = _sut.MapGeneralData(input);

            //Assert
            AssertPropertyContainsResetDataChange(output.EnforceActive);
            AssertPropertyContainsResetDataChange(output.ValidFrom);
            AssertPropertyContainsResetDataChange(output.ValidTo);
        }

        [Fact]
        public void Map_General_Data_Properties_Resets_NumberOfExpectedUsers_If_SourceValue_Not_Defined()
        {
            //Arrange
            var input = A<GeneralDataWriteRequestDTO>();
            input.NumberOfExpectedUsers = null;

            //Act
            var output = _sut.MapGeneralData(input);

            //Assert
            AssertPropertyContainsResetDataChange(output.NumberOfExpectedUsersInterval);
        }

        [Fact]
        public void Map_General_Data_Properties_Resets_AssociatedProjects_If_SourceValue_Not_Defined()
        {
            //Arrange
            var input = A<GeneralDataWriteRequestDTO>();
            input.AssociatedProjectUuids = null;

            //Act
            var output = _sut.MapGeneralData(input);

            //Assert
            AssertPropertyContainsResetDataChange(output.AssociatedProjectUuids);
        }

        [Fact]
        public void Can_Map_General_Data_Update_Properties()
        {
            //Arrange
            var input = A<GeneralDataUpdateRequestDTO>();

            //Act
            var output = _sut.MapGeneralDataUpdate(input);

            //Assert
            AssertCommonGeneralDataWriteProperties(input, output);
            Assert.Equal(input.MainContractUuid, AssertPropertyContainsDataChange(output.MainContractUuid));
        }

        [Fact]
        public void Map_General_Data_Update_Properties_Resets_Main_Contract_Id_If_Undefined()
        {
            //Arrange
            var input = A<GeneralDataUpdateRequestDTO>();
            input.MainContractUuid = null;

            //Act
            var output = _sut.MapGeneralDataUpdate(input);

            //Assert
            AssertCommonGeneralDataWriteProperties(input, output);
            AssertPropertyContainsResetDataChange(output.MainContractUuid);
        }

        [Fact]
        public void Can_Map_GDPR_Data_Properties()
        {
            //Arrange
            var input = A<GDPRWriteRequestDTO>();

            //Act
            var output = _sut.MapGDPR(input);

            //Assert
            Assert.Equal(input.Purpose, AssertPropertyContainsDataChange(output.Purpose));
            Assert.Equal(input.BusinessCritical, AssertPropertyContainsDataChange(output.BusinessCritical)?.ToYesNoDontKnowChoice());
            Assert.Equal(input.HostedAt, AssertPropertyContainsDataChange(output.HostedAt)?.ToHostingChoice());
            AssertLinkMapping(input.DirectoryDocumentation, output.DirectoryDocumentation);
            Assert.Equal(input.DataSensitivityLevels.ToList(), AssertPropertyContainsDataChange(output.DataSensitivityLevels).Select(x => x.ToDataSensitivityLevelChoice()));
            Assert.Equal(input.SensitivePersonDataUuids.ToList(), AssertPropertyContainsDataChange(output.SensitivePersonDataUuids));
            Assert.Equal(input.RegisteredDataCategoryUuids.ToList(), AssertPropertyContainsDataChange(output.RegisteredDataCategoryUuids));
            Assert.Equal(input.TechnicalPrecautionsInPlace, AssertPropertyContainsDataChange(output.TechnicalPrecautionsInPlace)?.ToYesNoDontKnowChoice());
            Assert.Equal(input.TechnicalPrecautionsApplied.ToList(), AssertPropertyContainsDataChange(output.TechnicalPrecautionsApplied).Select(x => x.ToTechnicalPrecautionChoice()));
            AssertLinkMapping(input.TechnicalPrecautionsDocumentation, output.TechnicalPrecautionsDocumentation);
            Assert.Equal(input.UserSupervision, AssertPropertyContainsDataChange(output.UserSupervision)?.ToYesNoDontKnowChoice());
            Assert.Equal(input.UserSupervisionDate, AssertPropertyContainsDataChange(output.UserSupervisionDate));
            AssertLinkMapping(input.UserSupervisionDocumentation, output.UserSupervisionDocumentation);
            Assert.Equal(input.RiskAssessmentConducted, AssertPropertyContainsDataChange(output.RiskAssessmentConducted)?.ToYesNoDontKnowChoice());
            Assert.Equal(input.RiskAssessmentNotes, AssertPropertyContainsDataChange(output.RiskAssessmentNotes));
            Assert.Equal(input.RiskAssessmentConductedDate, AssertPropertyContainsDataChange(output.RiskAssessmentConductedDate));
            AssertLinkMapping(input.RiskAssessmentDocumentation, output.RiskAssessmentDocumentation);
            Assert.Equal(input.RiskAssessmentResult, AssertPropertyContainsDataChange(output.RiskAssessmentResult)?.ToRiskLevelChoice());
            Assert.Equal(input.DPIAConducted, AssertPropertyContainsDataChange(output.DPIAConducted)?.ToYesNoDontKnowChoice());
            Assert.Equal(input.DPIADate, AssertPropertyContainsDataChange(output.DPIADate));
            Assert.Equal(input.NextDataRetentionEvaluationDate, AssertPropertyContainsDataChange(output.NextDataRetentionEvaluationDate));
            Assert.Equal(input.DataRetentionEvaluationFrequencyInMonths, AssertPropertyContainsDataChange(output.DataRetentionEvaluationFrequencyInMonths));
            AssertLinkMapping(input.DPIADocumentation, output.DPIADocumentation);
        }

        [Fact]
        public void Map_GDPR_Data_Properties_Maps_Reset_If_Directory_Documentation_Is_Null()
        {
            //Arrange
            var input = A<GDPRWriteRequestDTO>();
            input.DirectoryDocumentation = null;

            //Act
            var output = _sut.MapGDPR(input);

            //Assert
            AssertPropertyContainsResetDataChange(output.DirectoryDocumentation);
        }

        [Fact]
        public void Map_GDPR_Data_Properties_Maps_Reset_If_DataSensitivityLevels_Is_Null()
        {
            //Arrange
            var input = A<GDPRWriteRequestDTO>();
            input.DataSensitivityLevels = null;

            //Act
            var output = _sut.MapGDPR(input);

            //Assert
            AssertPropertyContainsResetDataChange(output.DataSensitivityLevels);
        }

        [Fact]
        public void Map_GDPR_Data_Properties_Maps_Reset_If_SensitivePersonDataUuids_Is_Null()
        {
            //Arrange
            var input = A<GDPRWriteRequestDTO>();
            input.SensitivePersonDataUuids = null;

            //Act
            var output = _sut.MapGDPR(input);

            //Assert
            AssertPropertyContainsResetDataChange(output.SensitivePersonDataUuids);
        }

        [Fact]
        public void Map_GDPR_Data_Properties_Maps_Reset_If_RegisteredDataCategoryUuids_Is_Null()
        {
            //Arrange
            var input = A<GDPRWriteRequestDTO>();
            input.RegisteredDataCategoryUuids = null;

            //Act
            var output = _sut.MapGDPR(input);

            //Assert
            AssertPropertyContainsResetDataChange(output.RegisteredDataCategoryUuids);
        }

        [Fact]
        public void Map_GDPR_Data_Properties_Maps_Reset_If_TechnicalPrecautionsApplied_Is_Null()
        {
            //Arrange
            var input = A<GDPRWriteRequestDTO>();
            input.TechnicalPrecautionsApplied = null;

            //Act
            var output = _sut.MapGDPR(input);

            //Assert
            AssertPropertyContainsResetDataChange(output.TechnicalPrecautionsApplied);
        }

        [Fact]
        public void Map_GDPR_Data_Properties_Maps_Reset_If_TechnicalPrecautionsDocumentation_Is_Null()
        {
            //Arrange
            var input = A<GDPRWriteRequestDTO>();
            input.TechnicalPrecautionsDocumentation = null;

            //Act
            var output = _sut.MapGDPR(input);

            //Assert
            AssertPropertyContainsResetDataChange(output.TechnicalPrecautionsDocumentation);
        }

        [Fact]
        public void Map_GDPR_Data_Properties_Maps_Reset_If_UserSupervisionDocumentation_Is_Null()
        {
            //Arrange
            var input = A<GDPRWriteRequestDTO>();
            input.UserSupervisionDocumentation = null;

            //Act
            var output = _sut.MapGDPR(input);

            //Assert
            AssertPropertyContainsResetDataChange(output.UserSupervisionDocumentation);
        }

        [Fact]
        public void Map_GDPR_Data_Properties_Maps_Reset_If_RiskAssessmentDocumentation_Is_Null()
        {
            //Arrange
            var input = A<GDPRWriteRequestDTO>();
            input.RiskAssessmentDocumentation = null;

            //Act
            var output = _sut.MapGDPR(input);

            //Assert
            AssertPropertyContainsResetDataChange(output.RiskAssessmentDocumentation);
        }

        [Fact]
        public void Map_GDPR_Data_Properties_Maps_Reset_If_DPIADocumentation_Is_Null()
        {
            //Arrange
            var input = A<GDPRWriteRequestDTO>();
            input.DPIADocumentation = null;

            //Act
            var output = _sut.MapGDPR(input);

            //Assert
            AssertPropertyContainsResetDataChange(output.DPIADocumentation);
        }

        [Fact]
        public void Can_Map_Archiving()
        {
            //Arrange
            var input = A<ArchivingWriteRequestDTO>();

            //Act
            var output = _sut.MapArchiving(input);

            //Assert
            Assert.Equal(input.ArchiveDuty, AssertPropertyContainsDataChange(output.ArchiveDuty)?.ToArchiveDutyChoice());
            Assert.Equal(input.TypeUuid, AssertPropertyContainsDataChange(output.ArchiveTypeUuid));
            Assert.Equal(input.LocationUuid, AssertPropertyContainsDataChange(output.ArchiveLocationUuid));
            Assert.Equal(input.TestLocationUuid, AssertPropertyContainsDataChange(output.ArchiveTestLocationUuid));
            Assert.Equal(input.SupplierOrganizationUuid, AssertPropertyContainsDataChange(output.ArchiveSupplierOrganizationUuid));
            Assert.Equal(input.Active, AssertPropertyContainsDataChange(output.ArchiveActive));
            Assert.Equal(input.Notes, AssertPropertyContainsDataChange(output.ArchiveNotes));
            Assert.Equal(input.FrequencyInMonths, AssertPropertyContainsDataChange(output.ArchiveFrequencyInMonths));
            Assert.Equal(input.DocumentBearing, AssertPropertyContainsDataChange(output.ArchiveDocumentBearing));

            var inputPeriods = input.JournalPeriods.OrderBy(_ => _.ArchiveId).ToList();
            var mappedPeriods = AssertPropertyContainsDataChange(output.ArchiveJournalPeriods).OrderBy(_ => _.ArchiveId).ToList();
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
            var output = _sut.MapArchiving(input);

            //Assert
            AssertPropertyContainsResetDataChange(output.ArchiveJournalPeriods);
        }

        [Theory]
        [InlineData(true, true, true, true, true, true, true)]
        [InlineData(true, true, true, true, true, true, false)]
        [InlineData(true, true, true, true, true, false, false)]
        [InlineData(true, true, true, true, false, false, false)]
        [InlineData(true, true, true, false, false, false, false)]
        [InlineData(true, true, false, false, false, false, false)]
        [InlineData(true, false, false, false, false, false, false)]
        [InlineData(false, false, false, false, false, false, false)]
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
        [InlineData(true, true, true, true, true, true, true)]
        [InlineData(true, true, true, true, true, true, false)]
        [InlineData(true, true, true, true, true, false, false)]
        [InlineData(true, true, true, true, false, false, false)]
        [InlineData(true, true, true, false, false, false, false)]
        [InlineData(true, true, false, false, false, false, false)]
        [InlineData(true, false, false, false, false, false, false)]
        [InlineData(false, false, false, false, false, false, false)]
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

            //Assert that all sections are mapped as changed - including undefined sections
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
            Assert.Equal(input.ToSystemUsageUuid,output.ToSystemUsageUuid);
            Assert.Equal(input.AssociatedContractUuid,output.AssociatedContractUuid);
            Assert.Equal(input.RelationFrequencyUuid,output.RelationFrequencyUuid);
            Assert.Equal(input.RelationInterfaceUuid,output.UsingInterfaceUuid);
            Assert.Equal(input.Description,output.Description);
            Assert.Equal(input.UrlReference,output.UrlReference);
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

        private static T AssertPropertyContainsDataChange<T>(OptionalValueChange<Maybe<T>> sourceData)
        {
            Assert.True(sourceData.HasChange);
            Assert.True(sourceData.NewValue.HasValue);
            return sourceData.NewValue.Value;
        }

        private static T AssertPropertyContainsDataChange<T>(OptionalValueChange<T> sourceData)
        {
            Assert.True(sourceData.HasChange);
            return sourceData.NewValue;
        }

        private static void AssertPropertyContainsResetDataChange<T>(OptionalValueChange<Maybe<T>> sourceData)
        {
            Assert.True(sourceData.HasChange);
            Assert.True(sourceData.NewValue.IsNone);
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
