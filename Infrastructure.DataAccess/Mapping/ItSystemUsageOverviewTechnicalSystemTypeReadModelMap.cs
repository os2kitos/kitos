using Core.DomainModel.ItSystemUsage.Read;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemUsageOverviewTechnicalSystemTypeReadModelMap : IEntityTypeConfiguration<ItSystemUsageOverviewTechnicalSystemTypeReadModel>
    {
        public void Configure(EntityTypeBuilder<ItSystemUsageOverviewTechnicalSystemTypeReadModel> builder)
        {
            builder.ToTable("ItSystemUsageOverviewTechnicalSystemTypeReadModel");
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Parent)
                .WithMany(x => x.TechnicalSystemTypes)
                .HasForeignKey(x => x.ParentId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.TechnicalSystemTypeUuid).IsRequired();
            builder.Property(x => x.TechnicalSystemTypeName).HasMaxLength(150);
            builder.HasIndex(x => x.TechnicalSystemTypeUuid).HasDatabaseName("ItSystemUsageOverviewTechnicalSystemTypeReadModel_Index_Uuid");
        }
    }
}
