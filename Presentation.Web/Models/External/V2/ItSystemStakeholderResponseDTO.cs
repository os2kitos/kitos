using System.Collections.Generic;

namespace Presentation.Web.Models.External.V2
{
    public class ItSystemStakeholderResponseDTO: ItSystemResponseDTO
    {
        /// <summary>
        /// List of name and UUID pairs for organizations using this IT-System
        /// </summary>
        public IEnumerable<IdentityNamePairResponseDTO> UsingOrganizations { get; set; }
    }
}