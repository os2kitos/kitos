using Core.DomainModel.Shared;

namespace Core.DomainModel.Events
{
    public class EntityBeingDeletedEvent<TEntity> : EntityLifeCycleEvent<TEntity>
    {
        public EntityBeingDeletedEvent(TEntity entity)
            : base(LifeCycleEventType.Deleting, entity)
        {
        }
    }
}
