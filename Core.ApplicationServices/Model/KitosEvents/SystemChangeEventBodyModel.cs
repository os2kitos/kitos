using System;
using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.KitosEvents;

public class SystemChangeEventBodyModel : IEventBody
{
    public Guid SystemUuid { get; set; }
    public OptionalValueChange<string> SystemName { get; set; } = OptionalValueChange<string>.None;
    public OptionalValueChange<Maybe<Guid>> DataProcessorUuid { get; set; } = OptionalValueChange<Maybe<Guid>>.None;
    public OptionalValueChange<string> DataProcessorName { get; set; } = OptionalValueChange<string>.None;
    public Dictionary<string, object> ToKeyValuePairs()
    {
        var keyValuePairs = new Dictionary<string, object> { { nameof(SystemUuid), SystemUuid } };

        if (SystemName.HasChange) keyValuePairs.Add(nameof(SystemName), SystemName.NewValue);
        if (DataProcessorUuid.HasChange) keyValuePairs.Add(nameof(DataProcessorUuid), DataProcessorUuid.NewValue.HasValue ? DataProcessorUuid.NewValue.Value : null);
        if (DataProcessorName.HasChange) keyValuePairs.Add(nameof(DataProcessorName), DataProcessorName.NewValue);

        return keyValuePairs;
    }

}