using Presentation.Web.Models.API.V2.Types.SystemUsage;
using System;

namespace Presentation.Web.Models.API.V2.Response.SystemUsage
{
    public class ValidityResponseDTO
    {
        /// <summary>
        /// Determines if the entity is considered valid. This is computed from both "EnforcedValid" as well as ValidAccordingToValidityPeriod.
        /// </summary>
        public bool Valid { get; set; }
        /// <summary>
        /// Determines if the entity is considered valid based on the validity period defined by ValidFrom and ValidTo
        /// </summary>
        public bool ValidAccordingToValidityPeriod { get; set; }
        public bool ValidAccordingToLifeCycle{ get; set; }
        /// <summary>
        /// Life cycle status of the entity
        /// </summary>
        public LifeCycleStatusChoice? LifeCycleStatus { get; set; }

        //TODO: Add ValidAccordingToMainItContract (for the next task regarding itcontract)
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