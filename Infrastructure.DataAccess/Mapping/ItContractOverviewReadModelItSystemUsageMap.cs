using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract.Read;
using System.Data.Entity.ModelConfiguration;

namespace Infrastructure.DataAccess.Mapping
{
    internal class ItContractOverviewReadModelItSystemUsageMap : EntityTypeConfiguration<ItContractOverviewReadModelItSystemUsage>
    {
        public ItContractOverviewReadModelItSystemUsageMap()
        {
            HasKey(x => x.Id);
            Property(x => x.ItSystemUsageName)
                .HasMaxLength(DataProcessingRegistrationConstraints.MaxNameLength)
                .HasIndexAnnotation("IX_ItContract_Read_System_Name");
            
            Property(x => x.ItSystemUsageSystemUuid)
                .HasMaxLength(50)
                .HasIndexAnnotation("IX_ItContract_Read_System_Uuid");
        }
    }
}
