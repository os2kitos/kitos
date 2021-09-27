using Core.DomainModel.Shared;

namespace Core.DomainModel.Events
{
    public class EntityDeletedEvent<TEntity> : EntityLifeCycleEvent<TEntity>
    {
        public EntityDeletedEvent(TEntity entity)
            : base(LifeCycleEventType.Deleted, entity)
        {
        }
    }
}
