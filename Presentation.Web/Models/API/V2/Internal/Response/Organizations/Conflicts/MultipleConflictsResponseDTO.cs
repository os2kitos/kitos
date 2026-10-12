using System.Collections.Generic;

namespace Presentation.Web.Models.API.V2.Internal.Response.Organizations.Conflicts
{
    public class MultipleConflictsResponseDTO
    {
        public required string MainEntityName { get; set; }
        public required IEnumerable<SimpleConflictResponseDTO> Conflicts { get; set; }
    }
}