using System.Collections.Generic;
using Core.DomainModel.ItSystem;

namespace UI.MVC4.Models
{
    public class ItSystemUsageDTO
    {
        public int Id { get; set; }
        public bool IsStatusActive { get; set; }
        public string Note { get; set; }
        public string LocalSystemId { get; set; }

        public string EsdhRef { get; set; }
        public string CmdbRef { get; set; }
        public string DirectoryOrUrlRef { get; set; }
        public string AdOrIdmRef { get; set; }

        public int? SensitiveDataTypeId { get; set; }
        public int? ArchiveTypeId { get; set; }
        
        public int? ResponsibleUnitId { get; set; }
        public virtual OrgUnitDTO ResponsibleUnit { get; set; }

        public int OrganizationId { get; set; }
        public virtual OrganizationDTO Organization { get; set; }

        public  int ItSystemId { get; set; }
        public virtual ItSystemDTO ItSystem { get; set; }

        public int? OverviewItSystemId { get; set; }

        public virtual IEnumerable<RightOutputDTO> Rights { get; set; }
        public virtual IEnumerable<InterfaceUsageDTO> InterfaceUsages { get; set; } 
    }
}