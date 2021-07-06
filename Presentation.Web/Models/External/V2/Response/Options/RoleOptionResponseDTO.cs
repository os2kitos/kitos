using System;

namespace Presentation.Web.Models.External.V2.Response.Options
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