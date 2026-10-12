using System;
using System.Collections.Generic;

namespace Core.ApplicationServices.Model.KitosEvents;

public class SystemTakenIntoUsageEventBodyModel: IEventBody
{
    public Guid SystemUuid { get; set; }
    public Guid OrganizationUuid { get; set; }

    public Dictionary<string, object?> ToKeyValuePairs() => new() { { nameof(SystemUuid), SystemUuid }, { nameof(OrganizationUuid), OrganizationUuid } };
}
