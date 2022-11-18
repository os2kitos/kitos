using System.Collections.Generic;

namespace Presentation.Web.Models.API.V1.Organizations
{
    public class ConnectionUpdateConsequencesResponseDTO
    {
        public IEnumerable<ConnectionUpdateOrganizationUnitConsequenceResponseDTO> Consequences { get; set; }
    }
}