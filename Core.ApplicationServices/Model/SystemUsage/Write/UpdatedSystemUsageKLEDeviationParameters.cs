using System;
using System.Collections.Generic;
using Core.ApplicationServices.Model.Shared;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.Model.SystemUsage.Write
{
    public class UpdatedSystemUsageKLEDeviationParameters
    {
        public OptionalValueChange<Maybe<IEnumerable<Guid>>> AddedKLEUuids { get; set; } = OptionalValueChange<Maybe<IEnumerable<Guid>>>.None;
        public OptionalValueChange<Maybe<IEnumerable<Guid>>> RemovedKLEUuids { get; set; } = OptionalValueChange<Maybe<IEnumerable<Guid>>>.None;

    }
}
