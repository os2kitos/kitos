using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage.Read;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemUsageOverviewUsedBySystemUsageReadModelMap : EntityTypeConfiguration<ItSystemUsageOverviewUsedBySystemUsageReadModel>
    {
        public ItSystemUsageOverviewUsedBySystemUsageReadModelMap()
        {
            HasKey(x => x.Id);
            HasRequired(x => x.Parent)
                .WithMany(x => x.IncomingRelatedItSystemUsages)
                .HasForeignKey(x => x.ParentId)
                .WillCascadeOnDelete(true);

            Property(x => x.ItSystemUsageId)
                .IsRequired()
                .HasIndexAnnotation("ItSystemUsageOverviewItSystemUsageReadModel_index_ItSystemUsageId", 0);

            Property(x => x.ItSystemUsageName)
                .IsRequired()
                .HasMaxLength(ItSystem.MaxNameLength)
                .HasIndexAnnotation("ItSystemUsageOverviewItSystemUsageReadModel_index_ItSystemUsageName", 0);

        }
    }
}
