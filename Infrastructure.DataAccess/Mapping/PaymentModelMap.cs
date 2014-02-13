using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItContract;

namespace Infrastructure.DataAccess.Mapping
{
    public class PaymentModelMap : EntityTypeConfiguration<PaymentModel>
    {
        public PaymentModelMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("PaymentModel");
            this.Property(t => t.Id).HasColumnName("Id");
        }
    }
}
