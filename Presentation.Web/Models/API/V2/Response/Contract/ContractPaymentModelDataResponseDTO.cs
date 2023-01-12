using System;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Models.API.V2.Response.Contract
{
    public class ContractPaymentModelDataResponseDTO
    {
        /// <summary>
        /// The data at which operations remuneration started
        /// </summary>
        public DateTime? OperationsRemunerationStartedAt { get; set; }
        /// <summary>
        /// Optionally assigned payment frequency
        /// </summary>
        public IdentityNamePairResponseDTO PaymentFrequency { get; set; }
        /// <summary>
        /// Optionally assigned payment model
        /// </summary>
        public IdentityNamePairResponseDTO PaymentModel { get; set; }
        /// <summary>
        /// Optionally assigned price regulation
        /// </summary>
        public IdentityNamePairResponseDTO PriceRegulation { get; set; }
    }
}