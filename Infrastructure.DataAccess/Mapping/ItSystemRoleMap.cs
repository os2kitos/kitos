using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemRoleMap : EntityTypeConfiguration<ItSystemRole>
    {
        public ItSystemRoleMap()
        {
            this.HasKey(t => t.Id);

            this.ToTable("ItSystemRole");

            this.Property(t => t.Name).IsRequired();
        }
    }
}