using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage.Read;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemUsageOverviewUsingSystemUsageReadModelMap : EntityTypeConfiguration<ItSystemUsageOverviewUsingSystemUsageReadModel>
    {
        public ItSystemUsageOverviewUsingSystemUsageReadModelMap()
        {
            HasKey(x => x.Id);
            HasRequired(x => x.Parent)
                .WithMany(x => x.OutgoingRelatedItSystemUsages)
                .HasForeignKey(x => x.ParentId)
                .WillCascadeOnDelete(true);

            Property(x => x.ItSystemUsageId)
                .IsRequired()
                .HasIndexAnnotation("ItSystemUsageOverviewUsingSystemUsageReadModel_index_ItSystemUsageId", 0);

            Property(x => x.ItSystemUsageName)
                .IsRequired()
                .HasMaxLength(ItSystem.MaxNameLength)
                .HasIndexAnnotation("ItSystemUsageOverviewUsingSystemUsageReadModel_index_ItSystemUsageName", 0);

        }
    }
}
