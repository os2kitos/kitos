using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItContract;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItContractAgreementElementTypeMap : EntityTypeConfiguration<ItContractAgreementElementTypes>
    {
        public ItContractAgreementElementTypeMap()
        {
            this.ToTable("ItContractAgreementElementTypes");
            this.Property(t => t.ItContract_Id).HasColumnName("ItContract_Id");
            this.Property(t => t.AgreementElementType_Id).HasColumnName("AgreementElementType_Id");

            HasKey(x => new
            {
                x.AgreementElementType_Id, x.ItContract_Id
            });
        }
    }
}
