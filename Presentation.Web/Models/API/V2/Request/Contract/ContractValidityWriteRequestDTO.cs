using System;

namespace Presentation.Web.Models.API.V2.Request.Contract
{
    public class ContractValidityWriteRequestDTO
    {
        /// <summary>
        /// Determines if the entity has been forced into valid state even if context properties would dictate otherwise (e.g. no longer in use)
        /// </summary>
        public bool EnforcedValid { get; set; }
        /// <summary>
        /// If specified, the entity is valid from this date.
        /// Must be less than or equal to ValidTo
        /// </summary>
        public DateTime? ValidFrom { get; set; }
        /// <summary>
        /// If specified, the entity is valid up until and including this date.
        /// Must be greater than or equal to ValidFrom
        /// </summary>
        public DateTime? ValidTo { get; set; }
        /// <summary>
        /// Determines if the parent contract should be part of the contracts validation
        /// </summary>
        public bool RequireValidParent { get; set; }

    }
}