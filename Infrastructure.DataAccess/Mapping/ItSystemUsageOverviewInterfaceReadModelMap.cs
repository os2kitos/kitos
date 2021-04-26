using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage.Read;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemUsageOverviewInterfaceReadModelMap : EntityTypeConfiguration<ItSystemUsageOverviewInterfaceReadModel>
    {
        public ItSystemUsageOverviewInterfaceReadModelMap()
        {
            HasKey(x => x.Id);
            HasRequired(x => x.Parent)
                .WithMany(x => x.DependsOnInterfaces)
                .HasForeignKey(x => x.ParentId)
                .WillCascadeOnDelete(true);

            Property(x => x.InterfaceId)
                .IsRequired()
                .HasIndexAnnotation("ItSystemUsageOverviewInterfaceReadModel_index_InterfaceId", 0);

            Property(x => x.InterfaceName)
                .IsRequired()
                .HasMaxLength(ItInterface.MaxNameLength)
                .HasIndexAnnotation("ItSystemUsageOverviewInterfaceReadModel_index_InterfaceName", 0);

        }
    }
}
