using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Types.Organization;

namespace Presentation.Web.Models.API.V2.Response.Organization
{
    public class OrganizationUnitResponseDTO : IdentityNamePairResponseDTO
    {
        /// <summary>
        /// Optional Link to parent unit in the organizational hierarchy
        /// </summary>
        public IdentityNamePairResponseDTO ParentOrganizationUnit { get; set; }
        /// <summary>
        /// Optional EAN number for the organization unit.
        /// </summary>
        public long? Ean { get; set; }
        /// <summary>
        /// Optional Organization Unit Id
        /// </summary>
        public string UnitId { get; set; }

        public OrganizationUnitOriginChoice Origin { get; set; }
    }
}