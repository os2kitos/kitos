using System;

namespace Presentation.Web.Models.External.V2.Response.Options
{
    public class RoleOptionExtendedResponseDTO : RoleOptionResponseDTO, IOptionAvailability
    {
        /// <summary>
        /// IsAvailable is set to true if the type is available in the requested organization context.
        /// If set to false, changes which point to this will fail since it has been deprecated within the organization.
        /// Existing registrations will be unaffected.
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