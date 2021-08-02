using System;
using System.Collections.Generic;

namespace Presentation.Web.Models.External.V2.Request.SystemUsage
{
    public class LocalKLEDeviationsRequestDTO
    {
        /// <summary>
        /// Inherited KLE which have been removed locally
        /// Constraint: Contents cannot intersect with AddedKLEUuids
        /// </summary>
        public IEnumerable<Guid> RemovedKLEUuids { get; set; }
        /// <summary>
        /// KLE which has been added locally
        /// Constraint: Contents cannot intersect with RemovedKLEUuids
        /// </summary>
        public IEnumerable<Guid> AddedKLEUuids { get; set; }
    }
}