using System.Collections.Generic;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;

namespace Core.DomainModel
{
    /// <summary>
    /// Represents a unit or department within an organization (OIO term: "OrgEnhed").
    /// </summary>
    public class OrganizationUnit : HasRightsEntity<OrganizationUnit, OrganizationRight, OrganizationRole>, IHierarchy<OrganizationUnit>
    {
        public OrganizationUnit()
        {
            this.TaskUsages = new List<TaskUsage>();
            this.TaskRefs = new List<TaskRef>();
            this.OwnedTasks = new List<TaskRef>();
            this.DefaultUsers = new List<User>();
            this.Using = new List<ItSystemUsage>();
            this.UsingItProjects = new List<ItProjectOrgUnitUsage>();
        }

        public string Name { get; set; }
        
        /// <summary>
        /// EAN number of the department.
        /// </summary>
        public long? Ean { get; set; }

        public int? ParentId { get; set; }
        /// <summary>
        /// Parent department.
        /// </summary>
        public virtual OrganizationUnit Parent { get; set; }
        public virtual ICollection<OrganizationUnit> Children { get; set; }

        public int OrganizationId { get; set; }
        /// <summary>
        /// The organization which the unit belongs to.
        /// </summary>
        public virtual Organization Organization { get; set; }

        /// <summary>
        /// The usage of task on this Organization Unit. 
        /// Should be a subset of the TaskUsages of the parent department.
        /// </summary>
        public virtual ICollection<TaskUsage> TaskUsages { get; set; }

        /// <summary>
        /// Local tasks that was created in this unit
        /// </summary>
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
        public virtual ICollection<ItProjectOrgUnitUsage> UsingItProjects { get; set; }
        
        /// <summary>
        /// This Organization Unit is responsible for these IT Contracts
        /// </summary>
        public virtual ICollection<ItContract.ItContract> ResponsibleForItContracts { get; set; }

        /// <summary>
        /// The Organization Unit is listed in these economy streams
        /// </summary>
        public virtual ICollection<EconomyStream> EconomyStreams { get; set; }

        public override bool HasUserWriteAccess(User user)
        {
            //if user has write access to the organisation, 
            //user has write access to the org unit
            if (Organization.HasUserWriteAccess(user)) return true;

            //Check rights on this org unit
            if (base.HasUserWriteAccess(user)) return true;

            //Check rights on parent org unit
            if (Parent != null && Parent.HasUserWriteAccess(user)) return true;

            return false;
        }
    }
}
