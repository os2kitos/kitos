using System;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.External.V2.Response
{
    public class IdentityNamePairResponseDTO
    {
        /// <summary>
        /// UUID which is unique within collection of entities of the same type
        /// </summary>
        [Required]
        public Guid Uuid { get; }

        /// <summary>
        /// Name
        /// </summary>
        [Required]
        public string Name { get; }

        public IdentityNamePairResponseDTO(Guid uuid, string name)
        {
            Uuid = uuid;
            Name = name;
        }
    }
}