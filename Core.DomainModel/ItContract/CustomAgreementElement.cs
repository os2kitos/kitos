namespace Core.DomainModel.ItContract
{
    public class CustomAgreementElement : IEntity<int>
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int ItContractId { get; set; }
        public virtual ItContract ItContract { get; set; }
    }
}