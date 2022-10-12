using Core.DomainModel.Shared;

namespace Core.DomainModel.Events
{
    public class UserBeingRemovedFromOrganizationEvent<TEntity> : EntityLifeCycleEvent<TEntity>
    {
        public int OrganizationId { get; }

        public UserBeingRemovedFromOrganizationEvent(TEntity entity, int organizationId) : base(LifeCycleEventType.Deleting, entity)
        {
            OrganizationId = organizationId;
        }
    }
}
