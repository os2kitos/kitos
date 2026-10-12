using Core.DomainModel.Shared;
using System.Diagnostics.CodeAnalysis;

namespace Presentation.Web.Models.API.V1.GDPR
{
    public class SubDataProcessorResponseDTO : ShallowOrganizationDTO
    {
        public NamedEntityWithExpirationStatusDTO? BasisForTransfer { get; set; }
        public YesNoUndecidedOption? TransferToInsecureThirdCountries { get; set; }
        public NamedEntityWithExpirationStatusDTO? InsecureCountry { get; set; }

        [SetsRequiredMembers]
        public SubDataProcessorResponseDTO(
            int id,
            string name,
            string cvrNumber,
            NamedEntityWithExpirationStatusDTO? basisForTransfer,
            YesNoUndecidedOption? transferToInsecureThirdCountries,
            NamedEntityWithExpirationStatusDTO? insecureCountry) : base(id, name, cvrNumber)
        {
            BasisForTransfer = basisForTransfer;
            TransferToInsecureThirdCountries = transferToInsecureThirdCountries;
            InsecureCountry = insecureCountry;
        }
    }
}