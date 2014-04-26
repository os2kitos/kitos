using System.Data.Entity.ModelConfiguration;

namespace Infrastructure.DataAccess.Mapping
{
    public class InfrastructureMap : EntityTypeConfiguration<Core.DomainModel.ItSystem.Infrastructure>
    {
        public InfrastructureMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Infrastructure");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.HostId).HasColumnName("HostId");
            this.Property(t => t.SupplierId).HasColumnName("SupplierId");
            this.Property(t => t.DepartmentId).HasColumnName("DepartmentId");

            // Relationships
            this.HasRequired(t => t.OrganizationUnit)
                .WithMany(t => t.Infrastructures)
                .HasForeignKey(d => d.DepartmentId);
            this.HasRequired(t => t.Supplier)
                .WithMany(t => t.Infrastructures)
                .HasForeignKey(d => d.SupplierId);

        }
    }
}
