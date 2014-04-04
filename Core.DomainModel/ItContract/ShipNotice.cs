namespace Core.DomainModel.ItContract
{
    public class ShipNotice
    {
        public int Id { get; set; }
        public string AlarmDate { get; set; }
        public string To { get; set; }
        public string Cc { get; set; }
        public string Subject { get; set; }
        public int ItContractId { get; set; }

        public virtual ItContract ItContract { get; set; }
    }
}
