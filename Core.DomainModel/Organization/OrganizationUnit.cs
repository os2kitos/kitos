using System.Collections.Generic;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystemUsage;

namespace Core.DomainModel
{
    /// <summary>
    /// Represents a unit or department within an organization (OIO term: "OrgEnhed").
    /// </summary>
    public class OrganizationUnit : HasRightsEntity<OrganizationUnit, OrganizationUnitRight, OrganizationUnitRole>, IHierarchy<OrganizationUnit>, IContextAware, IOrganizationModule
    {
        public OrganizationUnit()
        {
            this.TaskUsages = new List<TaskUsage>();
            this.TaskRefs = new List<TaskRef>();
            this.OwnedTasks = new List<TaskRef>();
            this.DefaultUsers = new List<OrganizationRight>();
            this.Using = new List<ItSystemUsageOrgUnitUsage>();
            this.UsingItProjects = new List<ItProjectOrgUnitUsage>();
        }

        public string LocalId { get; set; }

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
        /// <summary>
        /// Gets or sets the delegated system usages.
        /// TODO write better summary
        /// </summary>
        /// <value>
        /// The delegated system usages.
        /// </value>
        public virtual ICollection<ItSystemUsage.ItSystemUsage> DelegatedSystemUsages { get; set; }

        /// <summary>
        /// Gets or sets it system usages.
        /// TODO write better summary
        /// </summary>
        /// <value>
        /// It system usages.
        /// </value>
        public virtual ICollection<ItSystemUsage.ItSystemUsage> ItSystemUsages { get; set; } // TODO is this used anymore isn't Using used instead?

        /// <summary>
        /// Users which have set this as their default OrganizationUnit.
        /// </summary>
        /// <remarks>
        /// Goes through <seealso cref="OrganizationRight"/>.
        /// So to access the user you must call .User on the rights object.
        /// </remarks>
        public virtual ICollection<OrganizationRight> DefaultUsers { get; set; }

        /// <summary>
        /// This Organization Unit is using these IT Systems (Via ItSystemUsage)
        /// </summary>
        public virtual ICollection<ItSystemUsageOrgUnitUsage> Using { get; set; }

        /// <summary>
        /// This Organization Unit is using these IT projects
        /// </summary>
        public virtual ICollection<ItProjectOrgUnitUsage> UsingItProjects { get; set; }

        /// <summary>
        /// This Organization Unit is responsible for these IT ItContracts
        /// </summary>
        public virtual ICollection<ItContract.ItContract> ResponsibleForItContracts { get; set; }

        /// <summary>
        /// The Organization Unit is listed in these economy streams
        /// </summary>
        public virtual ICollection<EconomyStream> EconomyStreams { get; set; }

        /// <summary>
        /// Determines whether a user has write access to this instance.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>
        ///   <c>true</c> if user has write access, otherwise <c>false</c>.
        /// </returns>
        public override bool HasUserWriteAccess(User user)
        {
            // check rights on parent org unit
            if (Parent != null && Parent.HasUserWriteAccess(user))
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
            return OrganizationId == organizationId;
        }
    }
}
