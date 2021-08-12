using System;
using System.Collections.Generic;
using Core.ApplicationServices.Model.Shared;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.Model.SystemUsage.Write
{
    public class UpdatedSystemUsageGeneralProperties
    {
        public Maybe<ChangedValue<string>> LocalSystemId { get; set; } = Maybe<ChangedValue<string>>.None;
        public Maybe<ChangedValue<string>> LocalCallName { get; set; } = Maybe<ChangedValue<string>>.None;
        public Maybe<ChangedValue<Maybe<Guid>>> DataClassificationUuid { get; set; } = Maybe<ChangedValue<Maybe<Guid>>>.None;
        public Maybe<ChangedValue<string>> Notes { get; set; } = Maybe<ChangedValue<string>>.None;
        public Maybe<ChangedValue<string>> SystemVersion { get; set; } = Maybe<ChangedValue<string>>.None;
        public Maybe<ChangedValue<Maybe<(int lower, int? upperBound)>>> NumberOfExpectedUsersInterval { get; set; } = Maybe<ChangedValue<Maybe<(int lower, int? upperBound)>>>.None;
        public Maybe<ChangedValue<Maybe<bool>>> EnforceActive { get; set; } = Maybe<ChangedValue<Maybe<bool>>>.None;
        public Maybe<ChangedValue<Maybe<DateTime>>> ValidFrom { get; set; } = Maybe<ChangedValue<Maybe<DateTime>>>.None;
        public Maybe<ChangedValue<Maybe<DateTime>>> ValidTo { get; set; } = Maybe<ChangedValue<Maybe<DateTime>>>.None;
        public Maybe<ChangedValue<Maybe<Guid>>> MainContractUuid { get; set; } = Maybe<ChangedValue<Maybe<Guid>>>.None;
        public Maybe<ChangedValue<Maybe<IEnumerable<Guid>>>> AssociatedProjectUuids { get; set; } = Maybe<ChangedValue<Maybe<IEnumerable<Guid>>>>.None;
    }
}
