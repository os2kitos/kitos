using System;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.SharedProperties;

namespace Presentation.Web.Models.API.V2.Internal.Response.ItSystem
{
    public class ItSystemSearchResponseDTO : IdentityNamePairResponseDTO, IHasDeactivatedExternal
    {

        [Required]
        public bool Deactivated { get; set; }

        public ItSystemSearchResponseDTO()
        {
        }

        public ItSystemSearchResponseDTO(Guid uuid, string name, bool deactivated) 
            : base(uuid, name)
        {
            Deactivated = deactivated;
        }
    }
}