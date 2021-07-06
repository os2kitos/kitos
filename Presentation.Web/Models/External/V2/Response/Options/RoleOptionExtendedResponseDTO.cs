using System;

namespace Presentation.Web.Models.External.V2.Response.Options
{
    public class RoleOptionExtendedResponseDTO : RoleOptionResponseDTO
    {
        /// <summary>
        /// IsAvailable is set to true if the type is available in the requested context
        /// </summary>
        public bool IsAvailable { get; set; }

        public RoleOptionExtendedResponseDTO()
        {

        }

        public RoleOptionExtendedResponseDTO(Guid uuid, string name, bool writeAccess, bool isAvailable) 
            : base(uuid, name, writeAccess)
        {
            IsAvailable = isAvailable;
        }
    }
}