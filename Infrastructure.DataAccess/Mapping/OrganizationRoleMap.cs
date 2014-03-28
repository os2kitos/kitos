using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class OrganizationRoleMap : EntityTypeConfiguration<OrganizationRole>
    {
        public OrganizationRoleMap()
        {
            this.HasKey(t => t.Id);

            this.ToTable("OrganizationRole");

            this.Property(t => t.Name).IsRequired();
        }
    }
}