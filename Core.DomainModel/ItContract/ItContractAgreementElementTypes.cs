namespace Core.DomainModel.ItContract
{
    public class ItContractAgreementElementTypes
    {
        public int ItContract_Id { get; set; }
        public virtual ItContract ItContract { get; set; }

        public int AgreementElementType_Id { get; set; }
        public virtual AgreementElementType AgreementElementType { get; set;}
    }
}
