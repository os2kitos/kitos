using Core.DomainModel.ItSystemUsage.Read;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemUsageOverviewArchivePeriodReadModelMap : IEntityTypeConfiguration<ItSystemUsageOverviewArchivePeriodReadModel>
    {
        public void Configure(EntityTypeBuilder<ItSystemUsageOverviewArchivePeriodReadModel> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Parent)
                .WithMany(x => x.ArchivePeriods)
                .HasForeignKey(x => x.ParentId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.StartDate).IsRequired();
            builder.HasIndex(x => x.StartDate).HasDatabaseName("ItSystemUsageOverviewArchivePeriodReadModel_index_StartDate");

            builder.Property(x => x.EndDate).IsRequired();
            builder.HasIndex(x => x.EndDate).HasDatabaseName("ItSystemUsageOverviewArchivePeriodReadModel_index_EndDate");
        }
    }
}
