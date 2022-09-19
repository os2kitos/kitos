using Presentation.Web.Models.API.V2.Types.SystemUsage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Presentation.Web.Models.API.V2.Response.Contract
{
    public class ContractValidityResponseDTO
    {
        /// <summary>
        /// Determines if the entity is considered valid. This is computed from both "EnforcedValid" as well as ValidAccordingToValidityPeriod.
        /// </summary>
        public bool Valid { get; set; }
        /// <summary>
        /// Determines if this entity has been forced into valid state even if context properties would dictate otherwise (e.g. no longer in use)
        /// </summary>
        public bool EnforcedValid { get; set; }
        /// <summary>
        /// If specified, the entity is valid from this date.
        /// </summary>
        public DateTime? ValidFrom { get; set; }
        /// <summary>
        /// If specified, the entity is valid up until and including this date.
        /// </summary>
        public DateTime? ValidTo { get; set; }
    }
}