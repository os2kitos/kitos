using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItContract.Read;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrganizationEntity = Core.DomainModel.Organization.Organization;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItContractSupplierOverviewReadModelMap : IEntityTypeConfiguration<ItContractSupplierOverviewReadModel>
    {
        public void Configure(EntityTypeBuilder<ItContractSupplierOverviewReadModel> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.OrganizationId).HasDatabaseName("IX_ItContract_Supplier_Read_OrganizationId");
            builder.HasIndex(x => x.SupplierId).HasDatabaseName("IX_ItContract_Supplier_Read_SupplierId");
            builder.HasIndex(x => new { x.OrganizationId, x.SupplierId }).IsUnique().HasDatabaseName("UX_ItContract_Supplier_Read_Org_Supplier");
            builder.HasIndex(x => x.SupplierUuid).HasDatabaseName("IX_ItContract_Supplier_Read_SupplierUuid");

            builder.Property(x => x.SupplierName).HasMaxLength(OrganizationEntity.MaxNameLength);
            builder.HasIndex(x => x.SupplierName).HasDatabaseName("IX_ItContract_Supplier_Read_SupplierName");
            builder.Property(x => x.SupplierCvr).HasMaxLength(10);
            builder.HasIndex(x => x.SupplierCvr).HasDatabaseName("IX_ItContract_Supplier_Read_SupplierCvr");
            builder.Property(x => x.SupplierForeignCvr);

            builder.HasIndex(x => x.IsSupplierDisabled).HasDatabaseName("IX_ItContract_Supplier_Read_IsSupplierDisabled");
            builder.HasIndex(x => x.SupplierType).HasDatabaseName("IX_ItContract_Supplier_Read_SupplierType");
            builder.HasIndex(x => x.HighestCriticalityUuid).HasDatabaseName("IX_ItContract_Supplier_Read_HighestCriticalityUuid");

            builder.Property(x => x.HighestCriticalityName).HasMaxLength(OptionEntity<ItContract>.MaxNameLength);
            builder.HasIndex(x => x.HighestCriticalityName).HasDatabaseName("IX_ItContract_Supplier_Read_HighestCriticalityName");

            builder.HasIndex(x => x.HighestCriticalityRank).HasDatabaseName("IX_ItContract_Supplier_Read_HighestCriticalityRank");

            builder.Property(x => x.ContractsAtHighestCriticalityCsv);

            builder.HasMany(x => x.ContractsAtHighestCriticality)
                .WithOne(x => x.Parent)
                .HasForeignKey(x => x.ParentId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
