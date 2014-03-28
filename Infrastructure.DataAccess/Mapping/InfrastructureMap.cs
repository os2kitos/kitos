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
            this.Property(t => t.Host_Id).HasColumnName("Host_Id");
            this.Property(t => t.Supplier_Id).HasColumnName("Supplier_Id");
            this.Property(t => t.Department_Id).HasColumnName("Department_Id");

            // Relationships
            this.HasRequired(t => t.OrganizationUnit)
                .WithMany(t => t.Infrastructures)
                .HasForeignKey(d => d.Department_Id);
            this.HasRequired(t => t.Host)
                .WithMany(t => t.Infrastructures)
                .HasForeignKey(d => d.Host_Id);
            this.HasRequired(t => t.ItSystem)
                .WithOptional(t => t.Infrastructure);
            this.HasRequired(t => t.Supplier)
                .WithMany(t => t.Infrastructures)
                .HasForeignKey(d => d.Supplier_Id);

        }
    }
}
