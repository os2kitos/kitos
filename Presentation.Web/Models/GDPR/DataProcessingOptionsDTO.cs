using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Presentation.Web.Models.Shared;

namespace Presentation.Web.Models.GDPR
{
    public class DataProcessingOptionsDTO
    {
        public IEnumerable<OptionWithDescriptionDTO> DataResponsibleOptions { get; set; }
        public IEnumerable<OptionWithDescriptionDTO> ThirdCountryOptions { get; set; }
        public IEnumerable<OptionWithDescriptionDTO> BasisForTransferOptions { get; set; }
    }
}