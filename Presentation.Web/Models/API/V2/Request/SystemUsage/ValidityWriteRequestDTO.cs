using System;
using Presentation.Web.Models.API.V2.Types.SystemUsage;


namespace Presentation.Web.Models.API.V2.Request.SystemUsage
{
    public class ValidityWriteRequestDTO
    { 
        /// <summary>
        /// Determines the life cycle status of the system (e.g. Not in use, Operational)
        /// </summary>
        public LifeCycleStatusChoice? LifeCycleStatus { get; set; }
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
    }
}