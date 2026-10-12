using System.Collections.Generic;

namespace Presentation.Web.Models.API.V2.Internal.Response.Organizations.Conflicts
{
    public class SystemWithUsageOutsideOrganizationConflictResponseDTO
    {
        public required string SystemName { get; set; }
        public required IEnumerable<string> OrganizationNames { get; set; }
    }
}