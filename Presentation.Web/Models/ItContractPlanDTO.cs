using System;
using System.Collections.Generic;

namespace Presentation.Web.Models
{
    public class ItContractPlanDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public IEnumerable<ItContractPlanDTO> Children { get; set; }
        
        //Second column
        public string ContractTypeName { get; set; }
        public string ContractTemplateName { get; set; }

        //Third column
        public string PurchaseFormName { get; set; }

        //Fourth column
        public DateTime? Concluded { get; set; }

        //Fifth column
        public int Duration { get; set; }
        
        //Sixth column
        public DateTime? ExpirationDate { get; set; }

        //Seventh column ("option")
        public string OptionExtendName { get; set; }
        public int ExtendMultiplier { get; set; }

        //Eighth column ("opsigelse")
        public string TerminationDeadlineName { get; set; }

        //Ninth
        public DateTime? IrrevocableTo { get; set; }

        //Tenth
        public string ProcurementStrategyName { get; set; }
       
        //Eleventh
        public int? ProcurementPlanHalf { get; set; }
        public int? ProcurementPlanYear { get; set; }

        /// <summary>
        /// When the contract ends (opsagt)
        /// </summary>
        public DateTime? Terminated { get; set; }

        /// <summary>
        /// Whether the contract is active or not
        /// </summary>
        public bool IsActive { get; set; }
    }
}
