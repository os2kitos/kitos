using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemMap : EntityMap<ItSystem>
    {
        public ItSystemMap()
        {
            // Properties
            this.Property(x => x.Name).IsRequired()
                .HasUniqueIndexAnnotation("IX_NamePerOrg", 0);
            this.Property(x => x.OrganizationId)
                .HasUniqueIndexAnnotation("IX_NamePerOrg", 1);

            // Table & Column Mappings
            this.ToTable("ItSystem");

            // Relationships
            this.HasOptional(t => t.Parent)
                .WithMany(d => d.Children)
                .HasForeignKey(t => t.ParentId)
                .WillCascadeOnDelete(false);

            this.HasRequired(t => t.Organization)
                .WithMany(d => d.ItSystems)
                .HasForeignKey(t => t.OrganizationId);

            this.HasOptional(t => t.BelongsTo)
                .WithMany(d => d.BelongingSystems)
                .HasForeignKey(t => t.BelongsToId);

            this.HasOptional(t => t.BusinessType)
                .WithMany(t => t.References)
                .HasForeignKey(t => t.BusinessTypeId);
            
            this.HasMany(t => t.CanUseInterfaces)
                .WithRequired(t => t.ItSystem)
                .HasForeignKey(d => d.ItSystemId);

            this.HasMany(t => t.ItInterfaceExhibits)
                .WithRequired(t => t.ItSystem)
                .HasForeignKey(d => d.ItSystemId);
        }
    }
}
