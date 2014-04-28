using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UI.MVC4.Models
{
    public class ItSystemUsageDTO
    {
        public int Id { get; set; }

        public string Note { get; set; }
        public string LocalSystemId { get; set; }
        public bool PersonSensitiveData { get; set; }
        public bool Archive { get; set; }
        public string EsdhRef { get; set; }
        public string CmdbRef { get; set; }
        public string DirectoryOrUrlRef { get; set; }
        public string AdOrIdmRef { get; set; }

        public int? ResponsibleUnitId { get; set; }
        public virtual OrgUnitDTO ResponsibleUnit { get; set; }
        public int OrganizationId { get; set; }
        public virtual OrganizationDTO Organization { get; set; }
        public int ItSystemId { get; set; }
        public virtual ItSystemDTO ItSystem { get; set; }

        //public virtual ICollection<ItContractDTO> Contracts { get; set; }
        public virtual ICollection<RoleDTO> SystemRoles { get; set; }
        //public virtual ICollection<WishDTO> Wishes { get; set; }
        public virtual ICollection<OrgUnitDTO> OrgUnits { get; set; }
        public virtual ICollection<TaskRefDTO> TaskRefs { get; set; }
    }
}