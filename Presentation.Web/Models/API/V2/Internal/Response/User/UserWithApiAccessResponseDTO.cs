using System;
using System.Diagnostics.CodeAnalysis;

namespace Presentation.Web.Models.API.V2.Internal.Response.User
{
    public class UserWithApiAccessResponseDTO : UserReferenceResponseDTO
    {
        public bool ApiAccess { get; set; }
        protected UserWithApiAccessResponseDTO()
        {
        }

        [SetsRequiredMembers]
        public UserWithApiAccessResponseDTO(Guid uuid, string name, string email, bool apiAccess)
            : base(uuid, name, email)
        {
            ApiAccess = apiAccess;
        }
    }
}