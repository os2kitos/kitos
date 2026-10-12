using Core.DomainModel.UIConfiguration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class UIModuleCustomizationMap : IEntityTypeConfiguration<UIModuleCustomization>
    {
        public void Configure(EntityTypeBuilder<UIModuleCustomization> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Module)
                .HasMaxLength(UIModuleCustomizationConstraints.MaxModuleLength)
                .IsRequired();

            builder.Property(x => x.OrganizationId)
                .IsRequired();

            builder.HasIndex(x => new { x.OrganizationId, x.Module })
                .IsUnique()
                .HasDatabaseName("UX_OrganizationId_UIModuleCustomization_Module");

            builder.HasOne(t => t.Organization)
                .WithMany(t => t.UIModuleCustomizations)
                .HasForeignKey(d => d.OrganizationId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
