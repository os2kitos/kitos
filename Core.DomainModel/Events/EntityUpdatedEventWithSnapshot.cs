using Core.Abstractions.Types;

namespace Core.DomainModel.Events;

public class EntityUpdatedEventWithSnapshot<TEntity, TEntitySnapshot> : EntityUpdatedEvent<TEntity>
where TEntitySnapshot : ISnapshot<TEntity>
{
    public EntityUpdatedEventWithSnapshot(TEntity entity, Maybe<TEntitySnapshot> snapshot) : base(entity)
    {
        Snapshot = snapshot;
    }

    public Maybe<TEntitySnapshot> Snapshot { get; set; }
}