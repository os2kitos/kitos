using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    public class ItSystemUsage : IEntity<int>
    {
        public int Id { get; set; }
        public bool IsStatusActive { get; set; }
        public string Note { get; set; }
        public string SystemId { get; set; }
        
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

        public virtual ICollection<ItContract.ItContract> Contracts { get; set; }
        public virtual ICollection<ItSystemRole> SystemRoles { get; set; }
        public virtual ICollection<Wish> Wishes { get; set; }
        public virtual ICollection<OrganizationUnit> OrgUnits { get; set; }
        public virtual ICollection<TaskRef> TaskRefs { get; set; }
    }
}
