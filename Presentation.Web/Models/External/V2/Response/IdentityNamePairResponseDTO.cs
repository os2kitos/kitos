using Presentation.Web.Models.External.V2.SharedProperties;
using System;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.External.V2.Response
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