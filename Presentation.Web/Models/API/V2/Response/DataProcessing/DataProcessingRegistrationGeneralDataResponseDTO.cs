using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Models.API.V2.Response.DataProcessing
{
    public class DataProcessingRegistrationGeneralDataResponseDTO
    {
        /// <summary>
        /// Optional data responsible selection
        /// </summary>
        public IdentityNamePairResponseDTO DataResponsible { get; set; }
        /// <summary>
        /// Additional remark related to the data responsible
        /// </summary>
        public string DataResponsibleRemark { get; set; }
        /// <summary>
        /// Determines if a data processing agreement has been concluded
        /// </summary>
        public YesNoIrrelevantChoice? IsAgreementConcluded { get; set; }
        /// <summary>
        /// Remark related to whether or not an agreement has been concluded
        /// </summary>
        public string IsAgreementConcludedRemark { get; set; }
        /// <summary>
        /// Describes the date when the data processing agreement was concluded
        /// </summary>
        public DateTime? AgreementConcludedAt { get; set; }
        /// <summary>
        /// Optional basis for transfer selection
        /// </summary>
        public IdentityNamePairResponseDTO BasisForTransfer { get; set; }
        /// <summary>
        /// Determines if the data processing includes transfer to insecure third countries
        /// </summary>
        public YesNoUndecidedChoice? TransferToInsecureThirdCountries { get; set; }
        /// <summary>
        /// Which insecure third countries are subject to data transfer as part of the data processing
        /// </summary>
        [Required]
        public IEnumerable<IdentityNamePairResponseDTO> InsecureCountriesSubjectToDataTransfer { get; set; }
        /// <summary>
        /// UUID's of the organization entities selected as data processors
        /// </summary>
        [Required]
        public IEnumerable<ShallowOrganizationResponseDTO> DataProcessors { get; set; }
        /// <summary>
        /// Determines if the data processing involves sub data processors
        /// </summary>
        public YesNoUndecidedChoice? HasSubDataProcessors { get; set; }
        /// <summary>
        /// UUID's of the organization entities selected as sub data processors
        /// </summary>
        [Required]
        public IEnumerable<DataProcessorRegistrationSubDataProcessorResponseDTO> SubDataProcessors { get; set; }

        /// <summary>
        /// Determines if the entity is considered valid. Validity is determined by
        /// - the MainContract Validity.
        /// </summary>
        [Required]
        public bool Valid { get; set; }
        /// <summary>
        /// Defines the master contract for this data processing registration (many contracts can point to a data processing registration but only one can be the master contract)
        /// </summary>
        public IdentityNamePairResponseDTO MainContract { get; set; }
        /// <summary>
        /// Defines the associated contracts with this data processing registration
        /// </summary>
        public IEnumerable<IdentityNamePairResponseDTO> AssociatedContracts { get; set; }
        /// <summary>
        /// The organization unit responsible for this data processing registration
        /// </summary>
        public IdentityNamePairResponseDTO ResponsibleOrganizationUnit { get; set; }

    }
}