using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.External.V2.Response
{
    public class ItSystemResponseDTO: ItSystemInformationResponseDTO
    {
        /// <summary>
        /// Organizations using this IT-System
        /// </summary>
        [Required]
        public IEnumerable<OrganizationResponseDTO> UsingOrganizations { get; set; }
    }
}