using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public class OrganizationMap : EntityMap<Organization>
    {
        public OrganizationMap()
        {
            // Properties
            // Table & Column Mappings
            this.ToTable("Organization");
            this.Property(t => t.Cvr).IsOptional();
            this.Property(t => t.Type).IsOptional();

            // Relationships
            this.HasOptional(t => t.Config)
                .WithRequired(t => t.Organization)
                .WillCascadeOnDelete(true);

        }
    }
}
