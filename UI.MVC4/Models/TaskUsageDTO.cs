using System.Collections.Generic;

namespace UI.MVC4.Models
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

    public class TaskUsageNestedDTO
    {
        public int Id { get; set; }

        public int TaskRefId { get; set; }
        public string TaskRefTaskKey { get; set; }
        public string TaskRefDescription { get; set; }

        public int OrgUnitId { get; set; }
        public string OrgUnitName { get; set; }

        public bool Starred { get; set; }
        public int TechnologyStatus { get; set; }
        public int UsageStatus { get; set; }
        public string Comment { get; set; }

        public IEnumerable<TaskUsageNestedDTO> Children { get; set; } 
        public bool HasDelegations { get; set; }
    }
}