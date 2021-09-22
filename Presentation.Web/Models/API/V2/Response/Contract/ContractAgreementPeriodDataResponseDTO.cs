using System;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Models.API.V2.Response.Contract
{
    public class ContractAgreementPeriodDataResponseDTO
    {
        /// <summary>
        /// If the agreement has a fixed duration, optionally define the years + months for which it is valid
        /// </summary>
        public int? DurationYears { get; set; }
        /// <summary>
        /// If the agreement has a fixed duration, optionally define the years + months for which it is valid
        /// </summary>
        public int? DurationMonths { get; set; }
        /// <summary>
        /// Determines if the agreement has no fixed duration
        /// </summary>
        public bool? IsContinuous { get; set; }
        /// <summary>
        /// Optional agreement extension option
        /// </summary>
        public IdentityNamePairResponseDTO ExtensionOptions { get; set; }
        /// <summary>
        /// Determines how many of the agreement available extension options that have been used
        /// </summary>
        public int ExtensionOptionsUsed { get; set; }
        /// <summary>
        /// The agreement cannot be revoked before this date
        /// </summary>
        public DateTime? IrrevocableUntil { get; set; }
    }
}