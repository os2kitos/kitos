using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public class OrganizationMap : EntityTypeConfiguration<Organization>
    {
        public OrganizationMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Organization");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.Cvr).IsOptional();
            this.Property(t => t.Type).IsOptional();

            // Relationships
            this.HasOptional(t => t.Config)
                .WithRequired(t => t.Organization)
                .WillCascadeOnDelete(true);

        }
    }
}
