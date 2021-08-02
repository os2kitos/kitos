using System;

namespace Presentation.Web.Models.External.V2.Request.SystemUsage
{
    public class ValidityWriteRequestDTO
    {
        /// <summary>
        /// Determines if this system has been forced into valid state even if context properties would dictate otherwise (e.g. no longer in use)
        /// </summary>
        public bool EnforcedValid { get; set; }
        /// <summary>
        /// If specified, the system usage is valid from this date.
        /// </summary>
        public DateTime? ValidFrom { get; set; }
        /// <summary>
        /// If specified, the system usage is valid up until and including this date.
        /// </summary>
        public DateTime? ValidTo { get; set; }
    }
}