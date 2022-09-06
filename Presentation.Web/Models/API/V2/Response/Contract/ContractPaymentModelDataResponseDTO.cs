using System;
using System.Collections.Generic;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Types.Contract;

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
        [NonEmptyGuid]
        public IdentityNamePairResponseDTO PaymentFrequency { get; set; }
        /// <summary>
        /// Optionally assigned payment model
        /// </summary>
        [NonEmptyGuid]
        public IdentityNamePairResponseDTO PaymentModel { get; set; }
        /// <summary>
        /// Optionally assigned price regulation
        /// </summary>
        [NonEmptyGuid]
        public IdentityNamePairResponseDTO PriceRegulation { get; set; }
    }
}