using Core.DomainModel;
using Core.DomainModel.Events;

namespace Core.DomainServices;

public class RaiseEntityUpdatedOnSnapshotEventsHandler<TEntity, TEntitySnapshot> : IDomainEventHandler<EntityUpdatedEventWithSnapshot<TEntity, TEntitySnapshot>>
    where TEntitySnapshot : ISnapshot<TEntity>
{
    private readonly IDomainEvents _domainEvents;
    public RaiseEntityUpdatedOnSnapshotEventsHandler(IDomainEvents domainEvents)
    {
        _domainEvents = domainEvents;
    }

    public void Handle(EntityUpdatedEventWithSnapshot<TEntity, TEntitySnapshot> domainEvent)
    {
        _domainEvents.Raise(new EntityUpdatedEvent<TEntity>(domainEvent.Entity));
    }
}