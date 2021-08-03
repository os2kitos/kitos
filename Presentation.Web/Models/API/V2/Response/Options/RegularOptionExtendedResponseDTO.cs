using System;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Models.API.V2.Response.Options
{
    public class RegularOptionExtendedResponseDTO : IdentityNamePairResponseDTO, IOptionAvailability
    {
        public RegularOptionExtendedResponseDTO(Guid uuid, string name, bool isAvailable) : base(uuid, name)
        {
            IsAvailable = isAvailable;
        }
        /// <summary>
        /// IsAvailable is set to true if the type is available in the requested organization context.
        /// If set to false, changes which point to this will fail since it has been deprecated within the organization.
        /// Existing registrations will be unaffected.
        /// </summary>
        public bool IsAvailable { get; set; }
    }
}