using Core.DomainModel.Archive;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemUsageArchiveMap : EntityMap<ItSystemUsageArchive>
    {
        public override void Configure(EntityTypeBuilder<ItSystemUsageArchive> builder)
        {
            base.Configure(builder);
            builder.ToTable("ItSystemUsageArchive");

            builder.Property(x => x.Uuid).IsRequired();
            builder.HasIndex(x => x.Uuid).IsUnique().HasDatabaseName("UX_ItSystemUsageArchive_Uuid");

            builder.Property(x => x.Note).IsRequired();
            builder.Property(x => x.ArchivingDate).IsRequired();
            builder.Property(x => x.ReferenceName).IsRequired();

            builder.HasOne(x => x.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ArchivedByUser)
                .WithMany()
                .HasForeignKey(x => x.ArchivedById)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Snapshot)
                .WithOne(x => x.ItSystemUsageArchive)
                .HasForeignKey<ItSystemUsageArchiveSnapshot>(x => x.ItSystemUsageArchiveUuid)
                .HasPrincipalKey<ItSystemUsageArchive>(x => x.Uuid)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.ArchiveReferences)
                .WithOne(x => x.ItSystemUsageArchive)
                .HasForeignKey(x => x.ItSystemUsageArchiveUuid)
                .HasPrincipalKey(x => x.Uuid)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.SnapshotUuid)
                .IsUnique()
                .HasDatabaseName("UX_ItSystemUsageArchive_ItSystemUsageSnapshotUuid");
        }
    }
}
