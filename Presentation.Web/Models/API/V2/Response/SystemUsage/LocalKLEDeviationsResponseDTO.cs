using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Models.API.V2.Response.SystemUsage
{
    /// <summary>
    /// Defines IT-System KLE deviations locally within an organization. All deviations are in the context of the inherited deviations which are found on the IT-System
    /// </summary>
    public class LocalKLEDeviationsResponseDTO
    {
        /// <summary>
        /// Inherited KLE which have been removed locally
        /// </summary>
        [Required]
        public IEnumerable<IdentityNamePairResponseDTO> RemovedKLE { get; set; }
        /// <summary>
        /// KLE which has been added locally
        /// </summary>
        [Required]
        public IEnumerable<IdentityNamePairResponseDTO> AddedKLE { get; set; }
    }
}