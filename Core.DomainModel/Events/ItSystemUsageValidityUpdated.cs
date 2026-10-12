namespace Core.DomainModel.Events
{
    public class ItSystemUsageValidityUpdated(ItSystemUsage.ItSystemUsage usage) : EntityUpdatedEvent<ItSystemUsage.ItSystemUsage>(usage)
    {
    }
}
