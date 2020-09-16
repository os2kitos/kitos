namespace Infrastructure.Services.DomainEvents
{
    public class EntityUpdatedEvent<TEntity> : EntityLifeCycleEvent<TEntity>
    {
        public EntityUpdatedEvent(TEntity entity) 
            : base(LifeCycleEventType.Updated, entity)
        {
        }
    }
}
