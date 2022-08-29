using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItContract.Read;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItContractOverviewReadModelMap : EntityTypeConfiguration<ItContractOverviewReadModel>
    {
        public ItContractOverviewReadModelMap()
        {
            HasRequired(t => t.Organization)
                .WithMany(t => t.ItContractOverviewReadModels)
                .HasForeignKey(d => d.OrganizationId)
                .WillCascadeOnDelete(false);

            HasRequired(x => x.SourceEntity)
                .WithMany(x => x.OverviewReadModels)
                .HasForeignKey(x => x.SourceEntityId)
                .WillCascadeOnDelete(true);
        }
    }
}
