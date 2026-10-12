using Core.DomainModel.ItSystemUsage.Read;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemUsageOverviewSensitiveDataLevelReadModelMap : IEntityTypeConfiguration<ItSystemUsageOverviewSensitiveDataLevelReadModel>
    {
        public void Configure(EntityTypeBuilder<ItSystemUsageOverviewSensitiveDataLevelReadModel> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Parent)
                .WithMany(x => x.SensitiveDataLevels)
                .HasForeignKey(x => x.ParentId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.SensitivityDataLevel).IsRequired();
            builder.HasIndex(x => x.SensitivityDataLevel).HasDatabaseName("ItSystemUsageOverviewSensitiveDataLevelReadModel_Index_SensitiveDataLevel");
        }
    }
}
