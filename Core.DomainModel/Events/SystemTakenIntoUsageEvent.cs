using System;

namespace Core.DomainModel.Events;

public class SystemTakenIntoUsageEvent(ItSystemUsage.ItSystemUsage itSystemUsage, Guid systemUuid, Guid organizationUuid) : IDomainEvent
{
    public ItSystemUsage.ItSystemUsage ItSystemUsage { get; set; } = itSystemUsage;
    public Guid SystemUuid { get; set; } = systemUuid;
    public Guid OrganizationUuid { get; set; } = organizationUuid;
}
