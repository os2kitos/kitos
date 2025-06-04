namespace Presentation.Web.Models.API.V2.Internal.Response.Organizations
{
    public class ContactPersonResponseDTO: OrganizationMasterDataRoleResponseDTO
    {
            public string LastName { get; set; }
            public string PhoneNumber { get; set; }
        }
    }