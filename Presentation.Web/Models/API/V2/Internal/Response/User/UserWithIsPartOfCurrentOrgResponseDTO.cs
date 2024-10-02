namespace Presentation.Web.Models.API.V2.Internal.Response.User
{
    public class UserWithIsPartOfCurrentOrgResponseDTO : UserResponseDTO
    {
        public bool IsPartOfCurrentOrganization { get; set; }
    }
}