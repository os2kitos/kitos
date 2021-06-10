using System.Collections.Generic;

namespace Presentation.Web.Models.External.V2.Response
{
    public class ItSystemStakeholderResponseDTO: ItSystemResponseDTO
    {
        /// <summary>
        /// Organzational information for organizations using this IT-System
        /// </summary>
        public IEnumerable<OrganizationResponseDTO> UsingOrganizations { get; set; }
    }
}