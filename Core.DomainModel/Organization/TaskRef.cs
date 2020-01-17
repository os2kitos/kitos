using System;
using System.Collections.Generic;

namespace Core.DomainModel.Organization
{
    /// <summary>
    /// Represents a task (such as KLE), which can be associated
    /// with Systems, Projects or Organization Units.
    /// </summary>
    public class TaskRef : Entity, IHierarchy<TaskRef>
    {
        public TaskRef()
        {
            this.Children = new List<TaskRef>();
            this.ItSystems = new List<ItSystem.ItSystem>();
            this.ItSystemUsages = new List<ItSystemUsage.ItSystemUsage>();
            this.ItProjects = new List<ItProject.ItProject>();
        }

        /// <summary>
        /// Global ID
        /// </summary>
        public Guid Uuid { get; set; }

        /// <summary>
        /// Type of task. In KLE, this is used to distinguish between tasks and task groups
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Human readable ID
        /// </summary>
        public string TaskKey { get; set; }

        /// <summary>
        /// Further description
        /// </summary>
        public string Description { get; set; }

        public DateTime? ActiveFrom { get; set; }
        public DateTime? ActiveTo { get; set; }

        public int? ParentId { get; set; }

        public int OwnedByOrganizationUnitId { get; set; }
        /// <summary>
        /// The organization unit the task was created in.
        /// </summary>
        public virtual OrganizationUnit OwnedByOrganizationUnit { get; set; }

        public virtual TaskRef Parent { get; set; }
        public virtual ICollection<TaskRef> Children { get; set; }

        /// <summary>
        /// ItProjects which have been marked with this task
        /// </summary>
        public virtual ICollection<ItProject.ItProject> ItProjects { get; set; }

        /// <summary>
        /// Usages of this task
        /// </summary>
        public virtual ICollection<TaskUsage> Usages { get; set; }

        /// <summary>
        /// ItSystems which have been marked with this task
        /// </summary>
        public virtual ICollection<ItSystem.ItSystem> ItSystems { get; set; }

        /// <summary>
        /// ItSystemUsages which have been marked with this task
        /// </summary>
        public virtual ICollection<ItSystemUsage.ItSystemUsage> ItSystemUsages { get; set; }

        /// <summary>
        /// List of inherited IT System tasks not currently used by an ItSystemUsage
        /// </summary>
        public virtual ICollection<ItSystemUsage.ItSystemUsage> ItSystemUsagesOptOut { get; set; }
    }
}
