namespace Infrastructure.Services.DomainEvents
{
    public enum LifeCycleEventType
    {
        Created,
        Updated,
        Deleted
    }
    public class EntityLifeCycleEvent<TEntity> : IDomainEvent
    {
        public LifeCycleEventType ChangeType { get; }
        public TEntity Entity { get; }

        public EntityLifeCycleEvent(LifeCycleEventType changeType, TEntity entity)
        {
            ChangeType = changeType;
            Entity = entity;
        }
    }
}
