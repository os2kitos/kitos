using System;
using System.Diagnostics.CodeAnalysis;

namespace Presentation.Web.Models.API.V2.Response.Options
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

        [SetsRequiredMembers]
        public RoleOptionExtendedResponseDTO(Guid uuid, string name, bool writeAccess, bool isAvailable, string description, bool isExternallyAvailable, string externallyAvailableDescription)
            : base(uuid, name, writeAccess, description, isExternallyAvailable, externallyAvailableDescription)
        {
            IsAvailable = isAvailable;
        }
    }
}