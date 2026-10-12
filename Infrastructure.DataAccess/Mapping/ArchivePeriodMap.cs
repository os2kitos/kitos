using Core.DomainModel.ItSystem;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class ArchivePeriodMap : EntityMap<ArchivePeriod>
    {
        public override void Configure(EntityTypeBuilder<ArchivePeriod> builder)
        {
            base.Configure(builder);
            builder.ToTable("ArchivePeriod");

            builder.HasOne(t => t.ItSystemUsage)
                .WithMany(t => t.ArchivePeriods)
                .HasForeignKey(d => d.ItSystemUsageId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.Uuid).IsRequired();
            builder.HasIndex(x => x.Uuid).IsUnique().HasDatabaseName("UX_ArchivePeriod_Uuid");
        }
    }
}
