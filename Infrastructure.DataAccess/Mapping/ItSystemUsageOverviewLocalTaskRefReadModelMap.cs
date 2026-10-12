using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainModel.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemUsageOverviewLocalTaskRefReadModelMap : IEntityTypeConfiguration<ItSystemUsageOverviewLocalTaskRefReadModel>
    {
        public void Configure(EntityTypeBuilder<ItSystemUsageOverviewLocalTaskRefReadModel> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Parent)
                .WithMany(x => x.LocalItSystemTaskRefs)
                .HasForeignKey(x => x.ParentId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.KLEId).HasMaxLength(TaskRef.MaxTaskKeyLength);
            builder.HasIndex(x => x.KLEId).HasDatabaseName("ItSystemUsageOverviewLocalTaskRefReadModel_Index_KLEId");

            builder.Property(x => x.KLEName).HasMaxLength(TaskRef.MaxDescriptionLength);
            builder.HasIndex(x => x.KLEName).HasDatabaseName("ItSystemUsageOverviewLocalTaskRefReadModel_Index_KLEName");
        }
    }
}
