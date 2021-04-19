using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystemUsage.Read;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemUsageOverviewSensitiveDataLevelReadModelMap : EntityTypeConfiguration<ItSystemUsageOverviewSensitiveDataLevelReadModel>
    {
        public ItSystemUsageOverviewSensitiveDataLevelReadModelMap()
        {
            HasKey(x => x.Id);
            HasRequired(x => x.Parent)
                .WithMany(x => x.SensitiveDataLevels)
                .HasForeignKey(x => x.ParentId)
                .WillCascadeOnDelete(true);

            Property(x => x.SensitivityDataLevel)
                .IsRequired()
                .HasIndexAnnotation("ItSystemUsageOverviewSensitiveDataLevelReadModel_Index_SensitiveDataLevel", 0);
        }
    }
}
