using System.Collections.Generic;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Models.API.V2.Response.SystemUsage
{
    public class OrganizationUsageResponseDTO
    {
        /// <summary>
        /// A collection of organization units which have taken this system into use
        /// </summary>
        public IEnumerable<IdentityNamePairResponseDTO> UsingOrganizationUnits { get; set; }
        /// <summary>
        /// Out of all of the using organization units, this one is responsible for the system within the organization.
        /// </summary>
        public IdentityNamePairResponseDTO ResponsibleOrganizationUnit { get; set; }
    }
}