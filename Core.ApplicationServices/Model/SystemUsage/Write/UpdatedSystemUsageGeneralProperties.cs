using System;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Shared;


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
    }
}
