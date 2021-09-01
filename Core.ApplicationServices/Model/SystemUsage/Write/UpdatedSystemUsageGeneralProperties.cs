using System;
using System.Collections.Generic;
using Core.ApplicationServices.Model.Shared;
using Infrastructure.Services.Types;

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
        public OptionalValueChange<Maybe<bool>> EnforceActive { get; set; } = OptionalValueChange<Maybe<bool>>.None;
        public OptionalValueChange<Maybe<DateTime>> ValidFrom { get; set; } = OptionalValueChange<Maybe<DateTime>>.None;
        public OptionalValueChange<Maybe<DateTime>> ValidTo { get; set; } = OptionalValueChange<Maybe<DateTime>>.None;
        public OptionalValueChange<Maybe<Guid>> MainContractUuid { get; set; } = OptionalValueChange<Maybe<Guid>>.None;
        public OptionalValueChange<Maybe<IEnumerable<Guid>>> AssociatedProjectUuids { get; set; } = OptionalValueChange<Maybe<IEnumerable<Guid>>>.None;
    }
}
