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

            HasIndex(x => new { x.OptionalOrganizationReferenceId, x.OccurredAtUtc, x.EntityType, x.EventType })
                .HasName("IX_Org_OccurredAt_EntityType_EventType")
                .IsUnique(false);

            HasIndex(x => new { x.OptionalOrganizationReferenceId, x.OptionalAccessModifier, x.OccurredAtUtc, x.EntityType, x.EventType })
                .HasName("IX_Org_AccessModifier_OccurredAt_EntityType_EventType")
                .IsUnique(false);

            HasIndex(x => x.EntityUuid)
                .HasName("IX_EntityUuid")
                .IsUnique(false);
        }
    }
}
