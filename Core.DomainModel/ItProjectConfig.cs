namespace Core.DomainModel
{
    public class ItProjectConfig : IEntity<int>
    {
        public int Id { get; set; }
        public string ItProjectGuide { get; set; }
        public bool ShowItProjectGuide { get; set; }
        public bool ShowPortfolio { get; set; }
        public bool ShowBC { get; set; }

        public virtual Municipality Municipality { get; set; }
    }
}