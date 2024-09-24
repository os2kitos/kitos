using Presentation.Web.Models.API.V2.Response.Shared;

namespace Presentation.Web.Models.API.V2.Internal.Response.User
{
    public class UserCollectionPermissionsResponseDTO : ResourceCollectionPermissionsResponseDTO
    {
        public bool Modify { get; set; }
        public bool Delete { get; set; }

        public UserCollectionPermissionsResponseDTO()
        {
            
        }

        public UserCollectionPermissionsResponseDTO(bool create, bool modify, bool delete) : this()
        {
            Create = create;
            Modify = modify;
            Delete = delete;
        }
    }
}