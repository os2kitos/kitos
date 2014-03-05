using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class DepartmentRoleMap : EntityTypeConfiguration<DepartmentRole>
    {
        public DepartmentRoleMap()
        {
            this.HasKey(t => t.Id);

            this.ToTable("DepartmentRole");

            this.Property(t => t.Name).IsRequired();
        }
    }
}