namespace Presentation.Web.Models
{
    public class TaskUsageDTO
    {
        public int Id { get; set; }
        public int TaskRefId { get; set; }
        public int OrgUnitId { get; set; }

        public bool Starred { get; set; }
        public int TechnologyStatus { get; set; }
        public int UsageStatus { get; set; }
        public string Comment { get; set; }

        public bool HasDelegations { get; set; }
    }
}