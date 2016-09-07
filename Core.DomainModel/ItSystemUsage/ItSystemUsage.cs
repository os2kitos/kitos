using System.Collections.Generic;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;

namespace Core.DomainModel.ItSystemUsage
{
    /// <summary>
    /// Represents an organisation's usage of an it system.
    /// </summary>
    public class ItSystemUsage : HasRightsEntity<ItSystemUsage, ItSystemRight, ItSystemRole>, IContextAware, ISystemModule, IHasOrganization
    {
        public ItSystemUsage()
        {
            this.Contracts = new List<ItContractItSystemUsage>();
            this.Wishes = new List<Wish>();
            this.OrgUnits = new List<OrganizationUnit>();
            this.TaskRefs = new List<TaskRef>();
            this.ItInterfaceUsages = new List<ItInterfaceUsage>();
            this.ItInterfaceExhibitUsages = new List<ItInterfaceExhibitUsage>();
            this.UsedBy = new List<ItSystemUsageOrgUnitUsage>();
            this.ItProjects = new List<ItProject.ItProject>();
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance's status is active.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance's status is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsStatusActive { get; set; }
        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>
        /// The note.
        /// </value>
        public string Note { get; set; }
        /// <summary>
        /// Gets or sets the user defined local system identifier.
        /// </summary>
        /// <remarks>
        /// This identifier is not the primary key.
        /// </remarks>
        /// <value>
        /// The local system identifier.
        /// </value>
        public string LocalSystemId { get; set; }
        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public string Version { get; set; }
        /// <summary>
        /// Gets or sets a reference to relevant documents in an extern ESDH system.
        /// </summary>
        /// <value>
        /// Extern reference  to ESDH system.
        /// </value>
        public string EsdhRef { get; set; }
        /// <summary>
        /// Gets or sets a reference to relevant documents in an extern CMDB system.
        /// </summary>
        /// <value>
        /// Extern reference  to CMDB system.
        /// </value>
        public string CmdbRef { get; set; }
        /// <summary>
        /// Gets or sets a path or url to relevant documents.
        /// </summary>
        /// <value>
        /// Path or url relevant documents.
        /// </value>
        public string DirectoryOrUrlRef { get; set; }
        /// <summary>
        /// Gets or sets the local call system.
        /// </summary>
        /// <value>
        /// The local call system.
        /// </value>
        public string LocalCallName { get; set; }
        /// <summary>
        /// Organization Unit responsible for this system usage.
        /// </summary>
        /// <value>
        /// The responsible organization unit.
        /// </value>
        public virtual ItSystemUsageOrgUnitUsage ResponsibleUsage { get; set; }

        public int OrganizationId { get; set; }
        /// <summary>
        /// Gets or sets the organization marked as responsible for this it system usage.
        /// </summary>
        /// <value>
        /// The responsible organization.
        /// </value>
        public virtual Organization.Organization Organization { get; set; }

        public int ItSystemId { get; set; }
        /// <summary>
        /// Gets or sets the it system this instance is using.
        /// </summary>
        /// <value>
        /// It system.
        /// </value>
        public virtual ItSystem.ItSystem ItSystem { get; set; }

        public int? ArchiveTypeId { get; set; }
        public virtual ArchiveType ArchiveType { get; set; }

        public int? SensitiveDataTypeId { get; set; }
        public virtual SensitiveDataType SensitiveDataType { get; set; }

        public int? OverviewId { get; set; }
        /// <summary>
        /// Gets or sets the it system usage that is set to be displayed on the it system overview page.
        /// </summary>
        /// <remarks>
        /// It's the it system name that is actually displayed.
        /// </remarks>
        /// <value>
        /// The overview it system.
        /// </value>
        public virtual ItSystemUsage Overview { get; set; }

        /// <summary>
        /// Gets or sets the main it contract for this instance.
        /// The it contract is used to determine whether this instance
        /// is marked as active/inactive.
        /// </summary>
        /// <value>
        /// The main contract.
        /// </value>
        public virtual ItContractItSystemUsage MainContract { get; set; }
        /// <summary>
        /// Gets or sets it contracts associated with this instance.
        /// </summary>
        /// <value>
        /// The contracts.
        /// </value>
        public virtual ICollection<ItContractItSystemUsage> Contracts { get; set; }
        /// <summary>
        /// Gets or sets the wishes associated with this instance.
        /// </summary>
        /// <value>
        /// Wishes.
        /// </value>
        public virtual ICollection<Wish> Wishes { get; set; }
        /// <summary>
        /// Gets or sets the organization units associated with this instance.
        /// </summary>
        /// <value>
        /// The organization units.
        /// </value>
        public virtual ICollection<OrganizationUnit> OrgUnits { get; set; }
        /// <summary>
        /// Gets or sets the organization units that are using this instance.
        /// </summary>
        /// <remarks>
        /// Must be organization units that belongs to <see cref="Organization"/>.
        /// </remarks>
        /// <value>
        /// The organization units used by this instance.
        /// </value>
        public virtual ICollection<ItSystemUsageOrgUnitUsage> UsedBy { get; set; }
        /// <summary>
        /// Gets or sets the tasks this instance supports.
        /// </summary>
        /// <value>
        /// The supported tasks.
        /// </value>
        public virtual ICollection<TaskRef> TaskRefs { get; set; }
        /// <summary>
        /// The local usages of interfaces.
        /// </summary>
        public virtual ICollection<ItInterfaceUsage> ItInterfaceUsages { get; set; }
        /// <summary>
        /// The local exposures of interfaces.
        /// </summary>
        public virtual ICollection<ItInterfaceExhibitUsage> ItInterfaceExhibitUsages { get; set; }
        /// <summary>
        /// Gets or sets the associated it projects.
        /// </summary>
        /// <remarks>
        /// <see cref="ItProject.ItProject"/> have a corresponding property linking back.
        /// </remarks>
        /// <value>
        /// Associated it projects.
        /// </value>
        public virtual ICollection<ItProject.ItProject> ItProjects { get; set; }

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
