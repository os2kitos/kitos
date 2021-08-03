using System;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Models.API.V2.SharedProperties;

namespace Presentation.Web.Models.API.V2.Response.Generic.Identity
{
    public class IdentityNamePairResponseDTO : IHasNameExternal, IHasUuidExternal
    {
        /// <summary>
        /// UUID which is unique within collection of entities of the same type
        /// </summary>
        [Required]
        public Guid Uuid { get; set; }

        /// <summary>
        /// Human readable name of the entity
        /// </summary>
        [Required]
        public string Name { get; set; }

        protected IdentityNamePairResponseDTO()
        {

        }

        public IdentityNamePairResponseDTO(Guid uuid, string name)
        {
            Uuid = uuid;
            Name = name;
        }
    }
}