using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItInterfaceMap : EntityMap<ItInterface>
    {
        public ItInterfaceMap()
        {
            // Properties
            this.Property(x => x.Version).HasMaxLength(ItInterface.MaxVersionLength);

            this.Property(x => x.OrganizationId);
            this.Property(x => x.Name)
                .HasMaxLength(ItInterface.MaxNameLength)
                .IsRequired();
            this.Property(x => x.ItInterfaceId)
                .HasMaxLength(ItInterface.MaxNameLength)
                .IsRequired();

            HasIndex(x => new { x.OrganizationId, x.Name, x.ItInterfaceId })
                .IsUnique(true)
                .HasName("UX_NameAndVersionUniqueToOrg");

            HasIndex(x => x.OrganizationId)
                .IsUnique(false)
                .HasName("IX_OrganizationId");

            HasIndex(x => x.Name)
                .IsUnique(false)
                .HasName("IX_Name");

            HasIndex(x => x.Version)
                .IsUnique(false)
                .HasName("IX_Version");

            // Table & Column Mappings
            this.ToTable("ItInterface");

            this.HasOptional(t => t.Interface)
                .WithMany(d => d.References)
                .HasForeignKey(t => t.InterfaceId);

            this.HasRequired(t => t.Organization)
                .WithMany(d => d.ItInterfaces)
                .HasForeignKey(t => t.OrganizationId)
                .WillCascadeOnDelete(false);

            TypeMapping.AddIndexOnAccessModifier<ItInterfaceMap, ItInterface>(this);

            Property(t => t.Uuid)
                .IsRequired()
                .HasUniqueIndexAnnotation("UX_ItInterface_Uuid", 0);
        }
    }
}
