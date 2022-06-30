using System;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Types.Contract;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Models.API.V2.Request.Contract
{
    public class ContractProcurementDataWriteRequestDTO
    {
        /// <summary>
        /// Optionally assigned purchase type
        /// Constraints:
        ///     - If changed from current state, the option type must be available in the organization
        /// </summary>
        [NonEmptyGuid]
        public Guid? PurchaseTypeUuid { get; set; }
        /// <summary>
        /// Optionally assigned procurement strategy
        /// Constraints:
        ///     - If changed from current state, the option type must be available in the organization
        /// </summary>
        [NonEmptyGuid]
        public Guid? ProcurementStrategyUuid { get; set; }
        /// <summary>
        /// Procurement plan
        /// </summary>
        public ProcurementPlanDTO ProcurementPlan { get; set; }
        /// <summary>
        ///     Repurchase Initiated for IT-Contract. (Genanskaffelse igangsat)
        /// </summary>
        public YesNoUndecidedChoice? ProcurementInitiated { get; set; }
    }
}