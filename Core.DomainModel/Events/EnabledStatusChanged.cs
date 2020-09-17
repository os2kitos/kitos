using Infrastructure.Services.DomainEvents;

namespace Core.DomainModel.Events
{
    public class EnabledStatusChanged<TEntity> : EntityUpdatedEvent<TEntity> where TEntity: IEntityWithEnabledStatus
    {
        public bool FromDisabled { get; }
        public bool ToDisabled { get; }

        public EnabledStatusChanged(TEntity entity, bool fromDisabled, bool toDisabled) 
            : base(entity)
        {
            FromDisabled = fromDisabled;
            ToDisabled = toDisabled;
        }
    }
}
