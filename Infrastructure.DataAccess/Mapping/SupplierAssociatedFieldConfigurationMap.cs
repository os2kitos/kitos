using Core.DomainModel.SupplierAssociatedFields;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class SupplierAssociatedFieldConfigurationMap : IEntityTypeConfiguration<SupplierAssociatedFieldConfiguration>
    {
        public void Configure(EntityTypeBuilder<SupplierAssociatedFieldConfiguration> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.OrganizationId)
                .IsRequired();

            builder.Property(x => x.FieldKey)
                .IsRequired();

            builder.HasIndex(x => new { x.OrganizationId, x.FieldKey })
                .IsUnique()
                .HasDatabaseName("UX_OrganizationId_SupplierAssociatedFieldConfiguration_FieldKey");

            builder.HasOne(x => x.Organization)
                .WithMany(x => x.SupplierAssociatedFieldConfigurations)
                .HasForeignKey(x => x.OrganizationId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
