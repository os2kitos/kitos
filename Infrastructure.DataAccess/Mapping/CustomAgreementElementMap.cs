using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItContract;

namespace Infrastructure.DataAccess.Mapping
{
    public class CustomAgreementElementMap : EntityTypeConfiguration<CustomAgreementElement>
    {
        public CustomAgreementElementMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Name)
                .IsRequired();

            // Table & Column Mappings
            this.ToTable("AgreementElement");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.Name).HasColumnName("Name");

            // Relationships
            this.HasRequired(t => t.ItContract)
                .WithMany(t => t.CustomAgreementElements)
                .HasForeignKey(d => d.ItContractId)
                .WillCascadeOnDelete(true);
        }
    }
}
