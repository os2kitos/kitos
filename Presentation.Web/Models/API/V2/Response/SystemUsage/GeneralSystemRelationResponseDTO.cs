using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V2.Response.SystemUsage
{
    public class GeneralSystemRelationResponseDTO : BaseSystemRelationResponseDTO
    {
        /// <summary>
        /// Identifies the system usage the relation points to
        /// </summary>
        [Required]
        public IdentityNamePairResponseDTO ToSystemUsage { get; set; }

        /// <summary>
        /// Identifies the origin system usage of the incoming system relation
        /// </summary>
        [Required]
        public IdentityNamePairResponseDTO FromSystemUsage { get; set; }
    }
}