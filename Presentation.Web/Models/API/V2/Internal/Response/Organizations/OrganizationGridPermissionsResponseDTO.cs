using System;

namespace Presentation.Web.Models.API.V2.Internal.Response.Organizations
{
    public class OrganizationGridPermissionsResponseDTO
    {
        public Guid OrgUuid {get; set;}
        public bool HasConfigModificationPermissions {get; set;}
    }
}