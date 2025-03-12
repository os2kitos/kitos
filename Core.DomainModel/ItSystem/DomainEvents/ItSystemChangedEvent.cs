using Core.Abstractions.Types;
using Core.DomainModel.Events;

namespace Core.DomainModel.ItSystem.DomainEvents
{
    public class ItSystemChangedEvent : EntityUpdatedEvent<ItSystem>
    {
        public ItSystemChangedEvent(ItSystem entity, Maybe<ItSystemSnapshot> snapshot) : base(entity)
        {
            Snapshot = snapshot;
        }

        public Maybe<ItSystemSnapshot> Snapshot { get; }
    }
}
