namespace Core.DomainModel.Events
{
    public class EntityCreatedEvent<TEntity> : EntityLifeCycleEvent<TEntity>
    {
        public EntityCreatedEvent(TEntity entity) 
            : base(LifeCycleEventType.Created, entity)
        {
        }
    }
}
