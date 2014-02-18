using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItProject;

namespace Infrastructure.DataAccess.Mapping
{
    public class HierarchyMap : EntityTypeConfiguration<Hierarchy>
    {
        public HierarchyMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Hierarchy");
            this.Property(t => t.Id).HasColumnName("Id");

            // Relationships
            this.HasRequired(t => t.ItProject)
                .WithOptional(t => t.Hierarchy)
                .WillCascadeOnDelete(true);

            this.HasOptional(t => t.ItProjectRef)
                .WithOptionalDependent()
                .WillCascadeOnDelete(false);

            this.HasOptional(t => t.ItProgramRef)
                .WithOptionalDependent()
                .WillCascadeOnDelete(false);
        }
    }
}
