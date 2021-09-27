using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.Tracking;

namespace Infrastructure.DataAccess.Mapping
{
    public class LifeCycleTrackingEventMap : EntityTypeConfiguration<LifeCycleTrackingEvent>
    {
        public LifeCycleTrackingEventMap()
        {
            HasKey(x => x.Id);
            HasOptional(x => x.OptionalOrganizationReference)
                .WithMany(x => x.LifeCycleTrackingEvents)
                .HasForeignKey(x => x.OptionalOrganizationReferenceId);

            HasOptional(x => x.OptionalRightsHolderOrganization)
                .WithMany(x => x.LifeCycleTrackingEventsWhereOrganizationIsRightsHolder)
                .HasForeignKey(x => x.OptionalRightsHolderOrganizationId);

            //FK indexes for fast joins (are not created automatically once manual indexes are added where the ids are part of the columns)
            HasIndex(x => x.OptionalOrganizationReferenceId)
                .IsUnique(false);

            HasIndex(x => x.OptionalRightsHolderOrganizationId)
                .IsUnique(false);

            //Indexes to match expected query patterns
            HasIndex(x => new { x.EventType, x.OccurredAtUtc, x.EntityType })
                .HasName("IX_EventType_OccurredAt_EntityType_EventType")
                .IsUnique(false);

            HasIndex(x => new { x.OptionalOrganizationReferenceId, x.EventType, x.OccurredAtUtc, x.EntityType })
                .HasName("IX_Org_EventType_OccurredAt_EntityType")
                .IsUnique(false);

            HasIndex(x => new { x.OptionalRightsHolderOrganizationId, x.OptionalOrganizationReferenceId, x.EventType, x.OccurredAtUtc, x.EntityType })
                .HasName("IX_RightsHolder_Org_EventType_OccurredAt_EntityType")
                .IsUnique(false);

            HasIndex(x => new { x.OptionalRightsHolderOrganizationId, x.EventType, x.OccurredAtUtc, x.EntityType })
                .HasName("IX_RightsHolder_EventType_OccurredAt_EntityType")
                .IsUnique(false);

            HasIndex(x => new { x.OptionalOrganizationReferenceId, x.OptionalAccessModifier, x.EventType, x.OccurredAtUtc, x.EntityType })
                .HasName("IX_Org_AccessModifier_EventType_OccurredAt_EntityType")
                .IsUnique(false);

            HasIndex(x => x.EntityUuid)
                .IsUnique(false);

            HasOptional(x => x.User)
                .WithMany(x => x.LifeCycleTrackingEvents)
                .HasForeignKey(x => x.UserId);
        }
    }
}
