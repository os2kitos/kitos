using Core.DomainModel.ItContract.Read;
using System.Data.Entity.ModelConfiguration;

namespace Infrastructure.DataAccess.Mapping
{
    internal class
        ItContractOverviewReadModelSystemRelationMap : EntityTypeConfiguration<ItContractOverviewReadModelSystemRelation>
    {
        public ItContractOverviewReadModelSystemRelationMap()
        {
            HasKey(x => x.Id);

            Property(x => x.RelationId)
                .HasIndexAnnotation("IX_RelationId");

            Property(x => x.FromSystemUsageId)
                .HasIndexAnnotation("IX_FromSystemUsageId");

            Property(x => x.ToSystemUsageId)
                .HasIndexAnnotation("IX_ToSystemUsageId");
        }
    }
}