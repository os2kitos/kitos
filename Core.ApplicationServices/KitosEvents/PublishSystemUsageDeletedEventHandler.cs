using Core.ApplicationServices.Model.KitosEvents;
using Core.DomainModel.Events;

namespace Core.ApplicationServices.KitosEvents;

public class PublishSystemUsageDeletedEventHandler(IKitosEventPublisherService eventPublisher)
    : IDomainEventHandler<SystemTakenOutOfUsageEvent>
{
    public void Handle(SystemTakenOutOfUsageEvent domainEvent)
    {
        var body = new SystemUsageDeletedEventBodyModel
        {
            SystemUuid = domainEvent.SystemUuid,
            OrganizationUuid = domainEvent.OrganizationUuid
        };
        eventPublisher.PublishEvent(new KitosEvent(body, KitosQueueTopics.KitosItSystemUsageDeletedEvent));
    }
}
