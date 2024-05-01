using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Models.API.V2.Response.Organization
{
    public class OrganizationUnitWithEanResponseDTO : IdentityNamePairResponseDTO
    {
        /// <summary>
        /// Optional EAN number for the organization unit.
        /// </summary>
        public long? Ean { get; set; }
    }
}