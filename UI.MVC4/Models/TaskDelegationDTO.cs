using System.Collections.Generic;

namespace UI.MVC4.Models
{
    public class TaskDelegationDTO
    {
        public TaskUsageDTO Usage { get; set; }
        public IEnumerable<TaskDelegationDTO> Delegations { get; set; }
        public bool HasDelegations { get; set; }
    }
}