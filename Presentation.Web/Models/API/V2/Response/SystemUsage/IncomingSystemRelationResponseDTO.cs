using System.ComponentModel.DataAnnotations;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Models.API.V2.Response.SystemUsage
{
    public class IncomingSystemRelationResponseDTO : BaseSystemRelationResponseDTO
    {
        /// <summary>
        /// Identifies the system usage the relation points to
        /// </summary>
        [Required]
        public IdentityNamePairResponseDTO FromSystemUsage { get; set; }
    }
}