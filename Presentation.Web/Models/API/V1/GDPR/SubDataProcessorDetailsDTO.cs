using Core.DomainModel.Shared;

namespace Presentation.Web.Models.API.V1.GDPR
{
    public class SubDataProcessorDetailsDTO
    {
        public int? BasisForTransferOptionId { get; }
        public YesNoUndecidedOption? TransferToInsecureThirdCountries { get; set; }
        public int? InsecureCountryOptionId { get; set; }
    }
}