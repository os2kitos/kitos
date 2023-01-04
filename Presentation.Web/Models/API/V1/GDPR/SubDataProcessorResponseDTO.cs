using Core.DomainModel.Shared;

namespace Presentation.Web.Models.API.V1.GDPR
{
    public class SubDataProcessorResponseDTO : ShallowOrganizationDTO
    {
        public NamedEntityWithExpirationStatusDTO BasisForTransfer { get; set; }
        public YesNoUndecidedOption? TransferToInsecureThirdCountries { get; set; }
        public NamedEntityWithExpirationStatusDTO InsecureCountry { get; set; }

        public SubDataProcessorResponseDTO(
            int id,
            string name,
            string cvrNumber,
            NamedEntityWithExpirationStatusDTO basisForTransfer,
            YesNoUndecidedOption? transferToInsecureThirdCountries,
            NamedEntityWithExpirationStatusDTO insecureCountry) : base(id, name)
        {
            BasisForTransfer = basisForTransfer;
            TransferToInsecureThirdCountries = transferToInsecureThirdCountries;
            InsecureCountry = insecureCountry;
            CvrNumber = cvrNumber;
        }

        public SubDataProcessorResponseDTO()
        {
        }
    }
}