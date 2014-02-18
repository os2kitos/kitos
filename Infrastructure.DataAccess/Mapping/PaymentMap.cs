using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItContract;

namespace Infrastructure.DataAccess.Mapping
{
    public class PaymentMap : EntityTypeConfiguration<Payment>
    {
        public PaymentMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Payment");
            this.Property(t => t.Id).HasColumnName("Id");

            // Relationships
            this.HasRequired(t => t.ItContract)
                .WithOptional(t => t.Payment);

        }
    }
}
