using Core.Abstractions.Types;
using Core.DomainModel.Events;

namespace Core.DomainModel.GDPR.Events
{
    public class DprChangedEvent : EntityUpdatedEvent<DataProcessingRegistration>
    {
        public DprChangedEvent(DataProcessingRegistration entity, Maybe<DprSnapshot> snapshot) : base(entity)
        {
            Snapshot = snapshot;
        }

        public  Maybe<DprSnapshot> Snapshot { get; }
    }
}
