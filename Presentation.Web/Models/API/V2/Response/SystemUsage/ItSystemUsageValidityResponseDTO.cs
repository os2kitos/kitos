using Presentation.Web.Models.API.V2.Types.SystemUsage;
using System;

namespace Presentation.Web.Models.API.V2.Response.SystemUsage
{
    public class ItSystemUsageValidityResponseDTO
    {
        /// <summary>
        /// Determines if the entity is considered valid. This is computed from ValidAccordingToValidityPeriod, ValidAccordingToLifeCycle and ValidAccordingToMainContract.
        /// </summary>
        public bool Valid { get; set; }
        /// <summary>
        /// Determines if the entity is considered valid based on the validity period defined by ValidFrom and ValidTo
        /// </summary>
        public bool ValidAccordingToValidityPeriod { get; set; }
        /// <summary>
        /// Determines if the entity is considered valid based on the Life Cycle
        /// </summary>
        public bool ValidAccordingToLifeCycle{ get; set; }
        /// <summary>
        /// Determines if the entity is considered valid based on the validity of the related contract.
        /// Note: This will be true if there is no contract (NoContract state) or if the contract is active.
        /// Consider using MainContractState for more detailed information.
        /// </summary>
        public bool ValidAccordingToMainContract{ get; set; }
        /// <summary>
        /// Detailed state of the main contract: NoContract (no contract associated), Active, or Inactive
        /// </summary>
        public MainContractStateChoice MainContractState { get; set; }
        /// <summary>
        /// Life cycle status of the entity
        /// </summary>
        public LifeCycleStatusChoice? LifeCycleStatus { get; set; }
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