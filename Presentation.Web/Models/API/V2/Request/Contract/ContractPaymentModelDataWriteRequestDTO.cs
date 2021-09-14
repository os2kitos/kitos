using System;
using System.Collections.Generic;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Types.Contract;

namespace Presentation.Web.Models.API.V2.Request.Contract
{
    public class ContractPaymentModelDataWriteRequestDTO
    {
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
        public IEnumerable<PaymentMileStoneDTO> PaymentMileStones { get; set; }
    }
}