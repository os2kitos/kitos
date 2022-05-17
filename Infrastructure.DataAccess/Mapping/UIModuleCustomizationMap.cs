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
                .IsRequired();
            Property(x => x.OrganizationId)
                .IsRequired();
            
            HasRequired(t => t.Organization)
                .WithMany(t => t.UIModuleCustomizations)
                .HasForeignKey(d => d.OrganizationId)
                .WillCascadeOnDelete(true);
        }
    }
}
