using System.Collections.Generic;

namespace Presentation.Web.Models.API.V2.Internal.Response.Organizations.Conflicts
{
    public class SystemWithUsageOutsideOrganizationConflictResponseDTO
    {
        public string SystemName { get; set; }
        public IEnumerable<string> OrganizationNames { get; set; }
    }
}