using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItContract;

namespace Infrastructure.DataAccess.Mapping
{
    public class PurchaseFormMap : EntityTypeConfiguration<PurchaseForm>
    {
        public PurchaseFormMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("PurchaseForm");
            this.Property(t => t.Id).HasColumnName("Id");
        }
    }
}
