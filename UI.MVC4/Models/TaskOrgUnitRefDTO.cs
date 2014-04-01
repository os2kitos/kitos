using System.Collections.Generic;

namespace UI.MVC4.Models
{
    public class TaskOrgUnitRefDTO
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string TaskKey { get; set; }
        public string Description { get; set; }
        public IEnumerable<int> HandledByOrgUnit { get; set; }
    }
}