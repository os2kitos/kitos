using System;

namespace Core.DomainModel.Tracking
{
    public class LifeCycleTrackingEvent : BaseTrackingEvent
    {
        public TrackedLifeCycleEventType EventType { get; set; }

        /// <summary>
        /// For EF
        /// </summary>
        protected LifeCycleTrackingEvent()
        {
        }

        public LifeCycleTrackingEvent(
            Guid entityUuid,
            TrackedEntityType entityType,
            TrackedLifeCycleEventType eventType)
            : base(DateTime.UtcNow, entityUuid, entityType)
        {
            EventType = eventType;
        }
    }
}
