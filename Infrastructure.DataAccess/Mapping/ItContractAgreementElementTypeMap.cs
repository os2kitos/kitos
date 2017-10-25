using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItContract;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItContractAgreementElementTypeMap : EntityTypeConfiguration<ItContractAgreementElementTypes>
    {
        public ItContractAgreementElementTypeMap()
        {
            HasKey(x => new
            {
                x.ItContract_Id, x.AgreementElementType_Id
            });
        }
    }
}
