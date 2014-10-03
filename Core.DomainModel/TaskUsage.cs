using System.Collections.Generic;

namespace Core.DomainModel
{
    /// <summary>
    /// Represents that a<see><cref>TaskRef</cref></see> has been marked as important for an
    /// <see><cref>OrganizationUnit</cref></see>.  
    /// Helper object which can hold comments and status property
    /// </summary>
    /// TODO this really should be a composite key entity
    /// TODO why is this a hierarchy?
    public class TaskUsage : Entity, IHierarchy<TaskUsage>
    {
        public TaskUsage()
        {
            Children = new List<TaskUsage>();
        }

        public int TaskRefId { get; set; }
        /// <summary>
        /// The task in use
        /// </summary>
        public virtual TaskRef TaskRef { get; set; }

        public int OrgUnitId { get; set; }
        /// <summary>
        /// The organization unit which uses the task
        /// </summary>
        public virtual OrganizationUnit OrgUnit { get; set; }

        public int? ParentId { get; set; }
        /// <summary>
        /// If the parent <see cref="OrganizationUnit"/> of <see cref="OrgUnit"/> also has marked the <see cref="TaskRef"/>,
        /// the parent usage is accesible from here.
        /// </summary>
        public virtual TaskUsage Parent { get; set; }

        /// <summary>
        /// Child usages (see <see cref="Parent"/>)
        /// </summary>
        public virtual ICollection<TaskUsage> Children { get; set; } 

        /// <summary>
        /// Whether the TaskUsage can be found on the overview
        /// </summary>
        public bool Starred { get; set; }

        public TrafficLight TechnologyStatus { get; set; }
        public TrafficLight UsageStatus { get; set; }

        public string Comment { get; set; }

        public override bool HasUserWriteAccess(User user)
        {
            if (OrgUnit != null && OrgUnit.HasUserWriteAccess(user)) return true;

            return base.HasUserWriteAccess(user);
        }
    }
}
