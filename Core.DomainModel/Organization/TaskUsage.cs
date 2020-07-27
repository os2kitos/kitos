using System.Collections.Generic;

namespace Core.DomainModel.Organization
{
    /// <summary>
    /// Represents that a<see><cref>TaskRef</cref></see> has been marked as important for an
    /// <see><cref>OrganizationUnit</cref></see>.
    /// Helper object which can hold comments and status property
    /// </summary>
    public class TaskUsage : Entity, IHierarchy<TaskUsage>, IContextAware, IOrganizationModule
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

        /// <summary>
        /// Determines whether a user has write access to this instance.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>
        ///   <c>true</c> if user has write access, otherwise <c>false</c>.
        /// </returns>
        public override bool HasUserWriteAccess(User user)
        {
            if (OrgUnit != null && OrgUnit.HasUserWriteAccess(user))
                return true;

            return base.HasUserWriteAccess(user);
        }

        /// <summary>
        /// Determines whether this instance is within a given organizational context.
        /// </summary>
        /// <param name="organizationId">The organization identifier (context) the user is accessing from.</param>
        /// <returns>
        ///   <c>true</c> if this instance is in the organizational context, otherwise <c>false</c>.
        /// </returns>
        public bool IsInContext(int organizationId)
        {
            return OrgUnit.IsInContext(organizationId);
        }
    }
}
