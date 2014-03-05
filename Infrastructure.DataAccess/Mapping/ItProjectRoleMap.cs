using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItProject;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItProjectRoleMap : EntityTypeConfiguration<ItProjectRole>
    {
        public ItProjectRoleMap()
        {
            this.HasKey(t => t.Id);

            this.ToTable("ItProjectRole");

            this.Property(t => t.Name).IsRequired();
        }
    }
}