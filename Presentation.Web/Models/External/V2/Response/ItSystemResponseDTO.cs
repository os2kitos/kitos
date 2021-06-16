using System;
using System.Collections.Generic;

namespace Presentation.Web.Models.External.V2.Response
{
    public class ItSystemResponseDTO: BaseItSystemResponseDTO
    {
        /// <summary>
        /// Organizations using this IT-System
        /// </summary>
        public IEnumerable<OrganizationResponseDTO> UsingOrganizations { get; set; }

        /// <summary>
        /// Time of last modification
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Responsible for last modification
        /// </summary>
        public IdentityNamePairResponseDTO LastModifiedBy { get; set; }
    }
}