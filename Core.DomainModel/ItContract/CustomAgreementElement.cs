namespace Core.DomainModel.ItContract
{
    public class CustomAgreementElement : Entity
    {
        public string Name { get; set; }

        public int ItContractId { get; set; }
        public virtual ItContract ItContract { get; set; }
    }
}