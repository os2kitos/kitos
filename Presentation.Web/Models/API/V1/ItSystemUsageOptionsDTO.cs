using System.Collections.Generic;

namespace Presentation.Web.Models.API.V1
{
    public class ItSystemUsageOptionsDTO
    {
        public IEnumerable<NamedEntityDTO> BusinessTypes { get; set; }
        public IEnumerable<BusinessRoleDTO> SystemRoles { get; set; }
        public IEnumerable<HierachyNodeDTO> OrganizationUnits { get; set; }
    }
}