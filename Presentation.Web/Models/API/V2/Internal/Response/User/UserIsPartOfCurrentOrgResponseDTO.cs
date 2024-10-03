using System;

namespace Presentation.Web.Models.API.V2.Internal.Response.User
{
    public class UserIsPartOfCurrentOrgResponseDTO
    {
        public Guid Uuid { get; set; }
        public bool IsPartOfCurrentOrganization { get; set; }
    }
}