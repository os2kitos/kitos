using System;
using System.Diagnostics.CodeAnalysis;

namespace Presentation.Web.Models.API.V2.Response.Options
{
    public class RoleOptionResponseDTO : RegularOptionResponseDTO
    {
        /// <summary>
        /// Determines if this role grants write access to the entity through which it has been created
        /// </summary>
        public bool WriteAccess { get; set; }

        /// <summary>
        /// Determines if this role is used by any existing entities
        /// </summary>
        public bool IsExternallyAvailable { get; set; }
        /// <summary>
        /// Description about this roles usage externally
        /// </summary>
        public required string ExternallyAvailableDescription { get; set; }

        public RoleOptionResponseDTO() { }

        [SetsRequiredMembers]
        public RoleOptionResponseDTO(Guid uuid, string name, bool writeAccess, string description, bool isExternallyAvailable, string externallyAvailableDescription)
            : base(uuid, name, description)
        {
            WriteAccess = writeAccess;
            IsExternallyAvailable = isExternallyAvailable;
            ExternallyAvailableDescription = externallyAvailableDescription;
        }
    }
}