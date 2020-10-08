using System.Collections.Generic;
using Presentation.Web.Models.Shared;

namespace Presentation.Web.Models.GDPR
{
    public class DataProcessingOptionsDTO
    {
        public IEnumerable<OptionWithDescriptionDTO> DataResponsibleOptions { get; set; }
        public IEnumerable<OptionWithDescriptionDTO> ThirdCountryOptions { get; set; }
        public IEnumerable<OptionWithDescriptionDTO> BasisForTransferOptions { get; set; }
        public IEnumerable<BusinessRoleDTO> Roles { get; set; }
        public IEnumerable<OptionWithDescriptionDTO> OversightOptions { get; set; }
    }
}