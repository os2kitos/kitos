namespace Core.DomainModel
{
    public class ItSystemConfig : IEntity<int>
    {
        public int Id { get; set; }
        public string ItSystemGuide { get; set; }
        public bool ShowItSystemGuide { get; set; }

        public virtual Municipality Municipality { get; set; }
    }
}