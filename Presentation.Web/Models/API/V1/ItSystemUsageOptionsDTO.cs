using System.Collections.Generic;

namespace Presentation.Web.Models.API.V1
{
    public class ItSystemUsageOptionsDTO
    {
        public required IEnumerable<NamedEntityDTO> BusinessTypes { get; set; }
        public required IEnumerable<BusinessRoleDTO> SystemRoles { get; set; }
        public required IEnumerable<HierachyNodeDTO> OrganizationUnits { get; set; }
    }
}