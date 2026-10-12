using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage.Read;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemUsageOverviewUsedBySystemUsageReadModelMap : IEntityTypeConfiguration<ItSystemUsageOverviewUsedBySystemUsageReadModel>
    {
        public void Configure(EntityTypeBuilder<ItSystemUsageOverviewUsedBySystemUsageReadModel> builder)
        {
            builder.ToTable("ItSystemUsageOverviewUsedBySystemUsageReadModels");

            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Parent)
                .WithMany(x => x.IncomingRelatedItSystemUsages)
                .HasForeignKey(x => x.ParentId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.ItSystemUsageId).IsRequired();
            builder.HasIndex(x => x.ItSystemUsageId).HasDatabaseName("ItSystemUsageOverviewItSystemUsageReadModel_index_ItSystemUsageId");

            builder.Property(x => x.ItSystemUsageUuid).IsRequired();
            builder.HasIndex(x => x.ItSystemUsageUuid).HasDatabaseName("ItSystemUsageOverviewItSystemUsageReadModel_index_ItSystemUsageUuid");

            builder.Property(x => x.ItSystemUsageName).IsRequired().HasMaxLength(ItSystem.MaxNameLength);
            builder.HasIndex(x => x.ItSystemUsageName).HasDatabaseName("ItSystemUsageOverviewItSystemUsageReadModel_index_ItSystemUsageName");
        }
    }
}
