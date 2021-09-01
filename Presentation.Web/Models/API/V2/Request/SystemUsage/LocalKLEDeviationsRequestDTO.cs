using System;
using System.Collections.Generic;

namespace Presentation.Web.Models.API.V2.Request.SystemUsage
{
    public class LocalKLEDeviationsRequestDTO
    {
        /// <summary>
        /// Inherited KLE which have been removed locally
        /// Constraints:
        ///     - No duplicates
        ///     - Contents CANNOT intersect with AddedKLEUuids
        ///     - Contents MUST be a complete subset of the KLE set on the system context
        /// </summary>
        public IEnumerable<Guid> RemovedKLEUuids { get; set; }
        /// <summary>
        /// KLE which has been added locally
        /// Constraints:
        ///     - No duplicates
        ///     - Contents CANNOT intersect with RemovedKLEUuids
        ///     - Contents CANNOT be a subset of the KLE set on the system context
        /// </summary>
        public IEnumerable<Guid> AddedKLEUuids { get; set; }
    }
}