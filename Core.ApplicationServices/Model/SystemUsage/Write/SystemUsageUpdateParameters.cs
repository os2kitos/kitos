using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared.Write;


namespace Core.ApplicationServices.Model.SystemUsage.Write
{
    public class SystemUsageUpdateParameters: ISupplierAssociatedEntityUpdateParameters
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
            if (general.LocalSystemId.HasChange)
                changed.Add(nameof(general.LocalSystemId));
            if (general.LocalCallName.HasChange)
                changed.Add(nameof(general.LocalCallName));
            if (general.DataClassificationUuid.HasChange)
                changed.Add(nameof(general.DataClassificationUuid));
            if (general.Notes.HasChange)
                changed.Add(nameof(general.Notes));
            if (general.SystemVersion.HasChange)
                changed.Add(nameof(general.SystemVersion));
            if (general.NumberOfExpectedUsersInterval.HasChange)
                changed.Add(nameof(general.NumberOfExpectedUsersInterval));
            if (general.LifeCycleStatus.HasChange)
                changed.Add(nameof(general.LifeCycleStatus));
            if (general.ValidFrom.HasChange)
                changed.Add(nameof(general.ValidFrom));
            if (general.ValidTo.HasChange)
                changed.Add(nameof(general.ValidTo));
            if (general.MainContractUuid.HasChange)
                changed.Add(nameof(general.MainContractUuid));
            if (general.ContainsAITechnology.HasChange)
                changed.Add(nameof(general.ContainsAITechnology));
            if (general.WebAccessibilityCompliance.HasChange)
                changed.Add(nameof(general.WebAccessibilityCompliance));
            if (general.LastWebAccessibilityCheck.HasChange)
                changed.Add(nameof(general.LastWebAccessibilityCheck));
            if (general.WebAccessibilityNotes.HasChange)
                changed.Add(nameof(general.WebAccessibilityNotes));
            if (general.IsSociallyCritical.HasChange)
                changed.Add(nameof(general.IsSociallyCritical));
            if (general.BusinessCritical.HasChange)
                changed.Add(nameof(general.BusinessCritical));
            if (general.SystemUsageCriticalityLevelUuid.HasChange)
                changed.Add(nameof(general.SystemUsageCriticalityLevelUuid));
            if (general.CriticalityLevelDocumentation.HasChange)
                changed.Add(nameof(general.CriticalityLevelDocumentation));
            if (general.Purpose.HasChange)
                changed.Add(nameof(general.Purpose));
            if (general.TechnicalSystemTypeUuids.HasChange)
                changed.Add(nameof(general.TechnicalSystemTypeUuids));
            if (general.HostedAt.HasChange)
                changed.Add(nameof(general.HostedAt));
            return changed;
        }

        private IEnumerable<string> GetChangedOrganizationalUsage()
        {
            var changed = new List<string>();
            var org = OrganizationalUsage.Value;
            if (org.UsingOrganizationUnitUuids.HasChange)
                changed.Add(nameof(org.UsingOrganizationUnitUuids));
            if (org.ResponsibleOrganizationUnitUuid.HasChange)
                changed.Add(nameof(org.ResponsibleOrganizationUnitUuid));
            return changed;
        }

        private IEnumerable<string> GetChangedKLE()
        {
            var changed = new List<string>();
            var kle = KLE.Value;
            if (kle.AddedKLEUuids.HasChange)
                changed.Add(nameof(kle.AddedKLEUuids));
            if (kle.RemovedKLEUuids.HasChange)
                changed.Add(nameof(kle.RemovedKLEUuids));
            return changed;
        }

        private IEnumerable<string> GetChangedRoles()
        {
            var changed = new List<string>();
            var roles = Roles.Value;
            if (roles.UserRolePairs.HasChange)
                changed.Add(nameof(roles.UserRolePairs));
            return changed;
        }

        private IEnumerable<string> GetChangedGDPR()
        {
            var changed = new List<string>();
            var gdpr = GDPR.Value;
            if (gdpr.ProcessingPurpose.HasChange)
                changed.Add(nameof(gdpr.ProcessingPurpose));
            if (gdpr.DirectoryDocumentation.HasChange)
                changed.Add(nameof(gdpr.DirectoryDocumentation));
            if (gdpr.DataSensitivityLevels.HasChange)
                changed.Add(nameof(gdpr.DataSensitivityLevels));
            if (gdpr.SensitivePersonDataUuids.HasChange)
                changed.Add(nameof(gdpr.SensitivePersonDataUuids));
            if (gdpr.PersonalDataOptions.HasChange)
                changed.Add(nameof(gdpr.PersonalDataOptions));
            if (gdpr.RegisteredDataCategoryUuids.HasChange)
                changed.Add(nameof(gdpr.RegisteredDataCategoryUuids));
            if (gdpr.TechnicalPrecautionsInPlace.HasChange)
                changed.Add(nameof(gdpr.TechnicalPrecautionsInPlace));
            if (gdpr.TechnicalPrecautionsApplied.HasChange)
                changed.Add(nameof(gdpr.TechnicalPrecautionsApplied));
            if (gdpr.TechnicalPrecautionsDocumentation.HasChange)
                changed.Add(nameof(gdpr.TechnicalPrecautionsDocumentation));
            if (gdpr.UserSupervision.HasChange)
                changed.Add(nameof(gdpr.UserSupervision));
            if (gdpr.UserSupervisionDate.HasChange)
                changed.Add(nameof(gdpr.UserSupervisionDate));
            if (gdpr.UserSupervisionDocumentation.HasChange)
                changed.Add(nameof(gdpr.UserSupervisionDocumentation));
            if (gdpr.RiskAssessmentConducted.HasChange)
                changed.Add(nameof(gdpr.RiskAssessmentConducted));
            if (gdpr.RiskAssessmentConductedDate.HasChange)
                changed.Add(nameof(gdpr.RiskAssessmentConductedDate));
            if (gdpr.RiskAssessmentResult.HasChange)
                changed.Add(nameof(gdpr.RiskAssessmentResult));
            if (gdpr.RiskAssessmentDocumentation.HasChange)
                changed.Add(nameof(gdpr.RiskAssessmentDocumentation));
            if (gdpr.PlannedRiskAssessmentDate.HasChange)
                changed.Add(nameof(gdpr.PlannedRiskAssessmentDate));
            if (gdpr.RiskAssessmentNotes.HasChange)
                changed.Add(nameof(gdpr.RiskAssessmentNotes));
            if (gdpr.DPIAConducted.HasChange)
                changed.Add(nameof(gdpr.DPIAConducted));
            if (gdpr.DPIADate.HasChange)
                changed.Add(nameof(gdpr.DPIADate));
            if (gdpr.DPIADocumentation.HasChange)
                changed.Add(nameof(gdpr.DPIADocumentation));
            if (gdpr.RetentionPeriodDefined.HasChange)
                changed.Add(nameof(gdpr.RetentionPeriodDefined));
            if (gdpr.NextDataRetentionEvaluationDate.HasChange)
                changed.Add(nameof(gdpr.NextDataRetentionEvaluationDate));
            if (gdpr.DataRetentionEvaluationFrequencyInMonths.HasChange)
                changed.Add(nameof(gdpr.DataRetentionEvaluationFrequencyInMonths));
            return changed;
        }

        private IEnumerable<string> GetChangedArchiving()
        {
            var changed = new List<string>();
            var arch = Archiving.Value;
            if (arch.ArchiveDuty.HasChange)
                changed.Add(nameof(arch.ArchiveDuty));
            if (arch.ArchiveTypeUuid.HasChange)
                changed.Add(nameof(arch.ArchiveTypeUuid));
            if (arch.ArchiveLocationUuid.HasChange)
                changed.Add(nameof(arch.ArchiveLocationUuid));
            if (arch.ArchiveTestLocationUuid.HasChange)
                changed.Add(nameof(arch.ArchiveTestLocationUuid));
            if (arch.ArchiveSupplierOrganizationUuid.HasChange)
                changed.Add(nameof(arch.ArchiveSupplierOrganizationUuid));
            if (arch.ArchiveActive.HasChange)
                changed.Add(nameof(arch.ArchiveActive));
            if (arch.ArchiveNotes.HasChange)
                changed.Add(nameof(arch.ArchiveNotes));
            if (arch.ArchiveFrequencyInMonths.HasChange)
                changed.Add(nameof(arch.ArchiveFrequencyInMonths));
            if (arch.ArchiveDocumentBearing.HasChange)
                changed.Add(nameof(arch.ArchiveDocumentBearing));
            if (arch.ArchiveJournalPeriods.HasChange)
                changed.Add(nameof(arch.ArchiveJournalPeriods));
            return changed;
        }

    }
}
