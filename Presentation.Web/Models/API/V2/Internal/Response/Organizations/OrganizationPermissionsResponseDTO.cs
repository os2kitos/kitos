using Presentation.Web.Models.API.V2.Response.Shared;

namespace Presentation.Web.Models.API.V2.Internal.Response.Organizations
{
    public class OrganizationPermissionsResponseDTO: ResourcePermissionsResponseDTO
    {
        public bool ModifyCvr { get; set; }
    }
}