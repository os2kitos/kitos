using System;

namespace Presentation.Web.Models.API.V2.Response.Options
{
    public class RoleOptionResponseDTO : RegularOptionResponseDTO
    {
        /// <summary>
        /// Determines if this role grants write access to the entity through which it has been created
        /// </summary>
        public bool WriteAccess { get; set; }

        public RoleOptionResponseDTO() { }

        public RoleOptionResponseDTO(Guid uuid, string name, bool writeAccess, string description)
            : base(uuid, name,description)
        {
            WriteAccess = writeAccess;
        }
    }
}