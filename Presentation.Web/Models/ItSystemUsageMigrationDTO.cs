using System.Collections.Generic;

namespace Presentation.Web.Models
{
    public class ItSystemUsageMigrationDTO
    {
        public IEnumerable<ItInterfaceExhibitUsageDTO> ItInterfaceExhibitUsages;
        public ItSystemDTO fromItSystem;
        public ItSystemDTO toItSystem;
        public OrganizationDTO Organization;

    }
}