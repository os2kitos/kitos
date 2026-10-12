using Core.DomainModel.ItContract.Read;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    internal class ItContractOverviewReadModelSystemRelationMap : IEntityTypeConfiguration<ItContractOverviewReadModelSystemRelation>
    {
        public void Configure(EntityTypeBuilder<ItContractOverviewReadModelSystemRelation> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.RelationId).HasDatabaseName("IX_RelationId");
            builder.HasIndex(x => x.FromSystemUsageId).HasDatabaseName("IX_FromSystemUsageId");
            builder.HasIndex(x => x.ToSystemUsageId).HasDatabaseName("IX_ToSystemUsageId");
        }
    }
}
