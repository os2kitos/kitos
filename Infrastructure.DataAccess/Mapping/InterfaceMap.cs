using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class InterfaceMap : EntityTypeConfiguration<Interface>
    {
        public InterfaceMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Interface");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.ItSystemId).HasColumnName("ItSystemId");

            // Relationships
            this.HasRequired(t => t.ItSystem)
                .WithMany(t => t.Interfaces)
                .HasForeignKey(d => d.ItSystemId);

            this.HasRequired(t => t.Method)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.MethodId);
        }
    }
}
