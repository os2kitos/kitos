using System;
using System.Collections.Generic;
using Core.ApplicationServices.Model.Shared;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.Model.SystemUsage.Write
{
    public class UpdatedSystemUsageKLEDeviationParameters
    {
        public Maybe<ChangedValue<Maybe<IEnumerable<Guid>>>> AddedKLEUuids { get; set; } = Maybe<ChangedValue<Maybe<IEnumerable<Guid>>>>.None;
        public Maybe<ChangedValue<Maybe<IEnumerable<Guid>>>> RemovedKLEUuids { get; set; } = Maybe<ChangedValue<Maybe<IEnumerable<Guid>>>>.None;

    }
}
