using System;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Models.API.V2.Request.Contract
{
    public class ContractAgreementPeriodDataWriteRequestDTO
    {
        /// <summary>
        /// If the agreement has a fixed duration, optionally define the years + months for which it is valid
        /// Constraints:
        ///     - If DurationMonths/Years are defined then IsContinuous must be null or false
        /// </summary>
        [Range(1, int.MaxValue)]
        public int? DurationYears { get; set; }
        /// <summary>
        /// If the agreement has a fixed duration, optionally define the years + months for which it is valid
        /// Constraints:
        ///     - If DurationMonths/Years are defined then IsContinuous must be null or false
        /// </summary>
        [Range(1,11)]
        public int? DurationMonths { get; set; }
        /// <summary>
        /// Determines if the agreement has no fixed duration
        /// Constraints:
        ///     - If set to true, the DurationMonths/Years must be null
        /// </summary>
        public bool? IsContinuous { get; set; }
        /// <summary>
        /// Optional agreement extension option
        /// Constraints:
        ///     - If changed from current state, the option type must be available in the organization
        /// </summary>
        [NonEmptyGuid]
        public Guid? ExtensionOptionsUuid { get; set; }
        /// <summary>
        /// Determines how many of the agreement available extension options that have been used
        /// </summary>
        [Range(0,int.MaxValue)]
        public int ExtensionOptionsUsed { get; set; }
        /// <summary>
        /// The agreement cannot be revoked before this date
        /// </summary>
        public DateTime? IrrevocableUntil { get; set; }
    }
}