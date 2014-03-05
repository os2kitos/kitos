namespace Core.DomainModel
{
    public class ItContractConfig : IEntity<int>
    {
        public int Id { get; set; }
        public string ItContractGuide { get; set; }
        public bool ShowItContractGuide { get; set; }

        public virtual Municipality Municipality { get; set; }
    }
}