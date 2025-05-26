using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemMap : EntityMap<ItSystem>
    {
        public ItSystemMap()
        {
            // Properties

            this.Property(x => x.Name)
                .HasMaxLength(ItSystem.MaxNameLength)
                .IsRequired();

            HasIndex(x => new { x.OrganizationId, x.Name })
                .IsUnique(true)
                .HasName("UX_NameUniqueToOrg");

            HasIndex(x => x.OrganizationId)
                .IsUnique(false)
                .HasName("IX_OrganizationId");

            HasIndex(x => x.Name)
                .IsUnique(false)
                .HasName("IX_Name");

            // Table & Column Mappings
            this.ToTable("ItSystem");

            // Relationships
            this.HasOptional(t => t.Parent)
                .WithMany(d => d.Children)
                .HasForeignKey(t => t.ParentId)
                .WillCascadeOnDelete(false);

            this.HasRequired(t => t.Organization)
                .WithMany(d => d.ItSystems)
                .HasForeignKey(t => t.OrganizationId)
                .WillCascadeOnDelete(false);

            this.HasOptional(t => t.BelongsTo)
                .WithMany(d => d.BelongingSystems)
                .HasForeignKey(t => t.BelongsToId);

            this.HasOptional(t => t.BusinessType)
                .WithMany(t => t.References)
                .HasForeignKey(t => t.BusinessTypeId);

            this.HasMany(t => t.ItInterfaceExhibits)
                .WithRequired(t => t.ItSystem)
                .HasForeignKey(d => d.ItSystemId);

            HasOptional(t => t.Reference);
            HasMany(t => t.ExternalReferences)
                .WithOptional(d => d.ItSystem)
                .HasForeignKey(d => d.ItSystem_Id)
                //No cascading delete in order to avoid causing cycles or multiple cascade paths
                .WillCascadeOnDelete(false);

            Property(x => x.Uuid)
                .IsRequired()
                .HasUniqueIndexAnnotation("UX_System_Uuuid", 0);

            Property(x => x.LegalName)
                .IsOptional()
                .HasMaxLength(ItSystem.MaxNameLength)
                .HasIndexAnnotation("ItSystem_IX_LegalName");

            Property(x => x.LegalDataProcessorName)
                .IsOptional()
                .HasMaxLength(Organization.MaxNameLength)
                .HasIndexAnnotation("ItSystem_IX_LegalDataProcessorName");

            TypeMapping.AddIndexOnAccessModifier<ItSystemMap, ItSystem>(this);
        }
    }
}
