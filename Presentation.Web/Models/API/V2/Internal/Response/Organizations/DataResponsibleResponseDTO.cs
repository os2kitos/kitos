namespace Presentation.Web.Models.API.V2.Internal.Response.Organizations
{
    public class DataResponsibleResponseDTO: OrganizationMasterDataRoleResponseDTO
    {
        public string Cvr { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
    }
}