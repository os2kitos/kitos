using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Presentation.Web.Models.API.V1.Shared;

namespace Presentation.Web.Models.API.V1.ItContract
{
    public class ContractOptionsDTO
    {
        public IEnumerable<OptionWithDescriptionAndExpirationDTO> CriticalityOptions{ get; set; }
        public IEnumerable<OptionWithDescriptionAndExpirationDTO> ContractTypeOptions{ get; set; }
        public IEnumerable<OptionWithDescriptionAndExpirationDTO> ContractTemplateOptions { get; set; }
        public IEnumerable<OptionWithDescriptionAndExpirationDTO> PurchaseFormOptions { get; set; }
        public IEnumerable<OptionWithDescriptionAndExpirationDTO> ProcurementStrategyOptions { get; set; }
    }
}