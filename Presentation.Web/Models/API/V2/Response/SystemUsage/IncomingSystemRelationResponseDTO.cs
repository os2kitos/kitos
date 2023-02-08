using System.ComponentModel.DataAnnotations;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Models.API.V2.Response.SystemUsage
{
    public class IncomingSystemRelationResponseDTO : BaseSystemRelationResponseDTO
    {
        /// <summary>
        /// Identifies the origin system usage of the incoming system relation
        /// </summary>
        [Required]
        public IdentityNamePairResponseDTO FromSystemUsage { get; set; }
    }
}