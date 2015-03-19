using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public class OrganizationUnitMap : EntityMap<OrganizationUnit>
    {
        public OrganizationUnitMap()
        {
            // Properties
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

            this.Property(t => t.Ean)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new[] { new IndexAttribute("IX_Ean") { IsUnique = true } }));
        }
    }
}
