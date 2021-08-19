using System;
using System.Collections.Generic;
using Core.ApplicationServices.Model.Shared;
using Core.DomainModel.ItSystem.DataTypes;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.GDPR;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.Model.SystemUsage.Write
{
    public class UpdatedSystemUsageGDPRProperties
    {
        public Maybe<ChangedValue<string>> Purpose { get; set; } = Maybe<ChangedValue<string>>.None;
        public Maybe<ChangedValue<DataOptions?>> BusinessCritical { get; set; } = Maybe<ChangedValue<DataOptions?>>.None;
        public Maybe<ChangedValue<HostedAt?>> HostedAt { get; set; } = Maybe<ChangedValue<HostedAt?>>.None;
        public Maybe<ChangedValue<Maybe<NamedLink>>> DirectoryDocumentation { get; set; } = Maybe<ChangedValue<Maybe<NamedLink>>>.None;
        public Maybe<ChangedValue<Maybe<IEnumerable<SensitiveDataLevel>>>> DataSensitivityLevels { get; set; } = Maybe<ChangedValue<Maybe<IEnumerable<SensitiveDataLevel>>>>.None;
        public Maybe<ChangedValue<Maybe<IEnumerable<Guid>>>> SensitivePersonDataUuids { get; set; } = Maybe<ChangedValue<Maybe<IEnumerable<Guid>>>>.None;
        public Maybe<ChangedValue<Maybe<IEnumerable<Guid>>>> RegisteredDataCategorieUuids { get; set; } = Maybe<ChangedValue<Maybe<IEnumerable<Guid>>>>.None;
        public Maybe<ChangedValue<DataOptions?>> TechnicalPrecautionsInPlace { get; set; } = Maybe<ChangedValue<DataOptions?>>.None;
        public Maybe<ChangedValue<Maybe<IEnumerable<TechnicalPrecaution>>>> TechnicalPrecautionsApplied { get; set; } = Maybe<ChangedValue<Maybe<IEnumerable<TechnicalPrecaution>>>>.None;
        public Maybe<ChangedValue<Maybe<NamedLink>>> TechnicalPrecautionsDocumentation { get; set; } = Maybe<ChangedValue<Maybe<NamedLink>>>.None;
        public Maybe<ChangedValue<DataOptions?>> UserSupervision { get; set; } = Maybe<ChangedValue<DataOptions?>>.None;
        public Maybe<ChangedValue<DateTime?>> UserSupervisionDate { get; set; } = Maybe<ChangedValue<DateTime?>>.None;
        public Maybe<ChangedValue<Maybe<NamedLink>>> UserSupervisionDocumentation { get; set; } = Maybe<ChangedValue<Maybe<NamedLink>>>.None;
        public Maybe<ChangedValue<DataOptions?>> RiskAssessmentConducted { get; set; } = Maybe<ChangedValue<DataOptions?>>.None;
        public Maybe<ChangedValue<DateTime?>> RiskAssessmentConductedDate { get; set; } = Maybe<ChangedValue<DateTime?>>.None;
        public Maybe<ChangedValue<RiskLevel?>> RiskAssessmentResult { get; set; } = Maybe<ChangedValue<RiskLevel?>>.None;
        public Maybe<ChangedValue<Maybe<NamedLink>>> RiskAssessmentDocumentation { get; set; } = Maybe<ChangedValue<Maybe<NamedLink>>>.None;
        public Maybe<ChangedValue<string>> RiskAssessmentNotes { get; set; } = Maybe<ChangedValue<string>>.None;
        public Maybe<ChangedValue<DataOptions?>> DPIAConducted { get; set; } = Maybe<ChangedValue<DataOptions?>>.None;
        public Maybe<ChangedValue<DateTime?>> DPIADate { get; set; } = Maybe<ChangedValue<DateTime?>>.None;
        public Maybe<ChangedValue<Maybe<NamedLink>>> DPIADocumentation { get; set; } = Maybe<ChangedValue<Maybe<NamedLink>>>.None;
        public Maybe<ChangedValue<DataOptions?>> RetentionPeriodDefined { get; set; } = Maybe<ChangedValue<DataOptions?>>.None;
        public Maybe<ChangedValue<DateTime?>> NextDataRetentionEvaluationDate { get; set; } = Maybe<ChangedValue<DateTime?>>.None;
        public Maybe<ChangedValue<int?>> DataRetentionEvaluationFrequencyInMonths { get; set; } = Maybe<ChangedValue<int?>>.None;
    }
}
