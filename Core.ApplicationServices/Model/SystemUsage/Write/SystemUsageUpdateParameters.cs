using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Shared.Write;


namespace Core.ApplicationServices.Model.SystemUsage.Write
{
    public class SystemUsageUpdateParameters : ISupplierAssociatedEntityUpdateParameters
    {
        public Maybe<UpdatedSystemUsageGeneralProperties> GeneralProperties { get; set; } = Maybe<UpdatedSystemUsageGeneralProperties>.None;
        public Maybe<UpdatedSystemUsageOrganizationalUseParameters> OrganizationalUsage { get; set; } = Maybe<UpdatedSystemUsageOrganizationalUseParameters>.None;
        public Maybe<UpdatedSystemUsageKLEDeviationParameters> KLE { get; set; } = Maybe<UpdatedSystemUsageKLEDeviationParameters>.None;
        public Maybe<IEnumerable<UpdatedExternalReferenceProperties>> ExternalReferences { get; set; } = Maybe<IEnumerable<UpdatedExternalReferenceProperties>>.None;
        public Maybe<UpdatedSystemUsageRoles> Roles { get; set; } = Maybe<UpdatedSystemUsageRoles>.None;
        public Maybe<UpdatedSystemUsageGDPRProperties> GDPR { get; set; } = Maybe<UpdatedSystemUsageGDPRProperties>.None;
        public Maybe<UpdatedSystemUsageArchivingParameters> Archiving { get; set; } = Maybe<UpdatedSystemUsageArchivingParameters>.None;

        public IEnumerable<string> GetChangedPropertyKeys()
        {
            var changed = new List<string>();

            if (GeneralProperties.HasValue)
                changed.AddRange(GetChangedGeneralProperties());

            if (OrganizationalUsage.HasValue)
                changed.AddRange(GetChangedOrganizationalUsage());

            if (KLE.HasValue)
                changed.AddRange(GetChangedKLE());

            if (ExternalReferences.HasValue)
                changed.Add(nameof(ExternalReferences));

            if (Roles.HasValue)
                changed.AddRange(GetChangedRoles());

            if (GDPR.HasValue)
                changed.AddRange(GetChangedGDPR());

            if (Archiving.HasValue)
                changed.AddRange(GetChangedArchiving());

            return changed;
        }

        private IEnumerable<string> GetChangedGeneralProperties()
        {
            var changed = new List<string>();
            var general = GeneralProperties.Value;
            general.LocalSystemId.AddIfChanged(changed, nameof(general.LocalSystemId));
            general.LocalCallName.AddIfChanged(changed, nameof(general.LocalCallName));
            general.DataClassificationUuid.AddIfChanged(changed, nameof(general.DataClassificationUuid));
            general.Notes.AddIfChanged(changed, nameof(general.Notes));
            general.SystemVersion.AddIfChanged(changed, nameof(general.SystemVersion));
            general.NumberOfExpectedUsersInterval.AddIfChanged(changed, nameof(general.NumberOfExpectedUsersInterval));
            general.LifeCycleStatus.AddIfChanged(changed, nameof(general.LifeCycleStatus));
            general.ValidFrom.AddIfChanged(changed, nameof(general.ValidFrom));
            general.ValidTo.AddIfChanged(changed, nameof(general.ValidTo));
            general.MainContractUuid.AddIfChanged(changed, nameof(general.MainContractUuid));
            general.ContainsAITechnology.AddIfChanged(changed, nameof(general.ContainsAITechnology));
            general.WebAccessibilityCompliance.AddIfChanged(changed, nameof(general.WebAccessibilityCompliance));
            general.LastWebAccessibilityCheck.AddIfChanged(changed, nameof(general.LastWebAccessibilityCheck));
            general.WebAccessibilityNotes.AddIfChanged(changed, nameof(general.WebAccessibilityNotes));
            general.IsSociallyCritical.AddIfChanged(changed, nameof(general.IsSociallyCritical));
            general.BusinessCritical.AddIfChanged(changed, nameof(general.BusinessCritical));
            general.SystemUsageCriticalityLevelUuid.AddIfChanged(changed, nameof(general.SystemUsageCriticalityLevelUuid));
            general.CriticalityLevelDocumentation.AddIfChanged(changed, nameof(general.CriticalityLevelDocumentation));
            general.Purpose.AddIfChanged(changed, nameof(general.Purpose));
            general.TechnicalSystemTypeUuids.AddIfChanged(changed, nameof(general.TechnicalSystemTypeUuids));
            general.HostedAt.AddIfChanged(changed, nameof(general.HostedAt));
            return changed;
        }

        private IEnumerable<string> GetChangedOrganizationalUsage()
        {
            var changed = new List<string>();
            var org = OrganizationalUsage.Value;
            org.UsingOrganizationUnitUuids.AddIfChanged(changed, nameof(org.UsingOrganizationUnitUuids));
            org.ResponsibleOrganizationUnitUuid.AddIfChanged(changed, nameof(org.ResponsibleOrganizationUnitUuid));
            return changed;
        }

        private IEnumerable<string> GetChangedKLE()
        {
            var changed = new List<string>();
            var kle = KLE.Value;
            kle.AddedKLEUuids.AddIfChanged(changed, nameof(kle.AddedKLEUuids));
            kle.RemovedKLEUuids.AddIfChanged(changed, nameof(kle.RemovedKLEUuids));
            return changed;
        }

        private IEnumerable<string> GetChangedRoles()
        {
            var changed = new List<string>();
            var roles = Roles.Value;
            roles.UserRolePairs.AddIfChanged(changed, nameof(roles.UserRolePairs));
            return changed;
        }

