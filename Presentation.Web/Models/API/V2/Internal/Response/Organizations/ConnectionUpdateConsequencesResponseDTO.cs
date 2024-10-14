using System.Collections.Generic;

namespace Presentation.Web.Models.API.V2.Internal.Response.Organizations
{
    public class ConnectionUpdateConsequencesResponseDTO
    {
        public IEnumerable<ConnectionUpdateOrganizationUnitConsequenceDTO> Consequences { get; set; }
    }
}