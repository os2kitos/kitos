using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    public class ItSystemUsage : IEntity<int>, IHasRights<ItSystemRight>, IHasOwner
    {
        public ItSystemUsage()
        {
            this.Contracts = new List<ItContract.ItContract>();
            this.Wishes = new List<Wish>();
            this.OrgUnits = new List<OrganizationUnit>();
            this.TaskRefs = new List<TaskRef>();
            this.Rights = new List<ItSystemRight>();
            this.InterfaceUsages = new List<InterfaceUsage>();
            this.InterfaceWishes = new List<ItSystem>();
            this.UsedBy = new List<OrganizationUnit>();
            this.ItProjects = new List<ItProject.ItProject>();
        }

        public int Id { get; set; }
        public bool IsStatusActive { get; set; }
        public string Note { get; set; }
        public string LocalSystemId { get; set; }
        
        public string EsdhRef { get; set; }
        public string CmdbRef { get; set; }
        public string DirectoryOrUrlRef { get; set; }
        public string AdOrIdmRef { get; set; }

        public int? ResponsibleUnitId { get; set; }
        public virtual OrganizationUnit ResponsibleUnit { get; set; }
        
        public int OrganizationId { get; set; }
        public virtual Organization Organization { get; set; }
        
        public int ItSystemId { get; set; }
        public virtual ItSystem ItSystem { get; set; }

        public int? ArchiveTypeId { get; set; }
        public virtual ArchiveType ArchiveType { get; set; }

        public int? SensitiveDataTypeId { get; set; }
        public virtual SensitiveDataType SensitiveDataType { get; set; }

        public int? OverviewItSystemId { get; set; }
        public virtual ItSystem OverviewItSystem { get; set; }

        public virtual ICollection<ItContract.ItContract> Contracts { get; set; }
        public virtual ICollection<Wish> Wishes { get; set; }
        public virtual ICollection<OrganizationUnit> OrgUnits { get; set; }

        /// <summary>
        /// These Organization Units are using this system
        /// </summary>
        public virtual ICollection<OrganizationUnit> UsedBy { get; set; }
        
        /// <summary>
        /// IT System support these tasks
        /// </summary>
        public virtual ICollection<TaskRef> TaskRefs { get; set; }

        public virtual ICollection<ItSystemRight> Rights { get; set; }

        /// <summary>
        /// The usages of interfaces. These are created when a contract of the usage is registered.
        /// </summary>
        public virtual ICollection<InterfaceUsage> InterfaceUsages { get; set; }

        /// <summary>
        /// Collection of interfaces, that is wished to be used.
        /// </summary>
        public virtual ICollection<ItSystem> InterfaceWishes { get; set; }

        public int ObjectOwnerId { get; set; }
        public virtual User ObjectOwner { get; set; }

        public virtual ICollection<ItProject.ItProject> ItProjects { get; set; }
    }
}
