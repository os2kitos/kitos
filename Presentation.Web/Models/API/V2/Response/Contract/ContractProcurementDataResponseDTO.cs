using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Types.Contract;

namespace Presentation.Web.Models.API.V2.Response.Contract
{
    public class ContractProcurementDataResponseDTO
    {
        /// <summary>
        /// Optionally assigned purchase type
        /// </summary>
        [NonEmptyGuid]
        public IdentityNamePairResponseDTO PurchaseType { get; set; }
        /// <summary>
        /// Optionally assigned procurement strategy
        /// </summary>
        public IdentityNamePairResponseDTO ProcurementStrategy { get; set; }
        /// <summary>
        /// Procurement plan
        /// </summary>
        public ProcurementPlanDTO ProcurementPlan { get; set; }
    }
}