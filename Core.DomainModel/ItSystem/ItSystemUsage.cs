using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    public class ItSystemUsage : IEntity<int>, IHasRights<ItSystemRight>
    {
        public ItSystemUsage()
        {
            this.Contracts = new List<ItContract.ItContract>();
            this.Wishes = new List<Wish>();
            this.OrgUnits = new List<OrganizationUnit>();
            this.TaskRefs = new List<TaskRef>();
            this.Rights = new List<ItSystemRight>();
            this.InterfaceUsages = new List<InterfaceUsage>();
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
        public virtual ICollection<TaskRef> TaskRefs { get; set; }

        public virtual ICollection<ItSystemRight> Rights { get; set; }

        public virtual ICollection<InterfaceUsage> InterfaceUsages { get; set; } 
    }
}
