using System.Collections.Generic;

namespace Presentation.Web.Models.API.V2.Internal.Response.Organizations.Conflicts
{
    public class MultipleConflictsResponseDTO
    {
        public string MainEntityName { get; set; }
        public IEnumerable<SimpleConflictResponseDTO> Conflicts { get; set; }
    }
}