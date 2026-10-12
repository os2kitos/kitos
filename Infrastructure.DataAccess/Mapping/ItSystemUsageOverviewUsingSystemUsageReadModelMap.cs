using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage.Read;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemUsageOverviewUsingSystemUsageReadModelMap : IEntityTypeConfiguration<ItSystemUsageOverviewUsingSystemUsageReadModel>
    {
        public void Configure(EntityTypeBuilder<ItSystemUsageOverviewUsingSystemUsageReadModel> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Parent)
                .WithMany(x => x.OutgoingRelatedItSystemUsages)
                .HasForeignKey(x => x.ParentId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.ItSystemUsageId).IsRequired();
            builder.HasIndex(x => x.ItSystemUsageId).HasDatabaseName("ItSystemUsageOverviewUsingSystemUsageReadModel_index_ItSystemUsageId");

            builder.Property(x => x.ItSystemUsageUuid).IsRequired();
            builder.HasIndex(x => x.ItSystemUsageUuid).HasDatabaseName("ItSystemUsageOverviewUsingSystemUsageReadModel_index_ItSystemUsageUuid");

            builder.Property(x => x.ItSystemUsageName).IsRequired().HasMaxLength(ItSystem.MaxNameLength);
            builder.HasIndex(x => x.ItSystemUsageName).HasDatabaseName("ItSystemUsageOverviewUsingSystemUsageReadModel_index_ItSystemUsageName");
        }
    }
}
