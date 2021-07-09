using System.Collections.Generic;

namespace Presentation.Web.Models.External.V2.Response.SystemUsage
{
    /// <summary>
    /// Defines IT-System KLE deviations locally within an organization. All deviations are in the context of the inherited deviations which are found on the IT-System
    /// </summary>
    public class LocalKLEDeviations
    {
        /// <summary>
        /// Inherited KLE which have been removed locally
        /// </summary>
        public IEnumerable<IdentityNamePairResponseDTO> RemovedKLE { get; set; }
        /// <summary>
        /// KLE which has been added locally
        /// </summary>
        public IEnumerable<IdentityNamePairResponseDTO> AddedKLE { get; set; }
    }
}