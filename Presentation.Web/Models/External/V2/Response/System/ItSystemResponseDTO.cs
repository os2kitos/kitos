using System;
using System.Collections.Generic;
using Presentation.Web.Models.External.V2.Response.Organization;
using Presentation.Web.Models.External.V2.SharedProperties;

namespace Presentation.Web.Models.External.V2.Response.System
{
    public class ItSystemResponseDTO: BaseItSystemResponseDTO, IHasLastModified
    {
        /// <summary>
        /// Organizations using this IT-System
        /// </summary>
        public IEnumerable<ShallowOrganizationResponseDTO> UsingOrganizations { get; set; }

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