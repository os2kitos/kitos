using Core.DomainModel;
using Core.DomainModel.Organization;

namespace Infrastructure.DataAccess.Mapping
{
    public class OrganizationUnitMap : EntityMap<OrganizationUnit>
    {
        public OrganizationUnitMap()
        {
            // Properties
            this.Property(x => x.OrganizationId).HasUniqueIndexAnnotation("UX_LocalId", 0);
            this.Property(x => x.LocalId).HasMaxLength(100).HasUniqueIndexAnnotation("UX_LocalId", 1);

            // Table & Column Mappings
            this.ToTable("OrganizationUnit");

            // Relationships
            this.HasOptional(o => o.Parent)
                .WithMany(p => p.Children)
                .HasForeignKey(o => o.ParentId)
                .WillCascadeOnDelete(false);

            this.HasRequired(o => o.Organization)
                .WithMany(m => m.OrgUnits)
                .HasForeignKey(o => o.OrganizationId)
                .WillCascadeOnDelete(true);

            this.HasMany(t => t.UsingItProjects)
                .WithRequired(t => t.OrganizationUnit)
                .HasForeignKey(d => d.OrganizationUnitId)
                .WillCascadeOnDelete(false);

            this.HasMany(t => t.Using)
                .WithRequired(t => t.OrganizationUnit)
                .HasForeignKey(d => d.OrganizationUnitId)
                .WillCascadeOnDelete(false);

            HasMany(o => o.UsingItProjects)
                .WithRequired(o => o.OrganizationUnit)
                .WillCascadeOnDelete(true);
        }
    }
}
