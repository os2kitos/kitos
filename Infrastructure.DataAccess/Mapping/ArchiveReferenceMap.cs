using Core.DomainModel.Archive;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class ArchiveReferenceMap : EntityMap<ArchiveReference>
    {
        public override void Configure(EntityTypeBuilder<ArchiveReference> builder)
        {
            base.Configure(builder);
            builder.ToTable("ArchiveReference");

            builder.Property(x => x.Uuid).IsRequired();
            builder.HasIndex(x => x.Uuid).IsUnique().HasDatabaseName("UX_ArchiveReference_Uuid");

            builder.Property(x => x.Label).IsRequired();
            builder.Property(x => x.Url).IsRequired();

            builder.HasOne(x => x.ItSystemUsageArchive)
                .WithMany(x => x.ArchiveReferences)
                .HasForeignKey(x => x.ItSystemUsageArchiveUuid)
                .HasPrincipalKey(x => x.Uuid)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
