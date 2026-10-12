using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystem.DataTypes;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Shared;
using System;
using System.Collections.Generic;


namespace Core.ApplicationServices.Model.SystemUsage.Write
{
    public class UpdatedSystemUsageGeneralProperties
    {
        public OptionalValueChange<string> LocalSystemId { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<string> LocalCallName { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<Maybe<Guid>> DataClassificationUuid { get; set; } = OptionalValueChange<Maybe<Guid>>.None;
        public OptionalValueChange<string> Notes { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<string> SystemVersion { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<Maybe<(int lower, int? upperBound)>> NumberOfExpectedUsersInterval { get; set; } = OptionalValueChange<Maybe<(int lower, int? upperBound)>>.None;
        public OptionalValueChange<LifeCycleStatusType?> LifeCycleStatus { get; set; } = OptionalValueChange<LifeCycleStatusType?>.None;
        public OptionalValueChange<Maybe<DateTime>> ValidFrom { get; set; } = OptionalValueChange<Maybe<DateTime>>.None;
        public OptionalValueChange<Maybe<DateTime>> ValidTo { get; set; } = OptionalValueChange<Maybe<DateTime>>.None;
        public OptionalValueChange<Maybe<Guid>> MainContractUuid { get; set; } = OptionalValueChange<Maybe<Guid>>.None;
        public OptionalValueChange<Maybe<YesNoUndecidedOption>> ContainsAITechnology { get; set; } = OptionalValueChange<Maybe<YesNoUndecidedOption>>.None;
        public OptionalValueChange<Maybe<YesNoPartiallyOption>> WebAccessibilityCompliance { get; set; } = OptionalValueChange<Maybe<YesNoPartiallyOption>>.None;
        public OptionalValueChange<Maybe<DateTime>> LastWebAccessibilityCheck { get; set; } = OptionalValueChange<Maybe<DateTime>>.None;
        public OptionalValueChange<string> WebAccessibilityNotes { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<DataOptions?> IsSociallyCritical { get; set; } = OptionalValueChange<DataOptions?>.None;
        public OptionalValueChange<DataOptions?> BusinessCritical { get; set; } = OptionalValueChange<DataOptions?>.None;
        public OptionalValueChange<Maybe<Guid>> SystemUsageCriticalityLevelUuid { get; set; } = OptionalValueChange<Maybe<Guid>>.None;
        public OptionalValueChange<Maybe<NamedLink>> CriticalityLevelDocumentation { get; set; } = OptionalValueChange<Maybe<NamedLink>>.None;
        public OptionalValueChange<string> Purpose { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<Maybe<IEnumerable<Guid>>> TechnicalSystemTypeUuids { get; set; } = OptionalValueChange<Maybe<IEnumerable<Guid>>>.None;
        public OptionalValueChange<HostedAt?> HostedAt { get; set; } = OptionalValueChange<HostedAt?>.None;
        public OptionalValueChange<IEnumerable<LicensingAndCodeModel>> LicensingAndCodeModels { get; set; } = OptionalValueChange<IEnumerable<LicensingAndCodeModel>>.None;
    }
}
