using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.KitosEvents;
using Core.DomainModel.Events;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystem.DomainEvents;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;

namespace Core.ApplicationServices.KitosEvents;

public class PublishSystemChangesEventHandler : IDomainEventHandler<EntityUpdatedEventWithSnapshot<ItSystem, ItSystemSnapshot>>, IDomainEventHandler<EntityUpdatedEventWithSnapshot<DataProcessingRegistration, DprSnapshot>>
{
    private readonly IKitosEventPublisherService _eventPublisher;
    private const string QueueTopic = KitosQueueTopics.SystemChangedEventTopic;

    public PublishSystemChangesEventHandler(IKitosEventPublisherService eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }
    public void Handle(EntityUpdatedEventWithSnapshot<ItSystem, ItSystemSnapshot> domainEvent)
    {
        var changeEvent = CalculateChangeEventFromSystemModel(domainEvent);
        if (changeEvent.IsNone)
        {
            return;
        }
        var newEvent = new KitosEvent(changeEvent.Value, QueueTopic);
        _eventPublisher.PublishEvent(newEvent);
    }

    public void Handle(EntityUpdatedEventWithSnapshot<DataProcessingRegistration, DprSnapshot> domainEvent)
    {
        var changeEvents = CalculateChangeEventsFromDprModel(domainEvent);
        if (changeEvents.IsNone)
        {
            return;
        }

        foreach (var changeEvent in changeEvents.Value)
        {
            var newEvent = new KitosEvent(changeEvent, QueueTopic);
            _eventPublisher.PublishEvent(newEvent);
        }
    }

    private static Maybe<IEnumerable<SystemChangeEventBodyModel>> CalculateChangeEventsFromDprModel(EntityUpdatedEventWithSnapshot<DataProcessingRegistration, DprSnapshot> domainEvent)
    {
        var snapshotMaybe = domainEvent.Snapshot;
        var dprAfter = domainEvent.Entity;
        if (snapshotMaybe.IsNone)
        {
            return Maybe<IEnumerable<SystemChangeEventBodyModel>>.None;
        }

        var snapshot = snapshotMaybe.Value;
        var dataProcessorUuidsAfter = dprAfter.DataProcessors.Select(x => x.Uuid).ToHashSet();

        if (snapshot.DataProcessorUuids.SetEquals(dataProcessorUuidsAfter))
        {
            return Maybe<IEnumerable<SystemChangeEventBodyModel>>.None;
        }

        return dprAfter.SystemUsages?.Select(usage => GetEventFromUsage(usage, dprAfter)).FromNullable();
    }

    private static SystemChangeEventBodyModel GetEventFromUsage(ItSystemUsage usage, DataProcessingRegistration dpr)
    {
        var dataProcessor = GetDataProcessor(dpr, usage.ItSystem);
        return new SystemChangeEventBodyModel
        {
            SystemUuid = usage.ItSystem.Uuid,
            DataProcessorUuid = dataProcessor.Select(x => x.Uuid).AsChangedValue(),
            DataProcessorName = dataProcessor.Select(x => x.Name).GetValueOrDefault().AsChangedValue()
        };
    }

    private static Maybe<Organization> GetDataProcessor(DataProcessingRegistration dpr, ItSystem system)
    {
        var dataProcessor = dpr.DataProcessors.LastOrDefault().FromNullable();
        return dataProcessor.IsNone ? system.GetRightsHolder() : dataProcessor;
    }

    private static Maybe<SystemChangeEventBodyModel> CalculateChangeEventFromSystemModel(EntityUpdatedEventWithSnapshot<ItSystem, ItSystemSnapshot> changeEvent)
    {
        var snapshotMaybe = changeEvent.Snapshot;
        var systemAfter = changeEvent.Entity;
        if (snapshotMaybe.IsNone)
        {
            return Maybe<SystemChangeEventBodyModel>.None;
        }

        var snapshot = snapshotMaybe.Value;

        if (snapshot.Name.Equals(systemAfter.Name))
        {
            return Maybe<SystemChangeEventBodyModel>.None;
        }

        return new SystemChangeEventBodyModel
        {
            SystemUuid = systemAfter.Uuid,
            SystemName = systemAfter.Name.AsChangedValue()
        };
    }
}