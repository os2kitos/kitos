using Core.DomainModel.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class OrganizationSupplierMap : IEntityTypeConfiguration<OrganizationSupplier>
    {
        public void Configure(EntityTypeBuilder<OrganizationSupplier> builder)
        {
            builder.HasKey(x => new { x.SupplierId, x.OrganizationId });

            builder.HasOne(x => x.Supplier)
                .WithMany(x => x.UsedAsSupplierByOrganizations)
                .HasForeignKey(x => x.SupplierId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Organization)
                .WithMany(x => x.Suppliers)
                .HasForeignKey(x => x.OrganizationId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
