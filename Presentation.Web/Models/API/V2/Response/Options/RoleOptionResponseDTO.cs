using System;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Models.API.V2.Response.Options
{
    public class RoleOptionResponseDTO : IdentityNamePairResponseDTO
    {
        /// <summary>
        /// Determines if this role grants write access to the entity through which it has been created
        /// </summary>
        public bool WriteAccess { get; set; }

        public RoleOptionResponseDTO() { }

        public RoleOptionResponseDTO(Guid uuid, string name, bool writeAccess)
            : base(uuid, name)
        {
            WriteAccess = writeAccess;
        }
    }
}