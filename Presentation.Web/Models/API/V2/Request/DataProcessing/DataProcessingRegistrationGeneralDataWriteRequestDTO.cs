using System;
using System.Collections.Generic;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Models.API.V2.Request.DataProcessing
{
    public class DataProcessingRegistrationGeneralDataWriteRequestDTO
    {
        /// <summary>
        /// Optional data responsible selection
        /// Constraints:
        ///     - If changed from existing value, the option must currently be available in the organization
        /// </summary>
        [NonEmptyGuid]
        public Guid? DataResponsibleUuid { get; set; }
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
        /// Constraints:
        ///     - IsAgreementConcluded equals 'yes'
        /// </summary>
        public DateTime? AgreementConcludedAt { get; set; }
        /// <summary>
        /// Optional basis for transfer selection
        /// Constraints:
        ///     - If changed from existing value, the option must currently be available in the organization
        /// </summary>
        [NonEmptyGuid]
        public Guid? BasisForTransferUuid { get; set; }
        /// <summary>
        /// Determines if the data processing includes transfer to insecure third countries
        /// </summary>
        public YesNoUndecidedChoice? TransferToInsecureThirdCountries { get; set; }
        /// <summary>
        /// Which insecure third countries are subject to data transfer as part of the data processing
        /// Constraints:
        ///     - TransferToInsecureThirdCountries equals 'yes'
        ///     - Duplicates are not allowed
        ///     - If changed from existing value, the options must currently be available in the organization
        /// </summary>
        public IEnumerable<Guid> InsecureCountriesSubjectToDataTransferUuids { get; set; }
        /// <summary>
        /// UUID's of the organization entities selected as data processors
        /// Constraints:
        ///     - No duplicates
        /// </summary>
        public IEnumerable<Guid> DataProcessorUuids { get; set; }
        /// <summary>
        /// Determines if the data processing involves sub data processors
        /// </summary>
        public YesNoUndecidedChoice? HasSubDataProcessors { get; set; }
        /// <summary>
        /// Sub data processors
        /// Constraints:
        ///     - HasSubDataProcesors equals 'yes'
        ///     - No duplicates allowed
        /// </summary>
        public IEnumerable<DataProcessorRegistrationSubDataProcessorWriteRequestDTO> SubDataProcessors { get; set; }

        /// <summary>
        /// Defines the master contract for this Data Processing Registration (many contracts can point to a DPR but only one can be the master contract)
        /// Constraint: The contract provided MUST point to this Data Processing Registration for it to be selected as "main contract".
        /// </summary>
        [NonEmptyGuid]
        public Guid? MainContractUuid { get; set; }
        /// <summary>
        /// Defines the organization unit that is responsible for this Data Processing Registration
        /// </summary>
        [NonEmptyGuid]
        public Guid? ResponsibleOrganizationUnitUuid { get; set; }
    }
}