using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItContract;
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

            Property(x => x.Name)
                .HasMaxLength(ItContractConstraints.MaxNameLength)
                .HasIndexAnnotation("IX_Contract_Name", 0);
        }
    }
}
