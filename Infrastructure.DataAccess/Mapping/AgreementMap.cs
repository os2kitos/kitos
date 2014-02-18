using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItContract;

namespace Infrastructure.DataAccess.Mapping
{
    public class AgreementMap : EntityTypeConfiguration<Agreement>
    {
        public AgreementMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Agreement");
            this.Property(t => t.Id).HasColumnName("Id");

            // Relationships
            this.HasRequired(t => t.ItContract)
                .WithOptional(t => t.Agreement);

        }
    }
}