        private IEnumerable<string> GetChangedGDPR()
        {
            var changed = new List<string>();
            var gdpr = GDPR.Value;
            gdpr.PersonalDataDeletionCapability.AddIfChanged(changed, nameof(gdpr.PersonalDataDeletionCapability));
            gdpr.PersonalDataDeletionMethod.AddIfChanged(changed, nameof(gdpr.PersonalDataDeletionMethod));
            gdpr.PersonalDataDeletionDeadlineTypeUuid.AddIfChanged(changed, nameof(gdpr.PersonalDataDeletionDeadlineTypeUuid));
            gdpr.PersonalDataDeletionProcedureDocumentation.AddIfChanged(changed, nameof(gdpr.PersonalDataDeletionProcedureDocumentation));
            gdpr.PersonalDataDeletionResponsibleUserUuid.AddIfChanged(changed, nameof(gdpr.PersonalDataDeletionResponsibleUserUuid));
            gdpr.PersonalDataDeletionVerifiedDate.AddIfChanged(changed, nameof(gdpr.PersonalDataDeletionVerifiedDate));
            gdpr.PersonalDataDeletionVerificationDocumentation.AddIfChanged(changed, nameof(gdpr.PersonalDataDeletionVerificationDocumentation));
            gdpr.ProcessingPurpose.AddIfChanged(changed, nameof(gdpr.ProcessingPurpose));
            gdpr.DirectoryDocumentation.AddIfChanged(changed, nameof(gdpr.DirectoryDocumentation));
            gdpr.DataSensitivityLevels.AddIfChanged(changed, nameof(gdpr.DataSensitivityLevels));
            gdpr.SensitivePersonDataUuids.AddIfChanged(changed, nameof(gdpr.SensitivePersonDataUuids));
            gdpr.PersonalDataOptions.AddIfChanged(changed, nameof(gdpr.PersonalDataOptions));
            gdpr.RegisteredDataCategoryUuids.AddIfChanged(changed, nameof(gdpr.RegisteredDataCategoryUuids));
            gdpr.TechnicalPrecautionsInPlace.AddIfChanged(changed, nameof(gdpr.TechnicalPrecautionsInPlace));
            gdpr.TechnicalPrecautionsApplied.AddIfChanged(changed, nameof(gdpr.TechnicalPrecautionsApplied));
            gdpr.TechnicalPrecautionsDocumentation.AddIfChanged(changed, nameof(gdpr.TechnicalPrecautionsDocumentation));
            gdpr.UserSupervision.AddIfChanged(changed, nameof(gdpr.UserSupervision));
            gdpr.UserSupervisionDate.AddIfChanged(changed, nameof(gdpr.UserSupervisionDate));
            gdpr.UserSupervisionDocumentation.AddIfChanged(changed, nameof(gdpr.UserSupervisionDocumentation));
            gdpr.RiskAssessmentConducted.AddIfChanged(changed, nameof(gdpr.RiskAssessmentConducted));
            gdpr.RiskAssessmentConductedDate.AddIfChanged(changed, nameof(gdpr.RiskAssessmentConductedDate));
            gdpr.RiskAssessmentResult.AddIfChanged(changed, nameof(gdpr.RiskAssessmentResult));
            gdpr.RiskAssessmentDocumentation.AddIfChanged(changed, nameof(gdpr.RiskAssessmentDocumentation));
            gdpr.PlannedRiskAssessmentDate.AddIfChanged(changed, nameof(gdpr.PlannedRiskAssessmentDate));
            gdpr.RiskAssessmentNotes.AddIfChanged(changed, nameof(gdpr.RiskAssessmentNotes));
            gdpr.DPIAConducted.AddIfChanged(changed, nameof(gdpr.DPIAConducted));
            gdpr.DPIADate.AddIfChanged(changed, nameof(gdpr.DPIADate));
            gdpr.DPIADocumentation.AddIfChanged(changed, nameof(gdpr.DPIADocumentation));
            gdpr.RetentionPeriodDefined.AddIfChanged(changed, nameof(gdpr.RetentionPeriodDefined));
            gdpr.NextDataRetentionEvaluationDate.AddIfChanged(changed, nameof(gdpr.NextDataRetentionEvaluationDate));
            gdpr.DataRetentionEvaluationFrequencyInMonths.AddIfChanged(changed, nameof(gdpr.DataRetentionEvaluationFrequencyInMonths));
            return changed;
        }

        private IEnumerable<string> GetChangedArchiving()
        {
            var changed = new List<string>();
            var arch = Archiving.Value;
            arch.ArchiveDuty.AddIfChanged(changed, nameof(arch.ArchiveDuty));
            arch.ArchiveTypeUuid.AddIfChanged(changed, nameof(arch.ArchiveTypeUuid));
            arch.ArchiveLocationUuid.AddIfChanged(changed, nameof(arch.ArchiveLocationUuid));
            arch.ArchiveTestLocationUuid.AddIfChanged(changed, nameof(arch.ArchiveTestLocationUuid));
            arch.ArchiveSupplierOrganizationUuid.AddIfChanged(changed, nameof(arch.ArchiveSupplierOrganizationUuid));
            arch.ArchiveActive.AddIfChanged(changed, nameof(arch.ArchiveActive));
            arch.ArchiveNotes.AddIfChanged(changed, nameof(arch.ArchiveNotes));
            arch.ArchiveFrequencyInMonths.AddIfChanged(changed, nameof(arch.ArchiveFrequencyInMonths));
            arch.ArchiveDocumentBearing.AddIfChanged(changed, nameof(arch.ArchiveDocumentBearing));
            arch.ArchiveJournalPeriods.AddIfChanged(changed, nameof(arch.ArchiveJournalPeriods));
            return changed;
        }

    }
}
