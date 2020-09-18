namespace Infrastructure.Services.DomainEvents
{
    public enum LifeCycleEventType
    {
        Created,
        Updated,
        Deleted
    }

    /// <summary>
    /// base class for entity lifecycle events. Do not use for registrations - use the specific implementations to get better granularity in what is posted to the handler
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public abstract class EntityLifeCycleEvent<TEntity> : IDomainEvent
    {
        public LifeCycleEventType ChangeType { get; }
        public TEntity Entity { get; }

        protected EntityLifeCycleEvent(LifeCycleEventType changeType, TEntity entity)
        {
            ChangeType = changeType;
            Entity = entity;
        }
    }
}
