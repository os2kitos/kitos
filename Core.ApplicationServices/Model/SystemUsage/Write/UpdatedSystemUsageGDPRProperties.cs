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
        public OptionalValueChange<string> Purpose { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<DataOptions?> BusinessCritical { get; set; } = OptionalValueChange<DataOptions?>.None;
        public OptionalValueChange<HostedAt?> HostedAt { get; set; } = OptionalValueChange<HostedAt?>.None;
        public OptionalValueChange<Maybe<NamedLink>> DirectoryDocumentation { get; set; } = OptionalValueChange<Maybe<NamedLink>>.None;
        public OptionalValueChange<Maybe<IEnumerable<SensitiveDataLevel>>> DataSensitivityLevels { get; set; } = OptionalValueChange<Maybe<IEnumerable<SensitiveDataLevel>>>.None;
        public OptionalValueChange<Maybe<IEnumerable<Guid>>> SensitivePersonDataUuids { get; set; } = OptionalValueChange<Maybe<IEnumerable<Guid>>>.None;
        public OptionalValueChange<Maybe<IEnumerable<Guid>>> RegisteredDataCategoryUuids { get; set; } = OptionalValueChange<Maybe<IEnumerable<Guid>>>.None;
        public OptionalValueChange<DataOptions?> TechnicalPrecautionsInPlace { get; set; } = OptionalValueChange<DataOptions?>.None;
        public OptionalValueChange<Maybe<IEnumerable<TechnicalPrecaution>>> TechnicalPrecautionsApplied { get; set; } = OptionalValueChange<Maybe<IEnumerable<TechnicalPrecaution>>>.None;
        public OptionalValueChange<Maybe<NamedLink>> TechnicalPrecautionsDocumentation { get; set; } = OptionalValueChange<Maybe<NamedLink>>.None;
        public OptionalValueChange<DataOptions?> UserSupervision { get; set; } = OptionalValueChange<DataOptions?>.None;
        public OptionalValueChange<DateTime?> UserSupervisionDate { get; set; } = OptionalValueChange<DateTime?>.None;
        public OptionalValueChange<Maybe<NamedLink>> UserSupervisionDocumentation { get; set; } = OptionalValueChange<Maybe<NamedLink>>.None;
        public OptionalValueChange<DataOptions?> RiskAssessmentConducted { get; set; } = OptionalValueChange<DataOptions?>.None;
        public OptionalValueChange<DateTime?> RiskAssessmentConductedDate { get; set; } = OptionalValueChange<DateTime?>.None;
        public OptionalValueChange<RiskLevel?> RiskAssessmentResult { get; set; } = OptionalValueChange<RiskLevel?>.None;
        public OptionalValueChange<Maybe<NamedLink>> RiskAssessmentDocumentation { get; set; } = OptionalValueChange<Maybe<NamedLink>>.None;
        public OptionalValueChange<string> RiskAssessmentNotes { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<DataOptions?> DPIAConducted { get; set; } = OptionalValueChange<DataOptions?>.None;
        public OptionalValueChange<DateTime?> DPIADate { get; set; } = OptionalValueChange<DateTime?>.None;
        public OptionalValueChange<Maybe<NamedLink>> DPIADocumentation { get; set; } = OptionalValueChange<Maybe<NamedLink>>.None;
        public OptionalValueChange<DataOptions?> RetentionPeriodDefined { get; set; } = OptionalValueChange<DataOptions?>.None;
        public OptionalValueChange<DateTime?> NextDataRetentionEvaluationDate { get; set; } = OptionalValueChange<DateTime?>.None;
        public OptionalValueChange<int?> DataRetentionEvaluationFrequencyInMonths { get; set; } = OptionalValueChange<int?>.None;
    }
}
