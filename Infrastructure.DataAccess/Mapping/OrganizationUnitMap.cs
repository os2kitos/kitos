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

            this.HasMany(t => t.Using)
                .WithRequired(t => t.OrganizationUnit)
                .HasForeignKey(d => d.OrganizationUnitId)
                .WillCascadeOnDelete(false);

            Property(x => x.Uuid)
                .IsRequired()
                .HasUniqueIndexAnnotation("UX_OrganizationUnit_UUID", 0);


            Property(x => x.ExternalOriginUuid)
                .IsOptional()
                //Non-unique index since it's an external origin uuid determined by an external system
                .HasIndexAnnotation("IX_OrganizationUnit_UUID");

            Property(x => x.Origin)
                .IsRequired()
                .HasIndexAnnotation("IX_OrganizationUnit_Origin");
        }
    }
}
