using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.DomainModel
{
    public class TaskUsage : IEntity<int>
    {
        public int Id { get; set; }
        public int TaskRefId { get; set; }
        public int OrgUnitId { get; set; }

        public virtual TaskRef TaskRef { get; set; }
        public virtual OrganizationUnit OrgUnit { get; set; }

        public bool Starred { get; set; }
        public int TechnologyStatus { get; set; }
        public int UsageStatus { get; set; }
        public string Comment { get; set; }
    }
}
