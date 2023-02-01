using System;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Models.API.V2.Response.Options
{
    public class RegularOptionResponseDTO : IdentityNamePairResponseDTO
    {
        protected RegularOptionResponseDTO()
        {
        }

        public RegularOptionResponseDTO(Guid uuid, string name, string description) : base(uuid, name)
        {
            Description = description;
        }

        /// <summary>
        /// Extended description of the choice
        /// </summary>
        [Required]
        public string Description { get; set; }
    }
}