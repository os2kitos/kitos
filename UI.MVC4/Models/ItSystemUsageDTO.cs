using System.Collections.Generic;

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
        public OrgUnitDTO ResponsibleUnit { get; set; }

        public int OrganizationId { get; set; }
        public OrganizationDTO Organization { get; set; }
        
        
        
        public int ItSystemId { get; set; }
        public ItSystemDTO ItSystem { get; set; }

        public int? OverviewItSystemId { get; set; }
        public string OverviewItSystemName { get; set; }

        public IEnumerable<RightOutputDTO> Rights { get; set; }
        
        public IEnumerable<OrgUnitDTO> UsedBy { get; set; }
        public IEnumerable<TaskRefDTO> TaskRefs { get; set; }

        public IEnumerable<InterfaceUsageDTO> InterfaceUsages { get; set; }
        public IEnumerable<InterfaceExposureDTO> InterfaceExposures { get; set; }

        public IEnumerable<ItProjectDTO> ItProjects { get; set; }
    }
}