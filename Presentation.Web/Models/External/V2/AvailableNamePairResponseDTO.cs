using Presentation.Web.Models.External.V2.Response;
using System;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.External.V2
{
    public class AvailableNamePairResponseDTO : IdentityNamePairResponseDTO
    {
        /// <summary>
        /// IsAvailable is set to true if the type is available in the requested context
        /// </summary>
        [Required]
        public bool IsAvailable { get; }

        public AvailableNamePairResponseDTO(Guid uuid, string name, bool isAvailable) : base(uuid, name)
        {
            IsAvailable = isAvailable;
        }
    }
}