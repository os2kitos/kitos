using Core.DomainModel.ItContract;
using Core.DomainModel.ItContract.Read;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItContractSupplierOverviewAtCriticalityContractReadModelMap : IEntityTypeConfiguration<ItContractSupplierOverviewAtCriticalityContractReadModel>
    {
        public void Configure(EntityTypeBuilder<ItContractSupplierOverviewAtCriticalityContractReadModel> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.ParentId).HasDatabaseName("IX_ItContract_Supplier_Read_Contracts_ParentId");
            builder.HasIndex(x => x.ContractId).HasDatabaseName("IX_ItContract_Supplier_Read_Contracts_ContractId");
            builder.HasIndex(x => x.ContractUuid).HasDatabaseName("IX_ItContract_Supplier_Read_Contracts_ContractUuid");

            builder.Property(x => x.ContractName).HasMaxLength(ItContractConstraints.MaxNameLength);
            builder.HasIndex(x => x.ContractName).HasDatabaseName("IX_ItContract_Supplier_Read_Contracts_ContractName");
        }
    }
}
