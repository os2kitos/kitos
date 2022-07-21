using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Presentation.Web.Models.API.V1.ItContract
{
    public class ContractProcurementPlanDTO
    {
        public int? ProcurementPlanYear { get; set; }
        public int? ProcurementPlanQuarter { get; set; }
    }
}