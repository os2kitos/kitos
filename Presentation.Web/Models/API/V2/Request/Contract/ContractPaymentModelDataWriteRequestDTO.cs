using System;
using Presentation.Web.Infrastructure.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Models.API.V2.Types.Contract;

namespace Presentation.Web.Models.API.V2.Request.Contract
{
    public class ContractPaymentModelDataWriteRequestDTO
    {
        /// <summary>
        /// Optional calendar invoice frequency: Yearly (0), HalfYearly (1), Quarterly (2).
        /// Changing the frequency discards old dates before applying supplied dates; null clears the frequency and dates.
        /// POST/PATCH omission preserves the current frequency. PUT omission resets it, as for other payment model fields.
        /// </summary>
        [EnumDataType(typeof(InvoiceFrequencyChoice))]
        public InvoiceFrequencyChoice? InvoiceFrequency { get; set; }

        /// <summary>
        /// Optional replacement list of full-date/period pairs. Requires an invoice frequency.
        /// Empty or null clears all dates. Omission preserves dates unless frequency changes (including PUT frequency reset).
        /// Periods must be unique and within the selected frequency.
        /// </summary>
        public IEnumerable<ContractInvoiceDateDTO>? InvoiceDates { get; set; }

        /// <summary>
        /// The data at which operations remuneration started
        /// </summary>
        public DateTime? OperationsRemunerationStartedAt { get; set; }
        /// <summary>
        /// Optionally assigned payment frequency
        /// Constraints:
        ///     - If changed from current state, the option type must be available in the organization
        /// </summary>
        [NonEmptyGuid]
        public Guid? PaymentFrequencyUuid { get; set; }
        /// <summary>
        /// Optionally assigned payment model
        /// Constraints:
        ///     - If changed from current state, the option type must be available in the organization
        /// </summary>
        [NonEmptyGuid]
        public Guid? PaymentModelUuid { get; set; }
        /// <summary>
        /// Optionally assigned price regulation
        /// Constraints:
        ///     - If changed from current state, the option type must be available in the organization
        /// </summary>
        [NonEmptyGuid]
        public Guid? PriceRegulationUuid { get; set; }
    }
}
