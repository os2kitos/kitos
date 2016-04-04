using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public class OrganizationUnitMap : EntityMap<OrganizationUnit>
    {
        public OrganizationUnitMap()
        {
            // Properties
            this.Property(x => x.OrganizationId).HasUniqueIndexAnnotation("UniqueLocalId", 0);
            this.Property(x => x.LocalId).HasMaxLength(100).HasUniqueIndexAnnotation("UniqueLocalId", 1);

            // Table & Column Mappings
            this.ToTable("OrganizationUnit");

            // Relationships
            this.HasOptional(o => o.Parent)
                .WithMany(p => p.Children)
                .HasForeignKey(o => o.ParentId)
                .WillCascadeOnDelete(true);

            this.HasRequired(o => o.Organization)
                .WithMany(m => m.OrgUnits)
                .HasForeignKey(o => o.OrganizationId)
                .WillCascadeOnDelete(true);

            this.HasMany(t => t.UsingItProjects)
                .WithRequired(t => t.OrganizationUnit)
                .HasForeignKey(d => d.OrganizationUnitId);

            this.HasMany(t => t.Using)
                .WithRequired(t => t.OrganizationUnit)
                .HasForeignKey(d => d.OrganizationUnitId);
        }
    }
}
