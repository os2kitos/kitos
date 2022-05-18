using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.UIConfiguration;

namespace Infrastructure.DataAccess.Mapping
{
    public class UIModuleCustomizationMap : EntityTypeConfiguration<UIModuleCustomization>
    {
        public UIModuleCustomizationMap()
        {
            HasKey(x => x.Id);
            
            Property(x => x.Module)
                .HasMaxLength(UIModuleCustomizationConstraints.MaxModuleLength)
                .IsRequired();

            Property(x => x.OrganizationId)
                .IsRequired();

            HasIndex(x => new { x.OrganizationId, x.Module })
                .IsUnique(true)
                .HasName("UX_OrganizationId_UIModuleCustomization_Module");

            HasRequired(t => t.Organization)
                .WithMany(t => t.UIModuleCustomizations)
                .HasForeignKey(d => d.OrganizationId)
                .WillCascadeOnDelete(true);
        }
    }
}
