using Presentation.Web.Models.API.V2.Response.Generic.Identity;
namespace Presentation.Web.Models.API.V2.Internal.Response.Organizations
{
    public class OrganizationMasterDataResponseDTO : IdentityNamePairResponseDTO
    {
        public string Cvr { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
    }
}