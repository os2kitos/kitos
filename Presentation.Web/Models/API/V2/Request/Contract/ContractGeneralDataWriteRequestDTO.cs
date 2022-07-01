using System;
using System.Collections.Generic;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Request.Generic.Validity;

namespace Presentation.Web.Models.API.V2.Request.Contract
{
    public class ContractGeneralDataWriteRequestDTO
    {
        /// <summary>
        /// User-assigned contract id
        /// </summary>
        public string ContractId { get; set; }
        /// <summary>
        /// Optionally assigned contract type
        /// Constraints:
        ///     - If changed from current state, the option type must be available in the organization
        /// </summary>
        [NonEmptyGuid]
        public Guid? ContractTypeUuid { get; set; }
        /// <summary>
        /// Optionally assigned contract template
        /// Constraints:
        ///     - If changed from current state, the option type must be available in the organization
        /// </summary>
        [NonEmptyGuid]
        public Guid? ContractTemplateUuid { get; set; }
        /// <summary>
        /// Optionally assigned agreement elements
        /// Constraints:
        ///     - If changed from current state, the option types must be available in the organization
        /// </summary>
        public IEnumerable<Guid> AgreementElementUuids { get; set; }

        public string Notes { get; set; }
        /// <summary>
        /// Validity of the it-contract
        /// </summary>
        public ValidityWriteRequestDTO Validity { get; set; }

        /// <summary>
        /// Optionally assigned criticality type
        /// Constraints:
        ///     - If changed from current state, the option type must be available in the organization
        /// </summary>
        [NonEmptyGuid]
        public Guid? CriticalityTypeUuid { get; set; }
    }
}