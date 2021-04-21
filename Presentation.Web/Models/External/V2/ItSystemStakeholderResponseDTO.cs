using System.Collections.Generic;

namespace Presentation.Web.Models.External.V2
{
    public class ItSystemStakeholderResponseDTO: ItSystemResponseDTO
    {
        public IEnumerable<IdentityNamePairResponseDTO> UsingOrganizations { get; set; }
    }
}