using System.Collections.Generic;

namespace Presentation.Web.Models.External.V2.Response
{
    public class ItSystemInResponseDto: ItSystemInformationResponseDTO
    {
        /// <summary>
        /// Organizations using this IT-System
        /// </summary>
        public IEnumerable<OrganizationResponseDTO> UsingOrganizations { get; set; }
    }
}