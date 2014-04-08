using System.Collections.Generic;

namespace UI.MVC4.Models
{
    public class TaskDelegationDTO
    {
        public TaskUsageDTO ParentUsage { get; set; }
        public IEnumerable<TaskUsageDTO> ChildrenUsage { get; set; } 
    }
}