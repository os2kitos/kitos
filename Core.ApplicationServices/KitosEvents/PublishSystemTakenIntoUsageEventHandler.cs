using Core.ApplicationServices.Model.KitosEvents;
using Core.DomainModel.Events;

namespace Core.ApplicationServices.KitosEvents;

public class PublishSystemTakenIntoUsageEventHandler(IKitosEventPublisherService eventPublisher)
    : IDomainEventHandler<SystemTakenIntoUsageEvent>
{
    private const string QueueTopic = KitosQueueTopics.KitosItSystemUsageCreatedEvent;

    public void Handle(SystemTakenIntoUsageEvent domainEvent)
    {
        var eventBody = new SystemTakenIntoUsageEventBodyModel{ SystemUuid = domainEvent.SystemUuid, OrganizationUuid = domainEvent.OrganizationUuid };

        var newEvent = new KitosEvent(eventBody, QueueTopic);
        eventPublisher.PublishEvent(newEvent);
    }
}
