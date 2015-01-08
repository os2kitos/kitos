using System.Collections.Generic;

namespace Presentation.Web.Models
{
    public class TaskDelegationDTO
    {
        public TaskUsageDTO Usage { get; set; }
        public IEnumerable<TaskDelegationDTO> Delegations { get; set; }
        public bool HasDelegations { get; set; }
    }
}
