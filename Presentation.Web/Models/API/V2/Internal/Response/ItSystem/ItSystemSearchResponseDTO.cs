using System;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Models.API.V2.Internal.Response.ItSystem
{
    public class ItSystemSearchResponseDTO : IdentityNamePairResponseDTO
    {

        [Required]
        public bool Disabled { get; set; }

        public ItSystemSearchResponseDTO()
        {
        }

        public ItSystemSearchResponseDTO(Guid uuid, string name, bool disabled) 
            : base(uuid, name)
        {
            Disabled = disabled;
        }
    }
}