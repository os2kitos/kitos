namespace Core.DomainModel.Events
{
    public class EntityUpdatedEvent<TEntity> : EntityLifeCycleEvent<TEntity>
    {
        public EntityUpdatedEvent(TEntity entity) 
            : base(LifeCycleEventType.Updated, entity)
        {
        }
    }
}
