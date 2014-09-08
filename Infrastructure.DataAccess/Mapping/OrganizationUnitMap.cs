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
        }
    }
}
