using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class AdminRoleMap : EntityTypeConfiguration<AdminRole>
    {
        public AdminRoleMap()
        {
            this.HasKey(t => t.Id);

            this.ToTable("AdminRole");

            this.Property(t => t.Name).IsRequired();
        }
    }
}