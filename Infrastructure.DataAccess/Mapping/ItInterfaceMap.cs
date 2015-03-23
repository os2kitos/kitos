using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItInterfaceMap : EntityMap<ItInterface>
    {
        public ItInterfaceMap()
        {
            // Properties

            // BUG there's an issue with indexing http://stackoverflow.com/questions/26055140/ef-migrations-drops-index-when-adding-compsite-index
            this.Property(x => x.OrganizationId)
                .HasUniqueIndexAnnotation("IX_NamePerOrg", 0);
            this.Property(x => x.Name)
                .HasMaxLength(100) // http://stackoverflow.com/questions/1827063/mysql-error-key-specification-without-a-key-length
                .IsRequired()
                .HasUniqueIndexAnnotation("IX_NamePerOrg", 1);
            
            // Table & Column Mappings
            this.ToTable("ItInterface");

            // Relationships
            this.HasMany(t => t.CanBeUsedBy)
                .WithRequired(t => t.ItInterface)
                .HasForeignKey(d => d.ItInterfaceId);

            this.HasOptional(t => t.Interface)
                .WithMany(d => d.References)
                .HasForeignKey(t => t.InterfaceId);

            this.HasOptional(t => t.InterfaceType)
                .WithMany(d => d.References)
                .HasForeignKey(t => t.InterfaceTypeId);

            this.HasOptional(t => t.Method)
                .WithMany(d => d.References)
                .HasForeignKey(t => t.MethodId);

            this.HasOptional(t => t.Version)
                .WithMany(d => d.References)
                .HasForeignKey(t => t.VersionId);
        }
    }
}
