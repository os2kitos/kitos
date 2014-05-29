using System.Collections.Generic;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;

namespace Core.DomainModel
{
    public class OrganizationUnit : Entity, IHasRights<OrganizationRight>
    {
        public OrganizationUnit()
        {
            this.Rights = new List<OrganizationRight>();
            this.TaskUsages = new List<TaskUsage>();
            this.TaskRefs = new List<TaskRef>();
            this.OwnedTasks = new List<TaskRef>();
            this.DefaultUsers = new List<User>();
            this.Using = new List<ItSystemUsage>();
            this.UsingItProjects = new List<ItProject.ItProject>();
        }

        public string Name { get; set; }
        public int? Ean { get; set; }

        public int? ParentId { get; set; }
        public virtual OrganizationUnit Parent { get; set; }
        public virtual ICollection<OrganizationUnit> Children { get; set; }

        public int OrganizationId { get; set; }
        public virtual Organization Organization { get; set; }

        public virtual ICollection<OrganizationRight> Rights { get; set; }
        public virtual ICollection<TaskUsage> TaskUsages { get; set; }
        public virtual ICollection<TaskRef> TaskRefs { get; set; }
        public virtual ICollection<TaskRef> OwnedTasks { get; set; }
        public virtual ICollection<ItSystemUsage> DelegatedSystemUsages { get; set; }
        public virtual ICollection<ItSystemUsage> ItSystemUsages { get; set; }

        /// <summary>
        /// Users which have set this as their default OrganizationUnit
        /// </summary>
        public virtual ICollection<User> DefaultUsers { get; set; }

        /// <summary>
        /// This Organization Unit is using these IT Systems (Via ItSystemUsage)
        /// </summary>
        public virtual ICollection<ItSystemUsage> Using { get; set; }

        /// <summary>
        /// This Organization Unit is using these IT projects
        /// </summary>
        public virtual ICollection<ItProject.ItProject> UsingItProjects { get; set; }

        /// <summary>
        /// This Organization Unit is responsible for these IT Projects
        /// </summary>
        public virtual ICollection<ItProject.ItProject> ResponsibleForItProjects { get; set; }

        /// <summary>
        /// This Organization Unit is responsible for these IT Contracts
        /// </summary>
        public virtual ICollection<ItContract.ItContract> ResponsibleForItContracts { get; set; }

        /// <summary>
        /// The Organization Unit is listed in these economy streams
        /// </summary>
        public virtual ICollection<EconomyStream> EconomyStreams { get; set; }
    }
}
