using System.Collections.Generic;

namespace Core.DomainModel
{
    public class TaskUsage : Entity, IHierarchy<TaskUsage>
    {
        public TaskUsage()
        {
            Children = new List<TaskUsage>();
        }

        public int TaskRefId { get; set; }
        public int OrgUnitId { get; set; }

        public virtual TaskRef TaskRef { get; set; }
        public virtual OrganizationUnit OrgUnit { get; set; }

        public int? ParentId { get; set; }
        public virtual TaskUsage Parent { get; set; }
        public virtual ICollection<TaskUsage> Children { get; set; } 

        public bool Starred { get; set; }
        public int TechnologyStatus { get; set; }
        public int UsageStatus { get; set; }
        public string Comment { get; set; }

        public override bool HasUserWriteAccess(User user)
        {
            if (OrgUnit != null && OrgUnit.HasUserWriteAccess(user)) return true;

            return base.HasUserWriteAccess(user);
        }
    }
}
