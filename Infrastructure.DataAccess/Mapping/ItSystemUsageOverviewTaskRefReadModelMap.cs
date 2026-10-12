using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainModel.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemUsageOverviewTaskRefReadModelMap : IEntityTypeConfiguration<ItSystemUsageOverviewTaskRefReadModel>
    {
        public void Configure(EntityTypeBuilder<ItSystemUsageOverviewTaskRefReadModel> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Parent)
                .WithMany(x => x.ItSystemTaskRefs)
                .HasForeignKey(x => x.ParentId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.KLEId).HasMaxLength(TaskRef.MaxTaskKeyLength);
            builder.HasIndex(x => x.KLEId).HasDatabaseName("ItSystemUsageOverviewTaskRefReadModel_Index_KLEId");

            builder.Property(x => x.KLEName).HasMaxLength(TaskRef.MaxDescriptionLength);
            builder.HasIndex(x => x.KLEName).HasDatabaseName("ItSystemUsageOverviewTaskRefReadModel_Index_KLEName");
        }
    }
}
