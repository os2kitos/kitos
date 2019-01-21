using Core.DomainModel.ItContract;

namespace Infrastructure.DataAccess.Mapping
{
    public class AgreementElementTypeMap : OptionEntityMap<AgreementElementType, ItContract>
    {
        public AgreementElementTypeMap(){

            this.HasMany(t => t.References)
                .WithRequired(t => t.AgreementElementType)
                .HasForeignKey(d => d.AgreementElementType_Id)
                .WillCascadeOnDelete(false);
        }
    }
}
