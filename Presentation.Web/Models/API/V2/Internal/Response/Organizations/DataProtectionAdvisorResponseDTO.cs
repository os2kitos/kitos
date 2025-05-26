namespace Presentation.Web.Models.API.V2.Internal.Response.Organizations
{
    public class DataProtectionAdvisorResponseDTO: OrganizationMasterDataRoleResponseDTO
    {
        public string Cvr { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
    }
}