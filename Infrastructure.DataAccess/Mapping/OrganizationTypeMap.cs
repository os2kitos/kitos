using Core.DomainModel;
using System.Data.Entity.ModelConfiguration;

namespace Infrastructure.DataAccess.Mapping
{
    public class OrganizationTypeMap : EntityTypeConfiguration<OrganizationType>
    {
        public OrganizationTypeMap()
        {
            // Properties
            this.Property(x => x.Name).IsRequired();
            this.Property(t => t.Category).IsRequired();

            // Table & Column Mappings
            this.ToTable("OrganizationTypes");
        }
    }
}
