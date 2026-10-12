using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract.Read;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    internal class ItContractOverviewReadModelItSystemUsageMap : IEntityTypeConfiguration<ItContractOverviewReadModelItSystemUsage>
    {
        public void Configure(EntityTypeBuilder<ItContractOverviewReadModelItSystemUsage> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ItSystemUsageName).HasMaxLength(DataProcessingRegistrationConstraints.MaxNameLength);
            builder.HasIndex(x => x.ItSystemUsageName).HasDatabaseName("IX_ItContract_Read_System_Name");

            builder.HasIndex(x => x.ItSystemUsageUuid).HasDatabaseName("IX_ItContract_Read_System_Usage_Uuid");

            builder.Property(x => x.ItSystemUsageSystemUuid).HasMaxLength(50);
            builder.HasIndex(x => x.ItSystemUsageSystemUuid).HasDatabaseName("IX_ItContract_Read_System_Uuid");
        }
    }
}
