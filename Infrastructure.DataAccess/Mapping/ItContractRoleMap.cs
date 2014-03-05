using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItContract;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItContractRoleMap : EntityTypeConfiguration<ItContractRole>
    {
        public ItContractRoleMap()
        {
            this.HasKey(t => t.Id);

            this.ToTable("ItContractRole");

            this.Property(t => t.Name).IsRequired();
        }
    }
}