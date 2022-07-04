using System.Collections.Generic;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Generic.Validity;

namespace Presentation.Web.Models.API.V2.Response.Contract
{
    public class ContractGeneralDataResponseDTO
    {
        /// <summary>
        /// User-assigned contract id
        /// </summary>
        public string ContractId { get; set; }
        /// <summary>
        /// Optionally assigned contract type
        /// </summary>
        [NonEmptyGuid]
        public IdentityNamePairResponseDTO ContractType { get; set; }
        /// <summary>
        /// Optionally assigned contract template
        /// </summary>
        [NonEmptyGuid]
        public IdentityNamePairResponseDTO ContractTemplate { get; set; }
        /// <summary>
        /// Optionally assigned agreement elements
        /// </summary>
        public IEnumerable<IdentityNamePairResponseDTO> AgreementElements { get; set; }
        public string Notes { get; set; }
        /// <summary>
        /// Validity of the it-contract
        /// </summary>
        public ValidityResponseDTO Validity { get; set; }
        /// <summary>
        /// Optionally assigned criticality type
        /// </summary>
        public IdentityNamePairResponseDTO CriticalityType { get; set; }
    }
}