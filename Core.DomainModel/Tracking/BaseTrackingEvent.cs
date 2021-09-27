using System;

namespace Core.DomainModel.Tracking
{
    public abstract class BaseTrackingEvent
    {
        public int Id { get; set; }

        protected BaseTrackingEvent()
        {
            OccurredAtUtc = DateTime.UtcNow;
        }

        protected BaseTrackingEvent(
            DateTime occurredAtUtc, 
            Guid entityUuid, 
            TrackedEntityType entityType, 
            Organization.Organization optionalOrganizationReference, 
            AccessModifier? optionalAccessModifier, 
            Organization.Organization optionalRightsHolderOrganization)
        {
            OccurredAtUtc = occurredAtUtc;
            EntityUuid = entityUuid;
            EntityType = entityType;
            OptionalOrganizationReference = optionalOrganizationReference;
            OptionalAccessModifier = optionalAccessModifier;
            OptionalRightsHolderOrganization = optionalRightsHolderOrganization;
        }

        public DateTime OccurredAtUtc { get; set; }
        public Guid EntityUuid { get; set; }
        public TrackedEntityType EntityType { get; set; }
        /// <summary>
        /// If available, a reference to the organization which the tracked entity belongs to
        /// </summary>
        public virtual Organization.Organization OptionalOrganizationReference { get; set; }
        public int? OptionalOrganizationReferenceId { get; set; }
        /// <summary>
        /// If available, an access modifier of the tracked entity (determines if the tracking event of an organization-bound entity is eligible for sharing)
        /// </summary>
        public AccessModifier? OptionalAccessModifier { get; set; }

        public int? OptionalRightsHolderOrganizationId { get; set; }
        /// <summary>
        /// If available, an access modifier of the tracked entity (determines if the tracking event of an organization-bound entity is eligible for sharing)
        /// </summary>
        public virtual Organization.Organization OptionalRightsHolderOrganization { get; set; }

        public int? UserId { get; set; }
        /// <summary>
        /// User responsible for the event
        /// </summary>
        public virtual User User { get; set; }
    }
}
