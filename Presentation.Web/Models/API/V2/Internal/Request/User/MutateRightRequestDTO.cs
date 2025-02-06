using System;

namespace Presentation.Web.Models.API.V2.Internal.Request.User
{
    public class MutateRightRequestDTO
    {
        public Guid UserUuid { get; set; }
        public int RoleId { get; set; }
        public Guid EntityUuid { get; set; }
    }
}
