using System.Collections.Generic;
using Presentation.Web.Models.API.V1.Shared;

namespace Presentation.Web.Models.API.V1.GDPR
{
    public class DataProcessingOptionsDTO
    {
        public required IEnumerable<OptionWithDescriptionDTO> DataResponsibleOptions { get; set; }
        public required IEnumerable<OptionWithDescriptionDTO> ThirdCountryOptions { get; set; }
        public required IEnumerable<OptionWithDescriptionDTO> BasisForTransferOptions { get; set; }
        public required IEnumerable<DataProcessingBusinessRoleDTO> Roles { get; set; }
        public required IEnumerable<OptionWithDescriptionDTO> OversightOptions { get; set; }
    }
}