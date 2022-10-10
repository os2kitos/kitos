using Core.DomainModel.Shared;

namespace Core.DomainModel.Events
{
    public class EntityBeingRemovedEvent<TEntity> : EntityLifeCycleEvent<TEntity>
    {
        public int OrganizationId { get; }

        public EntityBeingRemovedEvent(TEntity entity, int organizationId) : base(LifeCycleEventType.Deleting, entity)
        {
            OrganizationId = organizationId;
        }
    }
}
