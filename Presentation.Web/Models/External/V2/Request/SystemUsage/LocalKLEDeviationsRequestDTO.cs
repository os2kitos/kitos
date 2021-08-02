using System;
using System.Collections.Generic;

namespace Presentation.Web.Models.External.V2.Request.SystemUsage
{
    public class LocalKLEDeviationsRequestDTO
    {
        /// <summary>
        /// Inherited KLE which have been removed locally
        /// </summary>
        public IEnumerable<Guid> RemovedKLEUuids { get; set; }
        /// <summary>
        /// KLE which has been added locally
        /// </summary>
        public IEnumerable<Guid> AddedKLEUuids { get; set; }
    }
}