using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Presentation.Web.Models.API.V1.Shared;

namespace Presentation.Web.Models.API.V1.ItContract
{
    public class ContractOptionsDTO
    {
        public IEnumerable<OptionWithDescriptionDTO> CriticalityOptions{ get; set; }
    }
}