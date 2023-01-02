using System;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Models.API.V2.Request.DataProcessing
{
    public class DataProcessorRegistrationSubDataProcessorWriteRequestDTO
    {
        /// <summary>
        /// The UUID of the data processor's organization in KITOS
        /// </summary>
        [Required]
        public Guid DataProcessorOrganizationUuid { get; set; }
        /// <summary>
        /// Optional reference to the uuid of the selected basis for transfer option
        /// </summary>
        public Guid? BasisForTransferUuid { get; set; }
        /// <summary>
        /// Optional info regarding whether transfer to insecure third countries occur
        /// </summary>
        public YesNoUndecidedChoice? TransferToInsecureThirdCountry { get; set; }
        /// <summary>
        /// Optional reference to the uuid a specific insecure country, which is subject to data processing
        /// </summary>
        public Guid? InsecureThirdCountrySubjectToDataProcessingUuid { get; set; }
    }
}