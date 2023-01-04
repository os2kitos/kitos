using System.ComponentModel.DataAnnotations;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Models.API.V2.Response.DataProcessing
{
    public class DataProcessorRegistrationSubDataProcessorResponseDTO
    {
        /// <summary>
        /// The data processor's organization in KITOS
        /// </summary>
        [Required]
        public ShallowOrganizationResponseDTO DataProcessorOrganization { get; set; }
        /// <summary>
        /// Optional reference to selected basis for transfer option
        /// </summary>
        public IdentityNamePairResponseDTO BasisForTransfer { get; set; }
        /// <summary>
        /// Optionally info regarding whether transfer to insecure third countries occur
        /// </summary>
        public YesNoUndecidedChoice? TransferToInsecureThirdCountry { get; set; }
        /// <summary>
        /// Optional reference to a specific insecure country, which is subject to data processing
        /// </summary>
        public IdentityNamePairResponseDTO InsecureThirdCountrySubjectToDataProcessing { get; set; }
    }
}