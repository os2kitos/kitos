using Core.DomainModel.ItContract.Read;
using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.GDPR;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItContractOverviewReadModelDataProcessingAgreementMap : EntityTypeConfiguration<ItContractOverviewReadModelDataProcessingAgreement>
    {
        public ItContractOverviewReadModelDataProcessingAgreementMap()
        {
            HasKey(x => x.Id);
            Property(x => x.DataProcessingRegistrationName)
                .HasMaxLength(DataProcessingRegistrationConstraints.MaxNameLength)
                .HasIndexAnnotation("IX_ItContract_Read_Dpr_Name");
        }
    }
}
