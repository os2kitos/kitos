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
            TrackedLifeCycleEventType eventType,
            Guid entityUuid,
            TrackedEntityType entityType,
            Organization.Organization optionalOrganizationReference = null,
            AccessModifier? optionalAccessModifier = null,
            Organization.Organization optionalRightsHolderOrganization = null)
            : base(DateTime.UtcNow, entityUuid, entityType, optionalOrganizationReference, optionalAccessModifier, optionalRightsHolderOrganization)
        {
            EventType = eventType;
        }
    }
}
