using System;
using Core.ApplicationServices.Model.Shared;
using Core.DomainModel;

namespace Core.ApplicationServices.Model.System
{
    public class SystemUpdateParameters : SharedSystemUpdateParameters
    {
        public OptionalValueChange<(OptionalValueChange<ArchiveDutyRecommendationTypes?> recommendation,OptionalValueChange<string> comment)> ArchivingRecommendation { get; set; } = OptionalValueChange<(OptionalValueChange<ArchiveDutyRecommendationTypes?> recommendation, OptionalValueChange<string> comment)>.None;
        public OptionalValueChange<Guid?> RightsHolderUuid { get; set; } = OptionalValueChange<Guid?>.None;
        public OptionalValueChange<AccessModifier> Scope { get; set; } = OptionalValueChange<AccessModifier>.None;
        public OptionalValueChange<bool> Deactivated { get; set; } = OptionalValueChange<bool>.None;
    }
}
