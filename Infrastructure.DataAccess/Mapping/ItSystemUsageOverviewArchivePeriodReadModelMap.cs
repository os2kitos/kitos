using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystemUsage.Read;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemUsageOverviewArchivePeriodReadModelMap : EntityTypeConfiguration<ItSystemUsageOverviewArchivePeriodReadModel>
    {
        public ItSystemUsageOverviewArchivePeriodReadModelMap()
        {
            HasKey(x => x.Id);
            HasRequired(x => x.Parent)
                .WithMany(x => x.ArchivePeriods)
                .HasForeignKey(x => x.ParentId)
                .WillCascadeOnDelete(true);

            Property(x => x.StartDate)
                .IsRequired()
                .HasIndexAnnotation("ItSystemUsageOverviewArchivePeriodReadModel_index_StartDate", 0);

            Property(x => x.EndDate)
                .IsRequired()
                .HasIndexAnnotation("ItSystemUsageOverviewArchivePeriodReadModel_index_EndDate", 0);
        }
    }
}
