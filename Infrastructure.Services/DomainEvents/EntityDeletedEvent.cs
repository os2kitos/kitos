namespace Infrastructure.Services.DomainEvents
{
    public class EntityDeletedEvent<TEntity> : EntityLifeCycleEvent<TEntity>
    {
        public EntityDeletedEvent(TEntity entity)
            : base(LifeCycleEventType.Deleted, entity)
        {
        }
    }
}
