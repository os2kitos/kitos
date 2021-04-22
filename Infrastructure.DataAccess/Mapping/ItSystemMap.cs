using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemMap : EntityMap<ItSystem>
    {
        public ItSystemMap()
        {
            // Properties

            // BUG there's an issue with indexing http://stackoverflow.com/questions/26055140/ef-migrations-drops-index-when-adding-compsite-index
            this.Property(x => x.OrganizationId)
                .HasUniqueIndexAnnotation("UX_NamePerOrg", 0);
            this.Property(x => x.Name)
                .HasMaxLength(ItSystem.MaxNameLength) // http://stackoverflow.com/questions/1827063/mysql-error-key-specification-without-a-key-length
                .IsRequired()
                .HasUniqueIndexAnnotation("UX_NamePerOrg", 1);

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

            TypeMapping.AddIndexOnAccessModifier<ItSystemMap, ItSystem>(this);
        }
    }
}
