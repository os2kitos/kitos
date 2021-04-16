using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainModel.Organization;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemUsageOverviewTaskRefReadModelMap : EntityTypeConfiguration<ItSystemUsageOverviewTaskRefReadModel>
    {
        public ItSystemUsageOverviewTaskRefReadModelMap()
        {
            HasKey(x => x.Id);
            HasRequired(x => x.Parent)
                .WithMany(x => x.ItSystemTaskRefs)
                .HasForeignKey(x => x.ParentId)
                .WillCascadeOnDelete(true);

            Property(x => x.KLEId)
                .HasMaxLength(TaskRef.MaxTaskKeyLength)
                .HasIndexAnnotation("ItSystemUsageOverviewTaskRefReadModel_Index_KLEId", 0);

            Property(x => x.KLEName)
                .HasMaxLength(TaskRef.MaxDescriptionLength)
                .HasIndexAnnotation("ItSystemUsageOverviewTaskRefReadModel_Index_KLEName", 0);
        }
    }
}
