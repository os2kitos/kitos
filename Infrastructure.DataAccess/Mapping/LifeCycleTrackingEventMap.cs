using Core.DomainModel.Tracking;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class LifeCycleTrackingEventMap : IEntityTypeConfiguration<LifeCycleTrackingEvent>
    {
        public void Configure(EntityTypeBuilder<LifeCycleTrackingEvent> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.OptionalOrganizationReference)
                .WithMany(x => x.LifeCycleTrackingEvents)
                .HasForeignKey(x => x.OptionalOrganizationReferenceId);

            builder.HasOne(x => x.OptionalRightsHolderOrganization)
                .WithMany(x => x.LifeCycleTrackingEventsWhereOrganizationIsRightsHolder)
                .HasForeignKey(x => x.OptionalRightsHolderOrganizationId);

            builder.HasIndex(x => x.OptionalOrganizationReferenceId);
            builder.HasIndex(x => x.OptionalRightsHolderOrganizationId);

            builder.HasIndex(x => new { x.EventType, x.OccurredAtUtc, x.EntityType })
                .HasDatabaseName("IX_EventType_OccurredAt_EntityType_EventType");

            builder.HasIndex(x => new { x.OptionalOrganizationReferenceId, x.EventType, x.OccurredAtUtc, x.EntityType })
                .HasDatabaseName("IX_Org_EventType_OccurredAt_EntityType");

            builder.HasIndex(x => new { x.OptionalRightsHolderOrganizationId, x.OptionalOrganizationReferenceId, x.EventType, x.OccurredAtUtc, x.EntityType })
                .HasDatabaseName("IX_RightsHolder_Org_EventType_OccurredAt_EntityType");

            builder.HasIndex(x => new { x.OptionalRightsHolderOrganizationId, x.EventType, x.OccurredAtUtc, x.EntityType })
                .HasDatabaseName("IX_RightsHolder_EventType_OccurredAt_EntityType");

            builder.HasIndex(x => new { x.OptionalOrganizationReferenceId, x.OptionalAccessModifier, x.EventType, x.OccurredAtUtc, x.EntityType })
                .HasDatabaseName("IX_Org_AccessModifier_EventType_OccurredAt_EntityType");

            builder.HasIndex(x => x.EntityUuid);

            builder.HasOne(x => x.User)
                .WithMany(x => x.LifeCycleTrackingEvents)
                .HasForeignKey(x => x.UserId);
        }
    }
}
