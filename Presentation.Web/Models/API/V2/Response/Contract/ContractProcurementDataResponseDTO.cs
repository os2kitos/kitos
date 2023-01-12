using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Types.Contract;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Models.API.V2.Response.Contract
{
    public class ContractProcurementDataResponseDTO
    {
        /// <summary>
        /// Optionally assigned purchase type
        /// </summary>
        public IdentityNamePairResponseDTO PurchaseType { get; set; }
        /// <summary>
        /// Optionally assigned procurement strategy
        /// </summary>
        public IdentityNamePairResponseDTO ProcurementStrategy { get; set; }
        /// <summary>
        /// Procurement plan
        /// </summary>
        public ProcurementPlanDTO ProcurementPlan { get; set; }
        /// <summary>
        /// Procurement Initiated for IT-Contract. (Genanskaffelse igangsat)
        /// </summary>
        public YesNoUndecidedChoice? ProcurementInitiated { get; set; }
    }
}