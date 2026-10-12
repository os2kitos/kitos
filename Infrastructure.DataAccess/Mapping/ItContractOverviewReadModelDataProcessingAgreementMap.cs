using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract.Read;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItContractOverviewReadModelDataProcessingAgreementMap : IEntityTypeConfiguration<ItContractOverviewReadModelDataProcessingAgreement>
    {
        public void Configure(EntityTypeBuilder<ItContractOverviewReadModelDataProcessingAgreement> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.DataProcessingRegistrationName).HasMaxLength(DataProcessingRegistrationConstraints.MaxNameLength);
            builder.HasIndex(x => x.DataProcessingRegistrationName).HasDatabaseName("IX_ItContract_Read_Dpr_Name");

            builder.HasIndex(x => x.DataProcessingRegistrationUuid).HasDatabaseName("IX_ItContract_Read_Dpr_Uuid");
        }
    }
}
