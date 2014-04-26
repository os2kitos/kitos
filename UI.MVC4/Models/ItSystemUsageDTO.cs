using System.Collections.Generic;

namespace UI.MVC4.Models
{
    public class ItSystemUsageDTO
    {
        public int Id { get; set; }

        public string Note { get; set; }
        public string SystemId { get; set; }
        public int? SensitiveDataType { get; set; }
        public int? ArchiveType { get; set; }
        public string EsdhRef { get; set; }
        public string CmdbRef { get; set; }
        public string DirectoryOrUrlRef { get; set; }
        public string AdOrIdmRef { get; set; }

        public int? ResponsibleUnitId { get; set; }
        public OrgUnitDTO ResponsibleUnit { get; set; }
        
        public int ParentId { get; set; }
        public ItSystemDTO Parent { get; set; }
        
        public int OrganizationId { get; set; }
        public OrganizationDTO Organization { get; set; }
        
        public int ItSystemId { get; set; }
        public ItSystemDTO ItSystem { get; set; }

        //public IEnumerable<ItContractDTO> Contracts { get; set; }
        public IEnumerable<RoleDTO> SystemRoles { get; set; }
        //public IEnumerable<WishDTO> Wishes { get; set; }
        public IEnumerable<OrgUnitDTO> OrgUnits { get; set; }
        public IEnumerable<TaskRefDTO> TaskRefs { get; set; }
    }
}