using System;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using System.Collections.Generic;
using Presentation.Web.Models.API.V2.Types.Contract;

namespace Presentation.Web.Models.API.V2.Response.Contract
{
    public class ContractPaymentModelDataResponseDTO
    {
        /// <summary>
        /// Optional calendar invoice frequency: Yearly (0), HalfYearly (1), Quarterly (2).
        /// </summary>
        public InvoiceFrequencyChoice? InvoiceFrequency { get; set; }

        /// <summary>
        /// Full invoice dates ordered by period; empty when no dates are assigned.
        /// </summary>
        public IEnumerable<ContractInvoiceDateDTO> InvoiceDates { get; set; } = [];

        /// <summary>
        /// The data at which operations remuneration started
        /// </summary>
        public DateTime? OperationsRemunerationStartedAt { get; set; }
        /// <summary>
        /// Optionally assigned payment frequency
        /// </summary>
        public IdentityNamePairResponseDTO? PaymentFrequency { get; set; }
        /// <summary>
        /// Optionally assigned payment model
        /// </summary>
        public IdentityNamePairResponseDTO? PaymentModel { get; set; }
        /// <summary>
        /// Optionally assigned price regulation
        /// </summary>
        public IdentityNamePairResponseDTO? PriceRegulation { get; set; }
    }
}
