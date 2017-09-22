using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.Organization;

namespace Infrastructure.DataAccess.Mapping
{
    public class OrganizationTypeMap : EntityTypeConfiguration<OrganizationType>
    {
        public OrganizationTypeMap()
        {
            // Properties
            Property(x => x.Name).IsRequired();
            Property(t => t.Category).IsRequired();

            // Table & Column Mappings
            ToTable("OrganizationTypes");
        }
    }
}