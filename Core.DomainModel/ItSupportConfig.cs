namespace Core.DomainModel
{
    public class ItSupportConfig : IEntity<int>
    {
        public int Id { get; set; }
        public string ItSupportGuide { get; set; }
        public bool ShowItSupportGuide { get; set; }
        public bool ShowTabOverview { get; set; }
        public bool ShowColumnTechnology { get; set; }
        public bool ShowColumnUsage { get; set; }
        public bool ShowColumnMandatory { get; set; }

        public virtual Municipality Municipality { get; set; }
    }
}