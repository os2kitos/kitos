namespace Infrastructure.Services.DomainEvents
{
    public class EntityCreatedEvent<TEntity> : EntityLifeCycleEvent<TEntity>
    {
        public EntityCreatedEvent(TEntity entity) 
            : base(LifeCycleEventType.Created, entity)
        {
        }
    }
}
