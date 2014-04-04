using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class ComponentsMap : EntityTypeConfiguration<Component>
    {
        public ComponentsMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Component");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.ItSystemId).HasColumnName("ItSystemId");

            // Relationships
            this.HasRequired(t => t.ItSystem)
                .WithMany(t => t.Components)
                .HasForeignKey(d => d.ItSystemId);

        }
    }
}
