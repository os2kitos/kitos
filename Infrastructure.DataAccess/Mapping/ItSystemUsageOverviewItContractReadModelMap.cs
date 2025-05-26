using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage.Read;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemUsageOverviewItContractReadModelMap : EntityTypeConfiguration<ItSystemUsageOverviewItContractReadModel>
    {
        public ItSystemUsageOverviewItContractReadModelMap()
        {
            HasKey(x => x.Id);
            HasRequired(x => x.Parent)
                .WithMany(x => x.AssociatedContracts)
                .HasForeignKey(x => x.ParentId)
                .WillCascadeOnDelete(true);

            Property(x => x.ItContractId)
                .IsRequired()
                .HasIndexAnnotation("ItContractId", 0);

            Property(x => x.ItContractUuid).IsRequired();

            Property(x => x.ItContractName)
                .IsRequired()
                .HasMaxLength(ItContractConstraints.MaxNameLength)
                .HasIndexAnnotation("ItContractNameName", 0);
        }
    }
}
